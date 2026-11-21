using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.Common.Interfaces;
using PharmacyManagement.Application.DTOs.Returns;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Application.Mappings;
using PharmacyManagement.Domain.Entities;
using PharmacyManagement.Domain.Enums;
using PharmacyManagement.Domain.Exceptions;

namespace PharmacyManagement.Application.Services;

public sealed class ReturnService : IReturnService
{
    private readonly IApplicationDbContext _db;
    private readonly IDocumentNumberService _documents;
    private readonly IAuditService _audit;
    private readonly ICurrentUserService _currentUser;

    public ReturnService(
        IApplicationDbContext db,
        IDocumentNumberService documents,
        IAuditService audit,
        ICurrentUserService currentUser)
    {
        _db = db;
        _documents = documents;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<ReturnDto>> GetPagedAsync(
        QueryParameters query, ReturnStatus? status, ReturnType? type, int? branchId,
        CancellationToken cancellationToken = default)
    {
        var returns = _db.Returns.AsNoTracking()
            .Include(r => r.Branch)
            .Include(r => r.OriginalSale)
            .Include(r => r.OriginalPurchaseOrder)
            .Include(r => r.Customer)
            .Include(r => r.Supplier)
            .Include(r => r.Details).ThenInclude(d => d.Medicine)
            .Include(r => r.Details).ThenInclude(d => d.Batch)
            .Include(r => r.Refunds)
            .AsQueryable();

        if (status.HasValue)
        {
            returns = returns.Where(r => r.Status == status);
        }

        if (type.HasValue)
        {
            returns = returns.Where(r => r.Type == type);
        }

        if (branchId.HasValue)
        {
            returns = returns.Where(r => r.BranchId == branchId);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            returns = returns.Where(r =>
                r.ReturnNumber.Contains(term) ||
                (r.OriginalSale != null && r.OriginalSale.InvoiceNumber.Contains(term)) ||
                (r.OriginalPurchaseOrder != null && r.OriginalPurchaseOrder.OrderNumber.Contains(term)) ||
                (r.Customer != null && (r.Customer.FirstName.Contains(term) || r.Customer.LastName.Contains(term))) ||
                (r.Supplier != null && r.Supplier.Name.Contains(term)));
        }

        var total = await returns.CountAsync(cancellationToken);
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 200);
        var items = await returns
            .OrderByDescending(r => r.RequestedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<ReturnDto>.Create(items.Select(r => r.ToDto()).ToList(), page, pageSize, total);
    }

    public async Task<ReturnDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var record = await _db.Returns.AsNoTracking()
            .Include(r => r.Branch)
            .Include(r => r.OriginalSale)
            .Include(r => r.OriginalPurchaseOrder)
            .Include(r => r.Customer)
            .Include(r => r.Supplier)
            .Include(r => r.Details).ThenInclude(d => d.Medicine)
            .Include(r => r.Details).ThenInclude(d => d.Batch)
            .Include(r => r.Refunds)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken)
            ?? throw new NotFoundException("Return", id);

