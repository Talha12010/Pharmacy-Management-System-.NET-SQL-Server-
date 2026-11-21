using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.Common.Interfaces;
using PharmacyManagement.Application.DTOs.Purchasing;
using PharmacyManagement.Application.DTOs.Suppliers;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Application.Mappings;
using PharmacyManagement.Domain.Entities;
using PharmacyManagement.Domain.Enums;
using PharmacyManagement.Domain.Exceptions;

namespace PharmacyManagement.Application.Services;

public sealed class SupplierService : ISupplierService
{
    private readonly IApplicationDbContext _db;
    private readonly IAuditService _audit;
    private readonly ICurrentUserService _currentUser;

    public SupplierService(IApplicationDbContext db, IAuditService audit, ICurrentUserService currentUser)
    {
        _db = db;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<SupplierDto>> GetPagedAsync(QueryParameters query, bool includeInactive, CancellationToken cancellationToken = default)
    {
        var suppliers = _db.Suppliers.AsNoTracking();
        if (!includeInactive)
        {
            suppliers = suppliers.Where(s => s.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            suppliers = suppliers.Where(s =>
                s.Name.Contains(term) ||
                (s.CompanyName != null && s.CompanyName.Contains(term)) ||
                (s.ContactPerson != null && s.ContactPerson.Contains(term)) ||
                (s.Phone != null && s.Phone.Contains(term)) ||
                (s.Email != null && s.Email.Contains(term)));
        }

        suppliers = query.SortDesc
            ? suppliers.OrderByDescending(s => s.Name)
            : suppliers.OrderBy(s => s.Name);

        var total = await suppliers.CountAsync(cancellationToken);
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 200);
        var items = await suppliers.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        var ids = items.Select(s => s.Id).ToList();
        var balances = await BuildBalancesAsync(ids, cancellationToken);
        var result = items.Select(s =>
        {
            var dto = s.ToDto();
            if (balances.TryGetValue(s.Id, out var balance))
            {
                dto.TotalPurchases = balance.Purchases;
                dto.TotalPaid = balance.Paid;
                dto.PurchaseCount = balance.PurchaseCount;
                dto.OutstandingBalance = s.OpeningBalance + balance.Purchases - balance.Paid - balance.Returns;
            }

            return dto;
        }).ToList();

        return PagedResult<SupplierDto>.Create(result, page, pageSize, total);
    }

    public async Task<SupplierDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var supplier = await _db.Suppliers.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, cancellationToken)
            ?? throw new NotFoundException("Supplier", id);

        var dto = supplier.ToDto();
        var balances = await BuildBalancesAsync(new[] { id }, cancellationToken);
        if (balances.TryGetValue(id, out var balance))
        {
            dto.TotalPurchases = balance.Purchases;
            dto.TotalPaid = balance.Paid;
            dto.PurchaseCount = balance.PurchaseCount;
            dto.OutstandingBalance = supplier.OpeningBalance + balance.Purchases - balance.Paid - balance.Returns;
        }

