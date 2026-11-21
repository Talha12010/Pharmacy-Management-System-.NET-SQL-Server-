using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.Common.Interfaces;
using PharmacyManagement.Application.DTOs.Customers;
using PharmacyManagement.Application.DTOs.Sales;
using PharmacyManagement.Application.DTOs.Suppliers;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Application.Mappings;
using PharmacyManagement.Domain.Entities;
using PharmacyManagement.Domain.Enums;
using PharmacyManagement.Domain.Exceptions;

namespace PharmacyManagement.Application.Services;

public sealed class CustomerService : ICustomerService
{
    private readonly IApplicationDbContext _db;
    private readonly IAuditService _audit;
    private readonly ICurrentUserService _currentUser;

    public CustomerService(IApplicationDbContext db, IAuditService audit, ICurrentUserService currentUser)
    {
        _db = db;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<CustomerDto>> GetPagedAsync(QueryParameters query, bool includeInactive, CancellationToken cancellationToken = default)
    {
        var customers = _db.Customers.AsNoTracking();
        if (!includeInactive)
        {
            customers = customers.Where(c => c.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            customers = customers.Where(c =>
                c.Code.Contains(term) ||
                c.FirstName.Contains(term) ||
                c.LastName.Contains(term) ||
                (c.Phone != null && c.Phone.Contains(term)) ||
                (c.Email != null && c.Email.Contains(term)));
        }

        customers = query.SortDesc
            ? customers.OrderByDescending(c => c.FirstName).ThenByDescending(c => c.LastName)
            : customers.OrderBy(c => c.FirstName).ThenBy(c => c.LastName);

        var total = await customers.CountAsync(cancellationToken);
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 200);
        var items = await customers.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        var ids = items.Select(c => c.Id).ToList();
        var balances = await BuildBalancesAsync(ids, cancellationToken);
        var result = items.Select(c =>
        {
            var dto = c.ToDto();
            if (balances.TryGetValue(c.Id, out var balance))
            {
                dto.TotalPurchases = balance.Sales;
                dto.TotalPaid = balance.Paid + balance.Payments;
                dto.InvoiceCount = balance.InvoiceCount;
                dto.OutstandingBalance = balance.Sales - balance.Paid - balance.Payments - balance.Returns + balance.Refunds;
            }

            return dto;
        }).ToList();

        return PagedResult<CustomerDto>.Create(result, page, pageSize, total);
    }

    public async Task<CustomerDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var customer = await _db.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new NotFoundException("Customer", id);

        var dto = customer.ToDto();
        var balances = await BuildBalancesAsync(new[] { id }, cancellationToken);
        if (balances.TryGetValue(id, out var balance))
        {
            dto.TotalPurchases = balance.Sales;
            dto.TotalPaid = balance.Paid + balance.Payments;
            dto.InvoiceCount = balance.InvoiceCount;
            dto.OutstandingBalance = balance.Sales - balance.Paid - balance.Payments - balance.Returns + balance.Refunds;
        }

        return dto;
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var code = request.Code.Trim();
        if (await _db.Customers.AnyAsync(c => c.Code == code, cancellationToken))
        {
            throw new ConflictException($"Customer code '{code}' already exists.");
        }

        var customer = new Customer
        {
            Code = code,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Phone = request.Phone?.Trim(),
            Email = request.Email?.Trim(),
            Address = request.Address?.Trim(),
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            CreditLimit = request.CreditLimit,
            IsActive = true
        };

        _db.Customers.Add(customer);
        await _db.SaveChangesAsync(cancellationToken);
        var dto = await GetByIdAsync(customer.Id, cancellationToken);
        await _audit.LogAsync("CustomerCreated", nameof(Customer), customer.Id.ToString(), null, dto, cancellationToken);
        return dto;
    }

