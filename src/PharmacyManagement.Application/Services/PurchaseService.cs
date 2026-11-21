using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.Common.Interfaces;
using PharmacyManagement.Application.DTOs.Purchasing;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Application.Mappings;
using PharmacyManagement.Domain.Entities;
using PharmacyManagement.Domain.Enums;
using PharmacyManagement.Domain.Exceptions;

namespace PharmacyManagement.Application.Services;

public sealed class PurchaseService : IPurchaseService
{
    private readonly IApplicationDbContext _db;
    private readonly IDocumentNumberService _documents;
    private readonly IAuditService _audit;
    private readonly ICurrentUserService _currentUser;

    public PurchaseService(
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

    public async Task<PagedResult<PurchaseOrderListItemDto>> GetPagedAsync(
        QueryParameters query, PurchaseOrderStatus? status, int? supplierId, DateTime? from, DateTime? to,
        CancellationToken cancellationToken = default)
    {
        var orders = _db.PurchaseOrders.AsNoTracking();

        if (status.HasValue)
        {
            orders = orders.Where(o => o.Status == status);
        }

        if (supplierId.HasValue)
        {
            orders = orders.Where(o => o.SupplierId == supplierId);
        }

        if (from.HasValue)
        {
            var fromUtc = from.Value.Date;
            orders = orders.Where(o => o.OrderDateUtc >= fromUtc);
        }

        if (to.HasValue)
        {
            var toUtc = to.Value.Date.AddDays(1);
            orders = orders.Where(o => o.OrderDateUtc < toUtc);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            orders = orders.Where(o => o.OrderNumber.Contains(term) || o.Supplier.Name.Contains(term));
        }

        var total = await orders.CountAsync(cancellationToken);
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 200);
        var items = await orders
            .OrderByDescending(o => o.OrderDateUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new PurchaseOrderListItemDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                SupplierName = o.Supplier.Name,
                BranchName = o.Branch.Name,
                OrderDateUtc = o.OrderDateUtc,
                ExpectedDeliveryDate = o.ExpectedDeliveryDate,
                Status = o.Status,
                TotalAmount = o.TotalAmount,
                ItemCount = o.Details.Count
            })
            .ToListAsync(cancellationToken);

