using System.Data;
using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.Common.Interfaces;
using PharmacyManagement.Application.DTOs.Sales;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Application.Mappings;
using PharmacyManagement.Domain.Entities;
using PharmacyManagement.Domain.Enums;
using PharmacyManagement.Domain.Exceptions;

namespace PharmacyManagement.Application.Services;

public sealed class SaleService : ISaleService
{
    private readonly IApplicationDbContext _db;
    private readonly IDocumentNumberService _documents;
    private readonly IAuditService _audit;
    private readonly ICurrentUserService _currentUser;

    public SaleService(
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

    public async Task<PagedResult<SaleListItemDto>> GetPagedAsync(
        QueryParameters query, DateTime? from, DateTime? to, int? customerId, int? branchId,
        SaleStatus? status, CancellationToken cancellationToken = default)
    {
        var sales = _db.Sales.AsNoTracking();

        if (from.HasValue)
        {
            var fromUtc = from.Value.Date;
            sales = sales.Where(s => s.SaleDateUtc >= fromUtc);
        }

        if (to.HasValue)
        {
            var toUtc = to.Value.Date.AddDays(1);
            sales = sales.Where(s => s.SaleDateUtc < toUtc);
        }

        if (customerId.HasValue)
        {
            sales = sales.Where(s => s.CustomerId == customerId);
        }

        if (branchId.HasValue)
        {
            sales = sales.Where(s => s.BranchId == branchId);
        }

        if (status.HasValue)
        {
            sales = sales.Where(s => s.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            sales = sales.Where(s =>
                s.InvoiceNumber.Contains(term) ||
                (s.Customer != null && (s.Customer.FirstName.Contains(term) || s.Customer.LastName.Contains(term))));
        }

        var total = await sales.CountAsync(cancellationToken);
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 200);

        var rows = await sales
            .OrderByDescending(s => s.SaleDateUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new
            {
                s.Id,
                s.InvoiceNumber,
                s.SaleDateUtc,
                CustomerName = s.Customer != null ? s.Customer.FirstName + " " + s.Customer.LastName : null,
                s.CashierUserId,
                s.TotalAmount,
                s.PaidAmount,
                s.Status,
                s.PaymentStatus,
                ItemCount = s.Details.Count
            })
            .ToListAsync(cancellationToken);

        var cashierIds = rows.Select(r => r.CashierUserId).Distinct().ToList();
        var cashiers = await _db.Users.AsNoTracking()
            .Where(u => cashierIds.Contains(u.Id))
            .Select(u => new { u.Id, Name = u.FirstName + " " + u.LastName })
            .ToDictionaryAsync(u => u.Id, u => u.Name, cancellationToken);

        var items = rows.Select(r => new SaleListItemDto
        {
            Id = r.Id,
            InvoiceNumber = r.InvoiceNumber,
            SaleDateUtc = r.SaleDateUtc,
            CustomerName = r.CustomerName,
            CashierName = cashiers.GetValueOrDefault(r.CashierUserId),
            TotalAmount = r.TotalAmount,
            PaidAmount = r.PaidAmount,
            Status = r.Status,
            PaymentStatus = r.PaymentStatus,
            ItemCount = r.ItemCount
        }).ToList();

        return PagedResult<SaleListItemDto>.Create(items, page, pageSize, total);
    }

    public async Task<SaleDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var sale = await _db.Sales.AsNoTracking()
            .Include(s => s.Branch)
            .Include(s => s.Customer)
            .Include(s => s.Prescription)
            .Include(s => s.Details).ThenInclude(d => d.Medicine)
            .Include(s => s.Payments)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken)
            ?? throw new NotFoundException("Sale", id);