    public async Task<CustomerDto> UpdateAsync(int id, UpdateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new NotFoundException("Customer", id);

        var code = request.Code.Trim();
        if (await _db.Customers.AnyAsync(c => c.Id != id && c.Code == code, cancellationToken))
        {
            throw new ConflictException($"Customer code '{code}' already exists.");
        }

        var before = await GetByIdAsync(id, cancellationToken);
        customer.Code = code;
        customer.FirstName = request.FirstName.Trim();
        customer.LastName = request.LastName.Trim();
        customer.Phone = request.Phone?.Trim();
        customer.Email = request.Email?.Trim();
        customer.Address = request.Address?.Trim();
        customer.DateOfBirth = request.DateOfBirth;
        customer.Gender = request.Gender;
        customer.CreditLimit = request.CreditLimit;
        customer.LoyaltyPoints = request.LoyaltyPoints;
        customer.IsActive = request.IsActive;

        await _db.SaveChangesAsync(cancellationToken);
        var after = await GetByIdAsync(id, cancellationToken);
        await _audit.LogAsync("CustomerUpdated", nameof(Customer), id.ToString(), before, after, cancellationToken);
        return after;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new NotFoundException("Customer", id);

        customer.IsDeleted = true;
        customer.DeletedAtUtc = DateTime.UtcNow;
        customer.IsActive = false;
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("CustomerDeleted", nameof(Customer), id.ToString(), null, new { customer.Code, customer.FullName }, cancellationToken);
    }