        return record.ToDto();
    }

    public async Task<ReturnDto> CreateAsync(CreateReturnRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Type is not (ReturnType.SaleReturn or ReturnType.PurchaseReturn))
        {
            throw new BusinessRuleException("Invalid return type.");
        }

        if (!await _db.Branches.AnyAsync(b => b.Id == request.BranchId, cancellationToken))
        {
            throw new NotFoundException("Branch", request.BranchId);
        }

        if (request.Items.Count == 0)
        {
            throw new BusinessRuleException("At least one returned item is required.");
        }

        if (request.Items.Select(i => (i.MedicineId, i.BatchId)).Distinct().Count() != request.Items.Count)
        {
            throw new BusinessRuleException("The same medicine and batch appear more than once in the return.");
        }

        Sale? sale = null;
        PurchaseOrder? purchaseOrder = null;
        var pendingQuantities = new Dictionary<(int MedicineId, int BatchId), int>();
        var saleDetails = new Dictionary<(int MedicineId, int BatchId), SaleDetail>();
        var total = 0m;

        if (request.Type == ReturnType.SaleReturn)
        {
            if (!request.OriginalSaleId.HasValue)
            {
                throw new BusinessRuleException("The original sale is required for a sale return.");
            }

            sale = await _db.Sales
                .Include(s => s.Details)
                .FirstOrDefaultAsync(s => s.Id == request.OriginalSaleId, cancellationToken)
                ?? throw new NotFoundException("Sale", request.OriginalSaleId);

            if (sale.Status == SaleStatus.Cancelled)
            {
                throw new BusinessRuleException("Cancelled sales cannot be returned.");
            }

            if (sale.BranchId != request.BranchId)
            {
                throw new BusinessRuleException("The sale belongs to a different branch.");
            }

            if (request.CustomerId.HasValue && sale.CustomerId.HasValue && request.CustomerId != sale.CustomerId)
            {
                throw new BusinessRuleException("The customer does not match the original sale.");
            }

            foreach (var detail in sale.Details)
            {
                saleDetails[(detail.MedicineId, detail.BatchId)] = detail;
            }

            var pending = await _db.ReturnDetails.AsNoTracking()
                .Where(d => d.Return.OriginalSaleId == sale.Id && d.Return.Status == ReturnStatus.Pending)
                .Select(d => new { d.MedicineId, d.BatchId, d.Quantity })
                .ToListAsync(cancellationToken);

            foreach (var group in pending.GroupBy(p => (p.MedicineId, p.BatchId)))
            {
                pendingQuantities[group.Key] = group.Sum(x => x.Quantity);
            }
        }
        else
        {
            if (!request.OriginalPurchaseOrderId.HasValue)
            {
                throw new BusinessRuleException("The original purchase order is required for a purchase return.");
            }

            purchaseOrder = await _db.PurchaseOrders
                .FirstOrDefaultAsync(o => o.Id == request.OriginalPurchaseOrderId, cancellationToken)
                ?? throw new NotFoundException("PurchaseOrder", request.OriginalPurchaseOrderId);

            if (purchaseOrder.Status is not (PurchaseOrderStatus.Received or PurchaseOrderStatus.PartiallyReceived))
            {
                throw new BusinessRuleException("Only received purchase orders can be returned to the supplier.");
            }

            if (purchaseOrder.BranchId != request.BranchId)
            {
                throw new BusinessRuleException("The purchase order belongs to a different branch.");
            }
        }

        var returnRecord = new ReturnRecord
        {
            ReturnNumber = await _documents.NextAsync(DocumentType.Return, DateTime.UtcNow, cancellationToken),
            Type = request.Type,
            BranchId = request.BranchId,
            OriginalSaleId = request.OriginalSaleId,
            OriginalPurchaseOrderId = request.OriginalPurchaseOrderId,
            CustomerId = request.Type == ReturnType.SaleReturn ? request.CustomerId ?? sale?.CustomerId : null,
            SupplierId = request.Type == ReturnType.PurchaseReturn ? purchaseOrder?.SupplierId : null,
            Reason = request.Reason.Trim(),
            Status = ReturnStatus.Pending,
            RefundMethod = request.RefundMethod,
            Notes = request.Notes?.Trim(),
            RequestedByUserId = _currentUser.UserId ?? Guid.Empty,
            RequestedAtUtc = DateTime.UtcNow
        };

        foreach (var item in request.Items)
        {
            if (item.Quantity <= 0)
            {
                throw new BusinessRuleException("Returned quantities must be greater than zero.");
            }

            var batch = await _db.MedicineBatches.FirstOrDefaultAsync(b => b.Id == item.BatchId, cancellationToken)
                ?? throw new NotFoundException("MedicineBatch", item.BatchId);

            if (batch.MedicineId != item.MedicineId)
            {
                throw new BusinessRuleException($"Batch '{batch.BatchNumber}' does not belong to the selected medicine.");
            }

            if (batch.BranchId != request.BranchId)
            {
                throw new BusinessRuleException($"Batch '{batch.BatchNumber}' does not belong to this branch.");
            }

            decimal unitPrice;
            if (request.Type == ReturnType.SaleReturn)
            {
                if (!saleDetails.TryGetValue((item.MedicineId, item.BatchId), out var saleDetail))
                {
                    throw new BusinessRuleException($"Batch '{batch.BatchNumber}' was not sold on invoice {sale?.InvoiceNumber}.");
                }

                var alreadyReturned = saleDetail.ReturnedQuantity + pendingQuantities.GetValueOrDefault((item.MedicineId, item.BatchId));
                var available = saleDetail.Quantity - alreadyReturned;
                if (item.Quantity > available)
                {
                    throw new BusinessRuleException($"Only {Math.Max(0, available)} units of batch '{batch.BatchNumber}' can be returned.");
                }

                unitPrice = saleDetail.UnitPrice;
            }
            else
            {
                if (item.Quantity > batch.CurrentQuantity)
                {
                    throw new BusinessRuleException($"Only {batch.CurrentQuantity} units are available in batch '{batch.BatchNumber}'.");
                }

                if (batch.SupplierId.HasValue && purchaseOrder is not null && batch.SupplierId != purchaseOrder.SupplierId)
                {
                    throw new BusinessRuleException($"Batch '{batch.BatchNumber}' was not supplied by this purchase order's supplier.");
                }

                unitPrice = item.UnitPrice ?? batch.PurchasePrice;
            }

            var lineTotal = SaleCalculator.Round(item.Quantity * unitPrice);
            total += lineTotal;

            returnRecord.Details.Add(new ReturnDetail
            {
                MedicineId = item.MedicineId,
                BatchId = item.BatchId,
                Quantity = item.Quantity,
                UnitPrice = unitPrice,
                LineTotal = lineTotal,
                Condition = item.Condition,
                RestockDecision = item.RestockDecision,
                Notes = item.Notes?.Trim()
            });
        }

        returnRecord.TotalAmount = SaleCalculator.Round(total);
        _db.Returns.Add(returnRecord);
        await _db.SaveChangesAsync(cancellationToken);
        var dto = await GetByIdAsync(returnRecord.Id, cancellationToken);
        await _audit.LogAsync("ReturnCreated", nameof(ReturnRecord), returnRecord.Id.ToString(), null, dto, cancellationToken);
        return dto;
    }

    public async Task<ReturnDto> ApproveAsync(int id, ApproveReturnRequest request, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var record = await _db.Returns
                .Include(r => r.Details)
                .Include(r => r.OriginalSale).ThenInclude(s => s!.Details)
                .FirstOrDefaultAsync(r => r.Id == id, cancellationToken)
                ?? throw new NotFoundException("Return", id);

            if (record.Status != ReturnStatus.Pending)
            {
                throw new BusinessRuleException($"Only pending returns can be approved. Current status: {record.Status}.");
            }

            var now = DateTime.UtcNow;
            var inspections = request.Inspections?.ToDictionary(i => i.ReturnDetailId)
                ?? new Dictionary<int, ReturnInspectionItem>();

            foreach (var detail in record.Details)
            {
                if (inspections.TryGetValue(detail.Id, out var inspection))
                {
                    detail.Condition = inspection.Condition;
                    detail.RestockDecision = inspection.RestockDecision;
                    if (!string.IsNullOrWhiteSpace(inspection.Notes))
                    {
                        detail.Notes = inspection.Notes.Trim();
                    }
                }

                if (detail.RestockDecision == RestockDecision.PendingInspection)
                {
                    detail.RestockDecision = detail.Condition == ReturnCondition.Resalable
                        ? RestockDecision.Restock
                        : RestockDecision.Discard;
                }

                detail.InspectedByUserId = _currentUser.UserId;
                detail.InspectedAtUtc = now;

                var batch = await _db.MedicineBatches.FirstOrDefaultAsync(b => b.Id == detail.BatchId, cancellationToken)
                    ?? throw new NotFoundException("MedicineBatch", detail.BatchId);

                if (record.Type == ReturnType.SaleReturn)
                {
                    if (batch.ExpiryDate.Date <= now.Date)
                    {
                        detail.RestockDecision = RestockDecision.Discard;
                    }

                    if (detail.RestockDecision == RestockDecision.Restock)
                    {
                        var before = batch.CurrentQuantity;
                        batch.CurrentQuantity += detail.Quantity;
                        _db.InventoryTransactions.Add(new InventoryTransaction
                        {
                            MedicineId = detail.MedicineId,
                            Batch = batch,
                            BatchId = batch.Id,
                            BranchId = record.BranchId,
                            Type = InventoryTransactionType.SaleReturn,
                            QuantityChange = detail.Quantity,
                            QuantityBefore = before,
                            QuantityAfter = batch.CurrentQuantity,
                            UnitCost = detail.UnitPrice,
                            ReferenceType = nameof(ReturnRecord),
                            ReferenceId = record.Id,
                            Notes = $"Sale return {record.ReturnNumber}",
                            PerformedByUserId = _currentUser.UserId,
                            CreatedAtUtc = now
                        });
                    }

                    if (record.OriginalSale is not null)
                    {
                        var saleDetail = record.OriginalSale.Details
                            .FirstOrDefault(d => d.MedicineId == detail.MedicineId && d.BatchId == detail.BatchId);
                        if (saleDetail is not null)
                        {
                            saleDetail.ReturnedQuantity += detail.Quantity;
                        }
                    }
                }
                else
                {
                    if (batch.CurrentQuantity < detail.Quantity)
                    {
                        throw new BusinessRuleException($"Insufficient stock in batch '{batch.BatchNumber}' for the purchase return.");
                    }

                    var before = batch.CurrentQuantity;
                    batch.CurrentQuantity -= detail.Quantity;
                    _db.InventoryTransactions.Add(new InventoryTransaction
                    {
                        MedicineId = detail.MedicineId,
                        Batch = batch,
                        BatchId = batch.Id,
                        BranchId = record.BranchId,
                        Type = InventoryTransactionType.PurchaseReturn,
                        QuantityChange = -detail.Quantity,
                        QuantityBefore = before,
                        QuantityAfter = batch.CurrentQuantity,
                        UnitCost = detail.UnitPrice,
                        ReferenceType = nameof(ReturnRecord),
                        ReferenceId = record.Id,
                        Notes = $"Purchase return {record.ReturnNumber}",
                        PerformedByUserId = _currentUser.UserId,
                        CreatedAtUtc = now
                    });
                }
            }

            if (record.OriginalSale is not null)
            {
                var sale = record.OriginalSale;
                sale.Status = sale.Details.All(d => d.ReturnedQuantity >= d.Quantity)
                    ? SaleStatus.Returned
                    : SaleStatus.PartiallyReturned;

                var alreadyRefunded = await _db.Refunds
                    .Where(r => r.Return.OriginalSaleId == sale.Id)
                    .SumAsync(r => (decimal?)r.Amount, cancellationToken) ?? 0m;

                var refundable = Math.Max(0, sale.PaidAmount - alreadyRefunded);
                var refundAmount = Math.Min(record.TotalAmount, refundable);
                if (refundAmount > 0)
                {
                    _db.Refunds.Add(new Refund
                    {
                        ReturnId = record.Id,
                        Amount = SaleCalculator.Round(refundAmount),
                        Method = request.RefundMethod ?? record.RefundMethod ?? PaymentMethod.Cash,
                        RefundedAtUtc = now,
                        ProcessedByUserId = _currentUser.UserId ?? Guid.Empty,
                        Notes = request.Notes?.Trim()
                    });
                }
            }

            record.Status = ReturnStatus.Approved;
            record.ApprovedByUserId = _currentUser.UserId;
            record.ApprovedAtUtc = now;
            if (!string.IsNullOrWhiteSpace(request.Notes))
            {
                record.Notes = request.Notes.Trim();
            }

            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            var dto = await GetByIdAsync(id, cancellationToken);
            await _audit.LogAsync("ReturnApproved", nameof(ReturnRecord), id.ToString(), null, new { record.ReturnNumber, record.TotalAmount }, cancellationToken);
            return dto;
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new ConflictException("Stock changed while approving the return. Please retry.");
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<ReturnDto> RejectAsync(int id, string reason, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new BusinessRuleException("A rejection reason is required.");
        }

        var record = await _db.Returns.FirstOrDefaultAsync(r => r.Id == id, cancellationToken)
            ?? throw new NotFoundException("Return", id);

        if (record.Status != ReturnStatus.Pending)
        {
            throw new BusinessRuleException($"Only pending returns can be rejected. Current status: {record.Status}.");
        }

        record.Status = ReturnStatus.Rejected;
        record.RejectionReason = reason.Trim();
        record.ApprovedByUserId = _currentUser.UserId;
        record.ApprovedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("ReturnRejected", nameof(ReturnRecord), id.ToString(), null, new { Reason = record.RejectionReason }, cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }
}