        return PagedResult<PurchaseOrderListItemDto>.Create(items, page, pageSize, total);
    }

    public async Task<PurchaseOrderDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await _db.PurchaseOrders.AsNoTracking()
            .Include(o => o.Supplier)
            .Include(o => o.Branch)
            .Include(o => o.Details).ThenInclude(d => d.Medicine)
            .Include(o => o.GoodsReceipts)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken)
            ?? throw new NotFoundException("PurchaseOrder", id);

        return order.ToDto();
    }

    public async Task<PurchaseOrderDto> CreateAsync(CreatePurchaseOrderRequest request, CancellationToken cancellationToken = default)
    {
        var (medicines, subTotal, taxAmount, totalAmount) = await ValidateAndCalculateAsync(request, cancellationToken);
        var now = DateTime.UtcNow;

        var order = new PurchaseOrder
        {
            OrderNumber = await _documents.NextAsync(DocumentType.PurchaseOrder, now, cancellationToken),
            SupplierId = request.SupplierId,
            BranchId = request.BranchId,
            OrderDateUtc = now,
            ExpectedDeliveryDate = request.ExpectedDeliveryDate,
            Status = PurchaseOrderStatus.Draft,
            SubTotal = subTotal,
            TaxAmount = taxAmount,
            DiscountAmount = request.DiscountAmount,
            TotalAmount = totalAmount,
            Notes = request.Notes?.Trim(),
            CreatedByUserId = _currentUser.UserId ?? Guid.Empty,
            CreatedAtUtc = now
        };

        foreach (var item in request.Items)
        {
            var lineSubTotal = decimal.Round(item.OrderedQuantity * item.UnitPrice, 2, MidpointRounding.AwayFromZero);
            var lineTax = decimal.Round(lineSubTotal * item.TaxPercentage / 100m, 2, MidpointRounding.AwayFromZero);
            order.Details.Add(new PurchaseOrderDetail
            {
                MedicineId = item.MedicineId,
                OrderedQuantity = item.OrderedQuantity,
                UnitPrice = item.UnitPrice,
                TaxPercentage = item.TaxPercentage,
                LineTotal = lineSubTotal + lineTax,
                BatchNumber = item.BatchNumber?.Trim(),
                ManufacturingDate = item.ManufacturingDate?.Date,
                ExpiryDate = item.ExpiryDate?.Date
            });
        }

        _db.PurchaseOrders.Add(order);
        await _db.SaveChangesAsync(cancellationToken);
        var dto = await GetByIdAsync(order.Id, cancellationToken);
        await _audit.LogAsync("PurchaseOrderCreated", nameof(PurchaseOrder), order.Id.ToString(), null, dto, cancellationToken);
        return dto;
    }

    public async Task<PurchaseOrderDto> UpdateAsync(int id, CreatePurchaseOrderRequest request, CancellationToken cancellationToken = default)
    {
        var order = await _db.PurchaseOrders
            .Include(o => o.Details)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken)
            ?? throw new NotFoundException("PurchaseOrder", id);

        if (order.Status != PurchaseOrderStatus.Draft)
        {
            throw new BusinessRuleException($"Only draft purchase orders can be edited. Current status: {order.Status}.");
        }

        var (_, subTotal, taxAmount, totalAmount) = await ValidateAndCalculateAsync(request, cancellationToken);
        var before = await GetByIdAsync(id, cancellationToken);

        order.SupplierId = request.SupplierId;
        order.BranchId = request.BranchId;
        order.ExpectedDeliveryDate = request.ExpectedDeliveryDate;
        order.SubTotal = subTotal;
        order.TaxAmount = taxAmount;
        order.DiscountAmount = request.DiscountAmount;
        order.TotalAmount = totalAmount;
        order.Notes = request.Notes?.Trim();

        _db.PurchaseOrderDetails.RemoveRange(order.Details);
        order.Details.Clear();

        foreach (var item in request.Items)
        {
            var lineSubTotal = decimal.Round(item.OrderedQuantity * item.UnitPrice, 2, MidpointRounding.AwayFromZero);
            var lineTax = decimal.Round(lineSubTotal * item.TaxPercentage / 100m, 2, MidpointRounding.AwayFromZero);
            order.Details.Add(new PurchaseOrderDetail
            {
                PurchaseOrderId = order.Id,
                MedicineId = item.MedicineId,
                OrderedQuantity = item.OrderedQuantity,
                UnitPrice = item.UnitPrice,
                TaxPercentage = item.TaxPercentage,
                LineTotal = lineSubTotal + lineTax,
                BatchNumber = item.BatchNumber?.Trim(),
                ManufacturingDate = item.ManufacturingDate?.Date,
                ExpiryDate = item.ExpiryDate?.Date
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
        var after = await GetByIdAsync(id, cancellationToken);
        await _audit.LogAsync("PurchaseOrderUpdated", nameof(PurchaseOrder), id.ToString(), before, after, cancellationToken);
        return after;
    }

    public async Task<PurchaseOrderDto> SubmitAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await LoadForTransitionAsync(id, cancellationToken);
        if (order.Status != PurchaseOrderStatus.Draft)
        {
            throw new BusinessRuleException("Only draft purchase orders can be submitted for approval.");
        }

        order.Status = PurchaseOrderStatus.PendingApproval;
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("PurchaseOrderSubmitted", nameof(PurchaseOrder), id.ToString(), null, new { Status = order.Status.ToString() }, cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<PurchaseOrderDto> ApproveAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await LoadForTransitionAsync(id, cancellationToken);
        if (order.Status != PurchaseOrderStatus.PendingApproval)
        {
            throw new BusinessRuleException("Only purchase orders pending approval can be approved.");
        }

        order.Status = PurchaseOrderStatus.Approved;
        order.ApprovedByUserId = _currentUser.UserId;
        order.ApprovedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("PurchaseOrderApproved", nameof(PurchaseOrder), id.ToString(), null, new { Status = order.Status.ToString() }, cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<PurchaseOrderDto> RejectAsync(int id, string reason, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new BusinessRuleException("A rejection reason is required.");
        }

        var order = await LoadForTransitionAsync(id, cancellationToken);
        if (order.Status != PurchaseOrderStatus.PendingApproval)
        {
            throw new BusinessRuleException("Only purchase orders pending approval can be rejected.");
        }

        order.Status = PurchaseOrderStatus.Rejected;
        order.RejectionReason = reason.Trim();
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("PurchaseOrderRejected", nameof(PurchaseOrder), id.ToString(), null, new { Reason = order.RejectionReason }, cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<PurchaseOrderDto> CancelAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await _db.PurchaseOrders.Include(o => o.GoodsReceipts).FirstOrDefaultAsync(o => o.Id == id, cancellationToken)
            ?? throw new NotFoundException("PurchaseOrder", id);

        if (order.Status is PurchaseOrderStatus.Received or PurchaseOrderStatus.PartiallyReceived)
        {
            throw new BusinessRuleException("Received purchase orders cannot be cancelled.");
        }

        if (order.Status is PurchaseOrderStatus.Cancelled or PurchaseOrderStatus.Rejected)
        {
            throw new BusinessRuleException($"Purchase order is already {order.Status}.");
        }

        if (order.GoodsReceipts.Count > 0)
        {
            throw new BusinessRuleException("This purchase order has receipts and cannot be cancelled.");
        }

        order.Status = PurchaseOrderStatus.Cancelled;
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("PurchaseOrderCancelled", nameof(PurchaseOrder), id.ToString(), null, new { Status = order.Status.ToString() }, cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<GoodsReceiptDto> ReceiveAsync(int id, ReceivePurchaseOrderRequest request, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var order = await _db.PurchaseOrders
                .Include(o => o.Details).ThenInclude(d => d.Medicine)
                .FirstOrDefaultAsync(o => o.Id == id, cancellationToken)
                ?? throw new NotFoundException("PurchaseOrder", id);

            if (order.Status is not (PurchaseOrderStatus.Approved or PurchaseOrderStatus.PartiallyReceived))
            {
                throw new BusinessRuleException($"Only approved purchase orders can receive stock. Current status: {order.Status}.");
            }

            if (request.Items.Count == 0)
            {
                throw new BusinessRuleException("At least one received item is required.");
            }

            if (request.Items.Select(i => i.PurchaseOrderDetailId).Distinct().Count() != request.Items.Count)
            {
                throw new BusinessRuleException("The same purchase order line appears more than once.");
            }

            var now = DateTime.UtcNow;
            var receipt = new GoodsReceipt
            {
                ReceiptNumber = await _documents.NextAsync(DocumentType.GoodsReceipt, now, cancellationToken),
                PurchaseOrderId = order.Id,
                ReceivedAtUtc = now,
                ReceivedByUserId = _currentUser.UserId ?? Guid.Empty,
                Notes = request.Notes?.Trim()
            };

            var pendingTransactions = new List<(GoodsReceiptDetail Detail, int Before, int After, decimal UnitCost)>();

            foreach (var item in request.Items)
            {
                var detail = order.Details.FirstOrDefault(d => d.Id == item.PurchaseOrderDetailId)
                    ?? throw new BusinessRuleException($"Purchase order line {item.PurchaseOrderDetailId} does not belong to this order.");

                var pending = detail.OrderedQuantity - detail.ReceivedQuantity;
                if (item.Quantity > pending)
                {
                    throw new BusinessRuleException($"Only {pending} units are pending for {detail.Medicine?.Name ?? "this item"}.");
                }

                if (item.ExpiryDate.Date <= now.Date)
                {
                    throw new BusinessRuleException("Expired stock cannot be received.");
                }

                if (item.ExpiryDate.Date <= item.ManufacturingDate.Date)
                {
                    throw new BusinessRuleException("Expiry date must be after the manufacturing date.");
                }

                var batchNumber = item.BatchNumber.Trim();
                var unitCost = item.UnitCost ?? detail.UnitPrice;

                var batch = await _db.MedicineBatches.FirstOrDefaultAsync(b =>
                    b.BranchId == order.BranchId &&
                    b.MedicineId == detail.MedicineId &&
                    b.BatchNumber == batchNumber, cancellationToken);

                int before;
                if (batch is not null)
                {
                    if (batch.ExpiryDate.Date != item.ExpiryDate.Date)
                    {
                        throw new ConflictException($"Batch '{batchNumber}' already exists with a different expiry date.");
                    }

                    before = batch.CurrentQuantity;
                    batch.CurrentQuantity += item.Quantity;
                    batch.PurchasePrice = unitCost;
                    batch.IsActive = true;
                }
                else
                {
                    before = 0;
                    batch = new MedicineBatch
                    {
                        MedicineId = detail.MedicineId,
                        BranchId = order.BranchId,
                        SupplierId = order.SupplierId,
                        BatchNumber = batchNumber,
                        ManufacturingDate = item.ManufacturingDate.Date,
                        ExpiryDate = item.ExpiryDate.Date,
                        PurchasePrice = unitCost,
                        SellingPrice = detail.Medicine?.SellingPrice ?? 0,
                        InitialQuantity = item.Quantity,
                        CurrentQuantity = item.Quantity,
                        ReceivedAtUtc = now,
                        IsActive = true
                    };
                    _db.MedicineBatches.Add(batch);
                }

                var lineTotal = decimal.Round(item.Quantity * unitCost, 2, MidpointRounding.AwayFromZero);
                receipt.Details.Add(new GoodsReceiptDetail
                {
                    MedicineId = detail.MedicineId,
                    Batch = batch,
                    BatchNumber = batchNumber,
                    ManufacturingDate = item.ManufacturingDate.Date,
                    ExpiryDate = item.ExpiryDate.Date,
                    Quantity = item.Quantity,
                    UnitCost = unitCost,
                    LineTotal = lineTotal
                });

                detail.ReceivedQuantity += item.Quantity;
                receipt.TotalAmount += lineTotal;
                pendingTransactions.Add((receipt.Details.Last(), before, before + item.Quantity, unitCost));
            }

            _db.GoodsReceipts.Add(receipt);
            await _db.SaveChangesAsync(cancellationToken);

            foreach (var (detail, before, after, unitCost) in pendingTransactions)
            {
                _db.InventoryTransactions.Add(new InventoryTransaction
                {
                    MedicineId = detail.MedicineId,
                    Batch = detail.Batch,
                    BatchId = detail.BatchId,
                    BranchId = order.BranchId,
                    Type = InventoryTransactionType.PurchaseReceipt,
                    QuantityChange = detail.Quantity,
                    QuantityBefore = before,
                    QuantityAfter = after,
                    UnitCost = unitCost,
                    ReferenceType = nameof(GoodsReceipt),
                    ReferenceId = receipt.Id,
                    Notes = $"Goods receipt {receipt.ReceiptNumber}",
                    PerformedByUserId = _currentUser.UserId,
                    CreatedAtUtc = now
                });
            }

            order.Status = order.Details.All(d => d.ReceivedQuantity >= d.OrderedQuantity)
                ? PurchaseOrderStatus.Received
                : PurchaseOrderStatus.PartiallyReceived;

            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            var dto = await GetReceiptByIdAsync(receipt.Id, cancellationToken);
            await _audit.LogAsync("GoodsReceived", nameof(GoodsReceipt), receipt.Id.ToString(), null, dto, cancellationToken);
            return dto;
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new ConflictException("Stock changed while receiving the order. Refresh and try again.");
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<PagedResult<GoodsReceiptDto>> GetReceiptsAsync(QueryParameters query, int? purchaseOrderId, CancellationToken cancellationToken = default)
    {
        var receipts = _db.GoodsReceipts.AsNoTracking()
            .Include(r => r.PurchaseOrder).ThenInclude(o => o.Supplier)
            .Include(r => r.PurchaseOrder).ThenInclude(o => o.Branch)
            .Include(r => r.Details).ThenInclude(d => d.Medicine)
            .AsQueryable();

        if (purchaseOrderId.HasValue)
        {
            receipts = receipts.Where(r => r.PurchaseOrderId == purchaseOrderId);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            receipts = receipts.Where(r =>
                r.ReceiptNumber.Contains(term) ||
                r.PurchaseOrder.OrderNumber.Contains(term) ||
                r.PurchaseOrder.Supplier.Name.Contains(term));
        }

        var total = await receipts.CountAsync(cancellationToken);
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 200);
        var items = await receipts
            .OrderByDescending(r => r.ReceivedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<GoodsReceiptDto>.Create(items.Select(r => r.ToDto()).ToList(), page, pageSize, total);
    }

    public async Task<GoodsReceiptDto> GetReceiptByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var receipt = await _db.GoodsReceipts.AsNoTracking()
            .Include(r => r.PurchaseOrder).ThenInclude(o => o.Supplier)
            .Include(r => r.PurchaseOrder).ThenInclude(o => o.Branch)
            .Include(r => r.Details).ThenInclude(d => d.Medicine)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken)
            ?? throw new NotFoundException("GoodsReceipt", id);

        return receipt.ToDto();
    }

    private async Task<PurchaseOrder> LoadForTransitionAsync(int id, CancellationToken cancellationToken)
    {
        return await _db.PurchaseOrders.FirstOrDefaultAsync(o => o.Id == id, cancellationToken)
            ?? throw new NotFoundException("PurchaseOrder", id);
    }

    private async Task<(Dictionary<int, Medicine> Medicines, decimal SubTotal, decimal TaxAmount, decimal TotalAmount)> ValidateAndCalculateAsync(
        CreatePurchaseOrderRequest request, CancellationToken cancellationToken)
    {
        if (!await _db.Suppliers.AnyAsync(s => s.Id == request.SupplierId && s.IsActive, cancellationToken))
        {
            throw new NotFoundException("Active supplier", request.SupplierId);
        }

        if (!await _db.Branches.AnyAsync(b => b.Id == request.BranchId, cancellationToken))
        {
            throw new NotFoundException("Branch", request.BranchId);
        }

        if (request.Items.Count == 0)
        {
            throw new BusinessRuleException("At least one item is required.");
        }

        if (request.Items.Select(i => i.MedicineId).Distinct().Count() != request.Items.Count)
        {
            throw new BusinessRuleException("The same medicine appears more than once in the purchase order.");
        }

        var medicineIds = request.Items.Select(i => i.MedicineId).ToList();
        var medicines = await _db.Medicines.Where(m => medicineIds.Contains(m.Id)).ToDictionaryAsync(m => m.Id, cancellationToken);

        foreach (var item in request.Items)
        {
            if (!medicines.ContainsKey(item.MedicineId))
            {
                throw new NotFoundException("Medicine", item.MedicineId);
            }

            if (item.OrderedQuantity <= 0)
            {
                throw new BusinessRuleException("Ordered quantity must be greater than zero.");
            }

            if (item.UnitPrice < 0)
            {
                throw new BusinessRuleException("Unit price cannot be negative.");
            }

            if (item.TaxPercentage is < 0 or > 100)
            {
                throw new BusinessRuleException("Tax percentage must be between 0 and 100.");
            }

            if (item.ManufacturingDate.HasValue && item.ExpiryDate.HasValue && item.ExpiryDate.Value.Date <= item.ManufacturingDate.Value.Date)
            {
                throw new BusinessRuleException("Expiry date must be after the manufacturing date.");
            }

            if (item.ExpiryDate.HasValue && item.ExpiryDate.Value.Date <= DateTime.UtcNow.Date)
            {
                throw new BusinessRuleException("Expiry date must be in the future.");
            }
        }

        var subTotal = request.Items.Sum(i => decimal.Round(i.OrderedQuantity * i.UnitPrice, 2, MidpointRounding.AwayFromZero));
        var taxAmount = request.Items.Sum(i => decimal.Round(decimal.Round(i.OrderedQuantity * i.UnitPrice, 2, MidpointRounding.AwayFromZero) * i.TaxPercentage / 100m, 2, MidpointRounding.AwayFromZero));
        var totalAmount = subTotal + taxAmount - request.DiscountAmount;

        if (request.DiscountAmount < 0)
        {
            throw new BusinessRuleException("Discount cannot be negative.");
        }

        if (totalAmount < 0)
        {
            throw new BusinessRuleException("Discount cannot exceed the order total.");
        }

        return (medicines, subTotal, taxAmount, totalAmount);
    }
}