    public async Task<PagedResult<SaleListItemDto>> GetPurchasesAsync(int customerId, QueryParameters query, CancellationToken cancellationToken = default)
    {
        if (!await _db.Customers.AnyAsync(c => c.Id == customerId, cancellationToken))
        {
            throw new NotFoundException("Customer", customerId);
        }

        var sales = _db.Sales.AsNoTracking().Where(s => s.CustomerId == customerId);
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

        var userIds = rows.Select(r => r.CashierUserId).Distinct().ToList();
        var cashiers = await _db.Users.AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
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

    public async Task<PagedResult<CustomerPaymentDto>> GetPaymentsAsync(int customerId, QueryParameters query, CancellationToken cancellationToken = default)
    {
        var payments = _db.CustomerPayments.AsNoTracking()
            .Include(p => p.Customer)
            .Include(p => p.Sale)
            .Where(p => p.CustomerId == customerId);

        var total = await payments.CountAsync(cancellationToken);
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 200);
        var items = await payments
            .OrderByDescending(p => p.PaymentDateUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<CustomerPaymentDto>.Create(items.Select(p => p.ToDto()).ToList(), page, pageSize, total);
    }

    public async Task<CustomerPaymentDto> RecordPaymentAsync(int customerId, RecordCustomerPaymentRequest request, CancellationToken cancellationToken = default)
    {
        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Id == customerId, cancellationToken)
            ?? throw new NotFoundException("Customer", customerId);

        if (request.Amount <= 0)
        {
            throw new BusinessRuleException("Payment amount must be greater than zero.");
        }

        if (request.SaleId.HasValue)
        {
            var sale = await _db.Sales.FirstOrDefaultAsync(s => s.Id == request.SaleId && s.CustomerId == customerId, cancellationToken)
                ?? throw new NotFoundException("Sale for this customer", request.SaleId);

            if (sale.Status == SaleStatus.Cancelled)
            {
                throw new BusinessRuleException("Payments cannot be recorded against a cancelled sale.");
            }

            var remaining = sale.TotalAmount - sale.PaidAmount;
            if (request.Amount > remaining + 0.005m)
            {
                throw new BusinessRuleException($"Payment exceeds the outstanding balance of {remaining:0.00} for invoice {sale.InvoiceNumber}.");
            }

            sale.PaidAmount += request.Amount;
            sale.PaymentStatus = sale.PaidAmount >= sale.TotalAmount - 0.005m ? PaymentStatus.Paid : PaymentStatus.PartiallyPaid;
        }
        else
        {
            var openSales = await _db.Sales
                .Where(s => s.CustomerId == customerId && s.Status != SaleStatus.Cancelled && s.TotalAmount > s.PaidAmount)
                .OrderBy(s => s.SaleDateUtc)
                .ToListAsync(cancellationToken);

            var outstanding = openSales.Sum(s => s.TotalAmount - s.PaidAmount);
            if (request.Amount > outstanding + 0.005m)
            {
                throw new BusinessRuleException($"Payment exceeds the customer's outstanding balance of {outstanding:0.00}.");
            }

            var remainingPayment = request.Amount;
            foreach (var sale in openSales)
            {
                if (remainingPayment <= 0)
                {
                    break;
                }

                var due = sale.TotalAmount - sale.PaidAmount;
                var applied = Math.Min(due, remainingPayment);
                sale.PaidAmount += applied;
                sale.PaymentStatus = sale.PaidAmount >= sale.TotalAmount - 0.005m ? PaymentStatus.Paid : PaymentStatus.PartiallyPaid;
                remainingPayment -= applied;
            }
        }

        var payment = new CustomerPayment
        {
            CustomerId = customerId,
            SaleId = request.SaleId,
            Amount = request.Amount,
            PaymentDateUtc = request.PaymentDateUtc ?? DateTime.UtcNow,
            Method = request.Method,
            Reference = request.Reference?.Trim(),
            Notes = request.Notes?.Trim(),
            RecordedByUserId = _currentUser.UserId ?? Guid.Empty,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.CustomerPayments.Add(payment);
        await _db.SaveChangesAsync(cancellationToken);
        var dto = (await _db.CustomerPayments.AsNoTracking().Include(p => p.Customer).Include(p => p.Sale)
            .FirstAsync(p => p.Id == payment.Id, cancellationToken)).ToDto();
        await _audit.LogAsync("CustomerPaymentRecorded", nameof(CustomerPayment), payment.Id.ToString(), null, dto, cancellationToken);
        return dto;
    }

    public async Task<StatementDto> GetStatementAsync(int customerId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var customer = await _db.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == customerId, cancellationToken)
            ?? throw new NotFoundException("Customer", customerId);

        var fromUtc = from?.Date ?? DateTime.MinValue;
        var toUtcExclusive = to.HasValue ? to.Value.Date.AddDays(1) : DateTime.UtcNow.Date.AddDays(1);

        var sales = await _db.Sales.AsNoTracking()
            .Where(s => s.CustomerId == customerId && s.Status != SaleStatus.Cancelled && s.SaleDateUtc < toUtcExclusive)
            .Select(s => new { s.Id, s.SaleDateUtc, s.InvoiceNumber, s.TotalAmount })
            .ToListAsync(cancellationToken);

        var salePayments = await _db.SalePayments.AsNoTracking()
            .Where(p => p.Sale.CustomerId == customerId && p.Sale.Status != SaleStatus.Cancelled && p.PaidAtUtc < toUtcExclusive)
            .Select(p => new { p.Id, p.PaidAtUtc, p.Sale.InvoiceNumber, p.Amount, p.Method })
            .ToListAsync(cancellationToken);

        var accountPayments = await _db.CustomerPayments.AsNoTracking()
            .Where(p => p.CustomerId == customerId && p.PaymentDateUtc < toUtcExclusive)
            .Select(p => new { p.Id, p.PaymentDateUtc, p.Reference, p.Amount, p.Method })
            .ToListAsync(cancellationToken);

        var returns = await _db.Returns.AsNoTracking()
            .Where(r => r.CustomerId == customerId && (r.Status == ReturnStatus.Approved || r.Status == ReturnStatus.Completed) && r.RequestedAtUtc < toUtcExclusive)
            .Select(r => new { r.Id, r.RequestedAtUtc, r.ReturnNumber, r.TotalAmount })
            .ToListAsync(cancellationToken);

        var refunds = await _db.Refunds.AsNoTracking()
            .Where(r => r.Return.CustomerId == customerId &&
                        (r.Return.Status == ReturnStatus.Approved || r.Return.Status == ReturnStatus.Completed) &&
                        r.RefundedAtUtc < toUtcExclusive)
            .Select(r => new { r.Id, r.RefundedAtUtc, r.Return.ReturnNumber, r.Amount, r.Method })
            .ToListAsync(cancellationToken);

        var opening = sales.Where(s => s.SaleDateUtc < fromUtc).Sum(s => s.TotalAmount)
            - salePayments.Where(p => p.PaidAtUtc < fromUtc).Sum(p => p.Amount)
            - accountPayments.Where(p => p.PaymentDateUtc < fromUtc).Sum(p => p.Amount)
            - returns.Where(r => r.RequestedAtUtc < fromUtc).Sum(r => r.TotalAmount)
            + refunds.Where(r => r.RefundedAtUtc < fromUtc).Sum(r => r.Amount);

        var lines = new List<StatementLineDto>();

        foreach (var s in sales.Where(s => s.SaleDateUtc >= fromUtc))
        {
            lines.Add(new StatementLineDto { DateUtc = s.SaleDateUtc, Reference = s.InvoiceNumber, Description = "Sales invoice", Debit = s.TotalAmount });
        }

        foreach (var p in salePayments.Where(p => p.PaidAtUtc >= fromUtc))
        {
            lines.Add(new StatementLineDto { DateUtc = p.PaidAtUtc, Reference = p.InvoiceNumber, Description = $"Payment at sale ({p.Method})", Credit = p.Amount });
        }

        foreach (var p in accountPayments.Where(p => p.PaymentDateUtc >= fromUtc))
        {
            lines.Add(new StatementLineDto { DateUtc = p.PaymentDateUtc, Reference = p.Reference ?? $"PAY-{p.Id}", Description = $"Account payment ({p.Method})", Credit = p.Amount });
        }

        foreach (var r in returns.Where(r => r.RequestedAtUtc >= fromUtc))
        {
            lines.Add(new StatementLineDto { DateUtc = r.RequestedAtUtc, Reference = r.ReturnNumber, Description = "Sales return", Credit = r.TotalAmount });
        }

        foreach (var r in refunds.Where(r => r.RefundedAtUtc >= fromUtc))
        {
            lines.Add(new StatementLineDto { DateUtc = r.RefundedAtUtc, Reference = r.ReturnNumber, Description = $"Refund ({r.Method})", Debit = r.Amount });
        }

        lines = lines.OrderBy(l => l.DateUtc).ToList();
        var running = opening;
        foreach (var line in lines)
        {
            running += line.Debit - line.Credit;
            line.Balance = running;
        }

        return new StatementDto
        {
            PartyId = customerId,
            PartyName = customer.FullName,
            OpeningBalance = opening,
            ClosingBalance = running,
            Lines = lines
        };
    }