        return dto;
    }

    public async Task<SupplierDto> CreateAsync(CreateSupplierRequest request, CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();
        if (await _db.Suppliers.AnyAsync(s => s.Name == name, cancellationToken))
        {
            throw new ConflictException($"Supplier '{name}' already exists.");
        }

        var supplier = new Supplier
        {
            Name = name,
            CompanyName = request.CompanyName?.Trim(),
            ContactPerson = request.ContactPerson?.Trim(),
            Phone = request.Phone?.Trim(),
            Email = request.Email?.Trim(),
            Address = request.Address?.Trim(),
            TaxRegistrationNumber = request.TaxRegistrationNumber?.Trim(),
            OpeningBalance = request.OpeningBalance,
            PaymentTerms = request.PaymentTerms?.Trim(),
            IsActive = true
        };

        _db.Suppliers.Add(supplier);
        await _db.SaveChangesAsync(cancellationToken);
        var dto = await GetByIdAsync(supplier.Id, cancellationToken);
        await _audit.LogAsync("SupplierCreated", nameof(Supplier), supplier.Id.ToString(), null, dto, cancellationToken);
        return dto;
    }

    public async Task<SupplierDto> UpdateAsync(int id, UpdateSupplierRequest request, CancellationToken cancellationToken = default)
    {
        var supplier = await _db.Suppliers.FirstOrDefaultAsync(s => s.Id == id, cancellationToken)
            ?? throw new NotFoundException("Supplier", id);

        var name = request.Name.Trim();
        if (await _db.Suppliers.AnyAsync(s => s.Id != id && s.Name == name, cancellationToken))
        {
            throw new ConflictException($"Supplier '{name}' already exists.");
        }

        var before = await GetByIdAsync(id, cancellationToken);
        supplier.Name = name;
        supplier.CompanyName = request.CompanyName?.Trim();
        supplier.ContactPerson = request.ContactPerson?.Trim();
        supplier.Phone = request.Phone?.Trim();
        supplier.Email = request.Email?.Trim();
        supplier.Address = request.Address?.Trim();
        supplier.TaxRegistrationNumber = request.TaxRegistrationNumber?.Trim();
        supplier.OpeningBalance = request.OpeningBalance;
        supplier.PaymentTerms = request.PaymentTerms?.Trim();
        supplier.IsActive = request.IsActive;

        await _db.SaveChangesAsync(cancellationToken);
        var after = await GetByIdAsync(id, cancellationToken);
        await _audit.LogAsync("SupplierUpdated", nameof(Supplier), id.ToString(), before, after, cancellationToken);
        return after;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var supplier = await _db.Suppliers.FirstOrDefaultAsync(s => s.Id == id, cancellationToken)
            ?? throw new NotFoundException("Supplier", id);

        supplier.IsDeleted = true;
        supplier.DeletedAtUtc = DateTime.UtcNow;
        supplier.IsActive = false;
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("SupplierDeleted", nameof(Supplier), id.ToString(), null, new { supplier.Name }, cancellationToken);
    }

    public async Task<PagedResult<PurchaseOrderListItemDto>> GetPurchasesAsync(int supplierId, QueryParameters query, CancellationToken cancellationToken = default)
    {
        var orders = _db.PurchaseOrders.AsNoTracking().Where(o => o.SupplierId == supplierId);
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

    public async Task<PagedResult<SupplierPaymentDto>> GetPaymentsAsync(int supplierId, QueryParameters query, CancellationToken cancellationToken = default)
    {
        var payments = _db.SupplierPayments.AsNoTracking()
            .Include(p => p.Supplier)
            .Include(p => p.PurchaseOrder)
            .Where(p => p.SupplierId == supplierId);

        var total = await payments.CountAsync(cancellationToken);
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 200);
        var items = await payments
            .OrderByDescending(p => p.PaymentDateUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<SupplierPaymentDto>.Create(items.Select(p => p.ToDto()).ToList(), page, pageSize, total);
    }

    public async Task<SupplierPaymentDto> RecordPaymentAsync(int supplierId, RecordSupplierPaymentRequest request, CancellationToken cancellationToken = default)
    {
        var supplier = await _db.Suppliers.FirstOrDefaultAsync(s => s.Id == supplierId, cancellationToken)
            ?? throw new NotFoundException("Supplier", supplierId);

        if (request.Amount <= 0)
        {
            throw new BusinessRuleException("Payment amount must be greater than zero.");
        }

        if (request.PurchaseOrderId.HasValue)
        {
            var order = await _db.PurchaseOrders.FirstOrDefaultAsync(o => o.Id == request.PurchaseOrderId && o.SupplierId == supplierId, cancellationToken)
                ?? throw new NotFoundException("Purchase order for this supplier", request.PurchaseOrderId);
            if (order.Status is PurchaseOrderStatus.Draft or PurchaseOrderStatus.PendingApproval or PurchaseOrderStatus.Rejected or PurchaseOrderStatus.Cancelled)
            {
                throw new BusinessRuleException("Payments can only be recorded against approved or received purchase orders.");
            }
        }

        var payment = new SupplierPayment
        {
            SupplierId = supplierId,
            PurchaseOrderId = request.PurchaseOrderId,
            Amount = request.Amount,
            PaymentDateUtc = request.PaymentDateUtc ?? DateTime.UtcNow,
            Method = request.Method,
            Reference = request.Reference?.Trim(),
            Notes = request.Notes?.Trim(),
            RecordedByUserId = CurrentUserId(),
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.SupplierPayments.Add(payment);
        await _db.SaveChangesAsync(cancellationToken);
        var dto = (await _db.SupplierPayments.AsNoTracking().Include(p => p.Supplier).Include(p => p.PurchaseOrder)
            .FirstAsync(p => p.Id == payment.Id, cancellationToken)).ToDto();
        await _audit.LogAsync("SupplierPaymentRecorded", nameof(SupplierPayment), payment.Id.ToString(), null, dto, cancellationToken);
        return dto;
    }

    public async Task<StatementDto> GetStatementAsync(int supplierId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var supplier = await _db.Suppliers.AsNoTracking().FirstOrDefaultAsync(s => s.Id == supplierId, cancellationToken)
            ?? throw new NotFoundException("Supplier", supplierId);

        var fromUtc = from?.Date ?? DateTime.MinValue;
        var toUtcExclusive = to.HasValue ? to.Value.Date.AddDays(1) : DateTime.UtcNow.Date.AddDays(1);

        var receipts = await _db.GoodsReceipts.AsNoTracking()
            .Where(r => r.PurchaseOrder.SupplierId == supplierId && r.ReceivedAtUtc < toUtcExclusive)
            .Select(r => new { r.Id, r.ReceivedAtUtc, r.ReceiptNumber, r.TotalAmount })
            .ToListAsync(cancellationToken);

        var payments = await _db.SupplierPayments.AsNoTracking()
            .Where(p => p.SupplierId == supplierId && p.PaymentDateUtc < toUtcExclusive)
            .Select(p => new { p.Id, p.PaymentDateUtc, p.Reference, p.Amount, p.Method })
            .ToListAsync(cancellationToken);

        var returns = await _db.Returns.AsNoTracking()
            .Where(r => r.SupplierId == supplierId && (r.Status == ReturnStatus.Approved || r.Status == ReturnStatus.Completed) && r.RequestedAtUtc < toUtcExclusive)
            .Select(r => new { r.Id, r.RequestedAtUtc, r.ReturnNumber, r.TotalAmount })
            .ToListAsync(cancellationToken);

        var opening = supplier.OpeningBalance
            + receipts.Where(r => r.ReceivedAtUtc < fromUtc).Sum(r => r.TotalAmount)
            - payments.Where(p => p.PaymentDateUtc < fromUtc).Sum(p => p.Amount)
            - returns.Where(r => r.RequestedAtUtc < fromUtc).Sum(r => r.TotalAmount);

        var lines = new List<StatementLineDto>();
        var running = opening;

        foreach (var r in receipts.Where(r => r.ReceivedAtUtc >= fromUtc))
        {
            running += r.TotalAmount;
            lines.Add(new StatementLineDto { DateUtc = r.ReceivedAtUtc, Reference = r.ReceiptNumber, Description = "Goods received", Debit = r.TotalAmount, Balance = running });
        }

        foreach (var p in payments.Where(p => p.PaymentDateUtc >= fromUtc))
        {
            running -= p.Amount;
            lines.Add(new StatementLineDto { DateUtc = p.PaymentDateUtc, Reference = p.Reference ?? $"PAY-{p.Id}", Description = $"Supplier payment ({p.Method})", Credit = p.Amount, Balance = running });
        }

        foreach (var r in returns.Where(r => r.RequestedAtUtc >= fromUtc))
        {
            running -= r.TotalAmount;
            lines.Add(new StatementLineDto { DateUtc = r.RequestedAtUtc, Reference = r.ReturnNumber, Description = "Purchase return", Credit = r.TotalAmount, Balance = running });
        }

        lines = lines.OrderBy(l => l.DateUtc).ToList();
        running = opening;
        foreach (var line in lines)
        {
            running += line.Debit - line.Credit;
            line.Balance = running;
        }

        return new StatementDto
        {
            PartyId = supplierId,
            PartyName = supplier.Name,
            OpeningBalance = opening,
            ClosingBalance = running,
            Lines = lines
        };
    }

    private async Task<Dictionary<int, SupplierBalance>> BuildBalancesAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken)
    {
        if (ids.Count == 0)
        {
            return new Dictionary<int, SupplierBalance>();
        }

        var purchases = await _db.GoodsReceipts.AsNoTracking()
            .Where(r => ids.Contains(r.PurchaseOrder.SupplierId))
            .GroupBy(r => r.PurchaseOrder.SupplierId)
            .Select(g => new { SupplierId = g.Key, Total = g.Sum(x => x.TotalAmount) })
            .ToDictionaryAsync(x => x.SupplierId, x => x.Total, cancellationToken);

        var paid = await _db.SupplierPayments.AsNoTracking()
            .Where(p => ids.Contains(p.SupplierId))
            .GroupBy(p => p.SupplierId)
            .Select(g => new { SupplierId = g.Key, Total = g.Sum(x => x.Amount) })
            .ToDictionaryAsync(x => x.SupplierId, x => x.Total, cancellationToken);

        var returns = await _db.Returns.AsNoTracking()
            .Where(r => r.SupplierId != null && ids.Contains(r.SupplierId.Value) &&
                        (r.Status == ReturnStatus.Approved || r.Status == ReturnStatus.Completed))
            .GroupBy(r => r.SupplierId!.Value)
            .Select(g => new { SupplierId = g.Key, Total = g.Sum(x => x.TotalAmount) })
            .ToDictionaryAsync(x => x.SupplierId, x => x.Total, cancellationToken);

        var counts = await _db.PurchaseOrders.AsNoTracking()
            .Where(o => ids.Contains(o.SupplierId))
            .GroupBy(o => o.SupplierId)
            .Select(g => new { SupplierId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.SupplierId, x => x.Count, cancellationToken);

        return ids.ToDictionary(
            id => id,
            id => new SupplierBalance(
                purchases.GetValueOrDefault(id),
                paid.GetValueOrDefault(id),
                returns.GetValueOrDefault(id),
                counts.GetValueOrDefault(id)));
    }

    private Guid CurrentUserId() => _currentUser.UserId ?? Guid.Empty;

    private sealed record SupplierBalance(decimal Purchases, decimal Paid, decimal Returns, int PurchaseCount);
}