        var dto = sale.ToDto();
        dto.CashierName = await _db.Users.AsNoTracking()
            .Where(u => u.Id == sale.CashierUserId)
            .Select(u => u.FirstName + " " + u.LastName)
            .FirstOrDefaultAsync(cancellationToken);
        return dto;
    }

    public async Task<SaleDto> CreateAsync(CreateSaleRequest request, CancellationToken cancellationToken = default)
    {
        if (!await _db.Branches.AnyAsync(b => b.Id == request.BranchId, cancellationToken))
        {
            throw new NotFoundException("Branch", request.BranchId);
        }

        if (request.Items.Count == 0)
        {
            throw new BusinessRuleException("The cart is empty.");
        }

        if (request.Items.Select(i => i.MedicineId).Distinct().Count() != request.Items.Count)
        {
            throw new BusinessRuleException("The same medicine appears more than once in the sale.");
        }

        if (request.Payments.Any(p => p.Method == PaymentMethod.Credit))
        {
            throw new BusinessRuleException("Credit is not a payment method. Leave the payment empty for a credit sale.");
        }

        if (request.Payments.Any(p => p.Amount <= 0))
        {
            throw new BusinessRuleException("Payment amounts must be greater than zero.");
        }

        var customer = request.CustomerId.HasValue
            ? await _db.Customers.FirstOrDefaultAsync(c => c.Id == request.CustomerId, cancellationToken)
              ?? throw new NotFoundException("Customer", request.CustomerId)
            : null;

        if (customer is { IsActive: false })
        {
            throw new BusinessRuleException("Inactive customers cannot be used for sales.");
        }

        Prescription? prescription = null;
        if (request.PrescriptionId.HasValue)
        {
            prescription = await _db.Prescriptions.Include(p => p.Details)
                .FirstOrDefaultAsync(p => p.Id == request.PrescriptionId, cancellationToken)
                ?? throw new NotFoundException("Prescription", request.PrescriptionId);

            if (prescription.BranchId != request.BranchId)
            {
                throw new BusinessRuleException("The prescription belongs to a different branch.");
            }

            if (prescription.Status != PrescriptionStatus.Verified)
            {
                throw new BusinessRuleException($"Prescription {prescription.PrescriptionNumber} must be verified before dispensing. Current status: {prescription.Status}.");
            }

            if (prescription.CustomerId.HasValue && request.CustomerId.HasValue && prescription.CustomerId != request.CustomerId)
            {
                throw new BusinessRuleException("The prescription belongs to a different customer.");
            }
        }

        var medicineIds = request.Items.Select(i => i.MedicineId).ToList();
        var medicines = await _db.Medicines
            .Include(m => m.Category)
            .Where(m => medicineIds.Contains(m.Id))
            .ToDictionaryAsync(m => m.Id, cancellationToken);

        foreach (var item in request.Items)
        {
            if (!medicines.TryGetValue(item.MedicineId, out var medicine))
            {
                throw new NotFoundException("Medicine", item.MedicineId);
            }

            if (!medicine.IsActive)
            {
                throw new BusinessRuleException($"Medicine '{medicine.Name}' is inactive and cannot be sold.");
            }

            if (medicine.RequiresPrescription)
            {
                if (prescription is null)
                {
                    throw new BusinessRuleException($"Medicine '{medicine.Name}' requires a verified prescription.");
                }

                if (prescription.Details.All(d => d.MedicineId != medicine.Id))
                {
                    throw new BusinessRuleException($"Medicine '{medicine.Name}' is not included in prescription {prescription.PrescriptionNumber}.");
                }
            }

            if (item.Quantity <= 0)
            {
                throw new BusinessRuleException("Sale quantity must be greater than zero.");
            }

            if (item.DiscountAmount < 0)
            {
                throw new BusinessRuleException("Discount cannot be negative.");
            }
        }

        await using var transaction = _db.Database.BeginTransaction(IsolationLevel.Serializable);
        try
        {
            var now = DateTime.UtcNow;
            var batches = await _db.MedicineBatches
                .Where(b => b.BranchId == request.BranchId && medicineIds.Contains(b.MedicineId))
                .ToListAsync(cancellationToken);

            var sale = new Sale
            {
                InvoiceNumber = await _documents.NextAsync(DocumentType.Sale, now, cancellationToken),
                BranchId = request.BranchId,
                CustomerId = request.CustomerId,
                PrescriptionId = request.PrescriptionId,
                CashierUserId = _currentUser.UserId ?? Guid.Empty,
                SaleDateUtc = now,
                Status = SaleStatus.Completed,
                Notes = request.Notes?.Trim()
            };

            var lineCalculations = new List<SaleLineCalculation>();
            var pendingMovements = new List<(MedicineBatch Batch, int Quantity, int Before, int After, decimal UnitCost)>();

            foreach (var item in request.Items)
            {
                var medicine = medicines[item.MedicineId];
                var unitPrice = item.UnitPrice ?? medicine.SellingPrice;
                if (unitPrice < 0)
                {
                    throw new BusinessRuleException("Unit price cannot be negative.");
                }

                var eligible = batches.Where(b => b.MedicineId == item.MedicineId).ToList();
                IReadOnlyList<BatchAllocation> allocations;
                if (item.BatchId.HasValue)
                {
                    var batch = eligible.FirstOrDefault(b => b.Id == item.BatchId.Value)
                        ?? throw new BusinessRuleException($"Batch {item.BatchId} is not available at this branch.");

                    if (!batch.IsActive || batch.CurrentQuantity < item.Quantity)
                    {
                        throw new BusinessRuleException($"Insufficient stock in batch '{batch.BatchNumber}'.");
                    }

                    if (batch.ExpiryDate.Date <= now.Date)
                    {
                        throw new BusinessRuleException($"Batch '{batch.BatchNumber}' is expired and cannot be sold.");
                    }

                    allocations = new[] { new BatchAllocation(batch, item.Quantity) };
                }
                else
                {
                    allocations = FefoAllocator.Allocate(eligible, item.Quantity, now);
                }

                var discountRemaining = SaleCalculator.Round(item.DiscountAmount);
                for (var i = 0; i < allocations.Count; i++)
                {
                    var allocation = allocations[i];
                    var discount = i == allocations.Count - 1
                        ? discountRemaining
                        : SaleCalculator.Round(item.DiscountAmount * allocation.Quantity / item.Quantity);
                    discountRemaining -= discount;

                    var line = SaleCalculator.CalculateLine(allocation.Quantity, unitPrice, discount, medicine.TaxPercentage);
                    lineCalculations.Add(line);

                    var before = allocation.Batch.CurrentQuantity;
                    allocation.Batch.CurrentQuantity -= allocation.Quantity;

                    sale.Details.Add(new SaleDetail
                    {
                        MedicineId = item.MedicineId,
                        BatchId = allocation.Batch.Id,
                        BatchNumber = allocation.Batch.BatchNumber,
                        Quantity = allocation.Quantity,
                        ReturnedQuantity = 0,
                        UnitPrice = unitPrice,
                        UnitCost = allocation.Batch.PurchasePrice,
                        DiscountAmount = line.Discount,
                        TaxPercentage = medicine.TaxPercentage,
                        TaxAmount = line.Tax,
                        LineTotal = line.LineTotal
                    });

                    pendingMovements.Add((allocation.Batch, allocation.Quantity, before, allocation.Batch.CurrentQuantity, allocation.Batch.PurchasePrice));
                }
            }

            var totals = SaleCalculator.CalculateTotals(lineCalculations, request.DiscountAmount);
            sale.SubTotal = totals.SubTotal;
            sale.DiscountAmount = totals.DiscountTotal;
            sale.TaxAmount = totals.TaxTotal;
            sale.TotalAmount = totals.Total;

            var tendered = request.Payments.Sum(p => p.Amount);
            var applied = Math.Min(tendered, totals.Total);
            var change = SaleCalculator.Round(tendered - applied);

            if (applied < totals.Total)
            {
                if (customer is null)
                {
                    throw new BusinessRuleException("Payment does not cover the sale total. Select a customer for a credit sale.");
                }

                if (customer.CreditLimit <= 0)
                {
                    throw new BusinessRuleException("This customer has no credit limit. Payment must cover the sale total.");
                }

                var outstanding = await _db.Sales.AsNoTracking()
                    .Where(s => s.CustomerId == customer.Id && s.Status != SaleStatus.Cancelled)
                    .SumAsync(s => (decimal?)(s.TotalAmount - s.PaidAmount), cancellationToken) ?? 0m;

                var unpaid = totals.Total - applied;
                if (outstanding + unpaid > customer.CreditLimit)
                {
                    throw new BusinessRuleException($"Credit limit exceeded. Outstanding: {outstanding:0.00}, credit limit: {customer.CreditLimit:0.00}.");
                }
            }

            var remainingToApply = applied;
            foreach (var payment in request.Payments)
            {
                if (remainingToApply <= 0)
                {
                    break;
                }

                var paymentAmount = Math.Min(payment.Amount, remainingToApply);
                sale.Payments.Add(new SalePayment
                {
                    Method = payment.Method,
                    Amount = paymentAmount,
                    Reference = payment.Reference?.Trim(),
                    PaidAtUtc = now
                });
                remainingToApply -= paymentAmount;
            }

            sale.PaidAmount = applied;
            sale.ChangeAmount = change;
            sale.PaymentStatus = applied >= totals.Total
                ? PaymentStatus.Paid
                : applied > 0 ? PaymentStatus.PartiallyPaid : PaymentStatus.Unpaid;

            if (customer is not null)
            {
                customer.LoyaltyPoints += (int)Math.Floor(totals.Total);
            }

            _db.Sales.Add(sale);
            await _db.SaveChangesAsync(cancellationToken);

            foreach (var movement in pendingMovements)
            {
                _db.InventoryTransactions.Add(new InventoryTransaction
                {
                    MedicineId = movement.Batch.MedicineId,
                    Batch = movement.Batch,
                    BatchId = movement.Batch.Id,
                    BranchId = request.BranchId,
                    Type = InventoryTransactionType.Sale,
                    QuantityChange = -movement.Quantity,
                    QuantityBefore = movement.Before,
                    QuantityAfter = movement.After,
                    UnitCost = movement.UnitCost,
                    ReferenceType = nameof(Sale),
                    ReferenceId = sale.Id,
                    Notes = $"Sale {sale.InvoiceNumber}",
                    PerformedByUserId = _currentUser.UserId,
                    CreatedAtUtc = now
                });
            }

            if (prescription is not null)
            {
                prescription.Status = PrescriptionStatus.Dispensed;
            }

            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            var dto = await GetByIdAsync(sale.Id, cancellationToken);
            await _audit.LogAsync("SaleCreated", nameof(Sale), sale.Id.ToString(), null,
                new { sale.InvoiceNumber, sale.TotalAmount, sale.PaidAmount, sale.PaymentStatus }, cancellationToken);
            return dto;
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new ConflictException("Stock changed while completing the sale. Please retry.");
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<SaleDto> CancelAsync(int id, string reason, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new BusinessRuleException("A cancellation reason is required.");
        }

        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var sale = await _db.Sales
                .Include(s => s.Details)
                .Include(s => s.Prescription)
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken)
                ?? throw new NotFoundException("Sale", id);

            if (sale.Status == SaleStatus.Cancelled)
            {
                throw new BusinessRuleException("This sale is already cancelled.");
            }

            if (sale.Status is SaleStatus.PartiallyReturned or SaleStatus.Returned)
            {
                throw new BusinessRuleException("Sales with returns cannot be cancelled. Use the returns workflow instead.");
            }

            if (await _db.Returns.AnyAsync(r => r.OriginalSaleId == id, cancellationToken))
            {
                throw new BusinessRuleException("This sale has return records and cannot be cancelled.");
            }

            var now = DateTime.UtcNow;
            foreach (var detail in sale.Details)
            {
                var batch = await _db.MedicineBatches.FirstOrDefaultAsync(b => b.Id == detail.BatchId, cancellationToken)
                    ?? throw new NotFoundException("MedicineBatch", detail.BatchId);

                var before = batch.CurrentQuantity;
                var quantity = detail.Quantity - detail.ReturnedQuantity;
                batch.CurrentQuantity += quantity;

                _db.InventoryTransactions.Add(new InventoryTransaction
                {
                    MedicineId = detail.MedicineId,
                    Batch = batch,
                    BatchId = batch.Id,
                    BranchId = sale.BranchId,
                    Type = InventoryTransactionType.SaleCancellation,
                    QuantityChange = quantity,
                    QuantityBefore = before,
                    QuantityAfter = batch.CurrentQuantity,
                    UnitCost = detail.UnitCost,
                    ReferenceType = nameof(Sale),
                    ReferenceId = sale.Id,
                    Notes = $"Sale {sale.InvoiceNumber} cancelled: {reason.Trim()}",
                    PerformedByUserId = _currentUser.UserId,
                    CreatedAtUtc = now
                });
            }

            sale.Status = SaleStatus.Cancelled;
            sale.CancelledAtUtc = now;
            sale.CancelledByUserId = _currentUser.UserId;
            sale.CancellationReason = reason.Trim();
            if (sale.PaidAmount > 0)
            {
                sale.PaymentStatus = PaymentStatus.Refunded;
            }

            if (sale.Prescription is { Status: PrescriptionStatus.Dispensed } prescription &&
                !await _db.Sales.AnyAsync(s => s.PrescriptionId == prescription.Id && s.Id != sale.Id && s.Status != SaleStatus.Cancelled, cancellationToken))
            {
                prescription.Status = PrescriptionStatus.Verified;
            }

            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            var dto = await GetByIdAsync(id, cancellationToken);
            await _audit.LogAsync("SaleCancelled", nameof(Sale), id.ToString(),
                new { Status = SaleStatus.Completed.ToString() }, new { Status = sale.Status.ToString(), Reason = sale.CancellationReason }, cancellationToken);
            return dto;
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new ConflictException("Stock changed while cancelling the sale. Please retry.");
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<SalePaymentDto> RecordPaymentAsync(int id, RecordSalePaymentRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Amount <= 0)
        {
            throw new BusinessRuleException("Payment amount must be greater than zero.");
        }

        if (request.Method == PaymentMethod.Credit)
        {
            throw new BusinessRuleException("Credit is not a valid payment method.");
        }

        var sale = await _db.Sales.FirstOrDefaultAsync(s => s.Id == id, cancellationToken)
            ?? throw new NotFoundException("Sale", id);

        if (sale.Status == SaleStatus.Cancelled)
        {
            throw new BusinessRuleException("Payments cannot be recorded against a cancelled sale.");
        }

        var remaining = SaleCalculator.Round(sale.TotalAmount - sale.PaidAmount);
        if (request.Amount > remaining)
        {
            throw new BusinessRuleException($"Payment exceeds the outstanding amount of {remaining:0.00}.");
        }

        var payment = new SalePayment
        {
            SaleId = sale.Id,
            Method = request.Method,
            Amount = request.Amount,
            Reference = request.Reference?.Trim(),
            PaidAtUtc = DateTime.UtcNow
        };

        _db.SalePayments.Add(payment);
        sale.PaidAmount = SaleCalculator.Round(sale.PaidAmount + request.Amount);
        sale.PaymentStatus = sale.PaidAmount >= sale.TotalAmount ? PaymentStatus.Paid : PaymentStatus.PartiallyPaid;

        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("SalePaymentRecorded", nameof(Sale), sale.Id.ToString(), null,
            new { payment.Amount, payment.Method, sale.PaidAmount }, cancellationToken);

        return new SalePaymentDto
        {
            Id = payment.Id,
            Method = payment.Method,
            Amount = payment.Amount,
            Reference = payment.Reference,
            PaidAtUtc = payment.PaidAtUtc
        };
    }

    public async Task<SaleInvoiceDto> GetInvoiceAsync(int id, CancellationToken cancellationToken = default)
    {
        var sale = await GetByIdAsync(id, cancellationToken);
        var branch = await _db.Branches.AsNoTracking().FirstAsync(b => b.Id == sale.BranchId, cancellationToken);
        var customer = sale.CustomerId.HasValue
            ? await _db.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == sale.CustomerId, cancellationToken)
            : null;

        return new SaleInvoiceDto
        {
            Sale = sale,
            BranchAddress = branch.Address ?? string.Empty,
            BranchPhone = branch.Phone,
            CustomerPhone = customer?.Phone,
            CustomerAddress = customer?.Address
        };
    }

    public async Task<TodaySalesSummaryDto> GetTodaySummaryAsync(int? branchId, CancellationToken cancellationToken = default)
    {
        var start = DateTime.UtcNow.Date;
        var end = start.AddDays(1);

        var sales = _db.Sales.AsNoTracking()
            .Where(s => s.SaleDateUtc >= start && s.SaleDateUtc < end && s.Status != SaleStatus.Cancelled &&
                        (!branchId.HasValue || s.BranchId == branchId));

        var invoiceCount = await sales.CountAsync(cancellationToken);
        var totals = await sales
            .GroupBy(_ => 1)
            .Select(g => new
            {
                TotalSales = g.Sum(s => s.TotalAmount),
                TotalTax = g.Sum(s => s.TaxAmount),
                TotalDiscount = g.Sum(s => s.DiscountAmount)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var breakdown = await _db.SalePayments.AsNoTracking()
            .Where(p => p.Sale.SaleDateUtc >= start && p.Sale.SaleDateUtc < end &&
                        p.Sale.Status != SaleStatus.Cancelled &&
                        (!branchId.HasValue || p.Sale.BranchId == branchId))
            .GroupBy(p => p.Method)
            .Select(g => new PaymentBreakdownDto
            {
                Method = g.Key,
                Count = g.Count(),
                Amount = g.Sum(p => p.Amount)
            })
            .ToListAsync(cancellationToken);

        return new TodaySalesSummaryDto
        {
            DateUtc = start,
            InvoiceCount = invoiceCount,
            TotalSales = totals?.TotalSales ?? 0,
            TotalTax = totals?.TotalTax ?? 0,
            TotalDiscount = totals?.TotalDiscount ?? 0,
            PaymentBreakdown = breakdown
        };
    }
}