    private async Task<Dictionary<int, CustomerBalance>> BuildBalancesAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken)
    {
        if (ids.Count == 0)
        {
            return new Dictionary<int, CustomerBalance>();
        }

        var sales = await _db.Sales.AsNoTracking()
            .Where(s => s.CustomerId != null && ids.Contains(s.CustomerId.Value) && s.Status != SaleStatus.Cancelled)
            .GroupBy(s => s.CustomerId!.Value)
            .Select(g => new { CustomerId = g.Key, Total = g.Sum(x => x.TotalAmount), Paid = g.Sum(x => x.PaidAmount), Count = g.Count() })
            .ToDictionaryAsync(x => x.CustomerId, x => x, cancellationToken);

        var payments = await _db.CustomerPayments.AsNoTracking()
            .Where(p => ids.Contains(p.CustomerId))
            .GroupBy(p => p.CustomerId)
            .Select(g => new { CustomerId = g.Key, Total = g.Sum(x => x.Amount) })
            .ToDictionaryAsync(x => x.CustomerId, x => x.Total, cancellationToken);

        var returns = await _db.Returns.AsNoTracking()
            .Where(r => r.CustomerId != null && ids.Contains(r.CustomerId.Value) &&
                        (r.Status == ReturnStatus.Approved || r.Status == ReturnStatus.Completed))
            .GroupBy(r => r.CustomerId!.Value)
            .Select(g => new { CustomerId = g.Key, Total = g.Sum(x => x.TotalAmount) })
            .ToDictionaryAsync(x => x.CustomerId, x => x.Total, cancellationToken);

        var refunds = await _db.Refunds.AsNoTracking()
            .Where(r => r.Return.CustomerId != null && ids.Contains(r.Return.CustomerId.Value) &&
                        (r.Return.Status == ReturnStatus.Approved || r.Return.Status == ReturnStatus.Completed))
            .GroupBy(r => r.Return.CustomerId!.Value)
            .Select(g => new { CustomerId = g.Key, Total = g.Sum(x => x.Amount) })
            .ToDictionaryAsync(x => x.CustomerId, x => x.Total, cancellationToken);

        return ids.ToDictionary(
            id => id,
            id => sales.TryGetValue(id, out var sale)
                ? new CustomerBalance(sale.Total, sale.Paid, payments.GetValueOrDefault(id), returns.GetValueOrDefault(id), refunds.GetValueOrDefault(id), sale.Count)
                : new CustomerBalance(0, 0, payments.GetValueOrDefault(id), returns.GetValueOrDefault(id), refunds.GetValueOrDefault(id), 0));
    }

    private sealed record CustomerBalance(decimal Sales, decimal Paid, decimal Payments, decimal Returns, decimal Refunds, int InvoiceCount);
}
