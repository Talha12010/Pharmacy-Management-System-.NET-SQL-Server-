using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common.Interfaces;
using PharmacyManagement.Application.DTOs.Reports;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Enums;
using PharmacyManagement.Domain.Exceptions;

namespace PharmacyManagement.Application.Services;

public sealed class ReportService : IReportService
{
    private readonly IApplicationDbContext _db;

    public ReportService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ReportTableDto> GetAsync(string type, ReportFilter filter, CancellationToken cancellationToken = default)
    {
        var normalized = (type ?? string.Empty).Trim().ToLowerInvariant();
        var to = filter.To?.Date ?? DateTime.UtcNow.Date;
        var from = filter.From?.Date ?? to.AddDays(-30);
        var subtitle = $"{from:yyyy-MM-dd} to {to:yyyy-MM-dd}";

        return normalized switch
        {
            ReportTypes.Sales => await BuildSalesAsync(filter, from, to, subtitle, cancellationToken),
            ReportTypes.ProfitLoss => await BuildProfitLossAsync(filter, from, to, subtitle, cancellationToken),
            ReportTypes.Purchases => await BuildPurchasesAsync(filter, from, to, subtitle, cancellationToken),
            ReportTypes.Inventory => await BuildInventoryAsync(filter, subtitle, cancellationToken),
            ReportTypes.StockValuation => await BuildStockValuationAsync(filter, subtitle, cancellationToken),
            ReportTypes.Expiry => await BuildExpiryAsync(filter, subtitle, cancellationToken),
            ReportTypes.LowStock => await BuildLowStockAsync(filter, subtitle, cancellationToken),
            ReportTypes.Suppliers => await BuildSuppliersAsync(filter, subtitle, cancellationToken),
            ReportTypes.Customers => await BuildCustomersAsync(filter, subtitle, cancellationToken),
            ReportTypes.Expenses => await BuildExpensesAsync(filter, from, to, subtitle, cancellationToken),
            ReportTypes.Returns => await BuildReturnsAsync(filter, from, to, subtitle, cancellationToken),
            ReportTypes.Payments => await BuildPaymentsAsync(filter, from, to, subtitle, cancellationToken),
            ReportTypes.EmployeeActivity => await BuildEmployeeActivityAsync(filter, from, to, subtitle, cancellationToken),
            _ => throw new BusinessRuleException($"Unknown report type '{type}'. Supported: {string.Join(", ", ReportTypes.All)}.")
        };
    }

    private async Task<ReportTableDto> BuildSalesAsync(ReportFilter filter, DateTime from, DateTime to, string subtitle, CancellationToken cancellationToken)
    {
        var toExclusive = to.AddDays(1);
        var sales = _db.Sales.AsNoTracking()
            .Where(s => s.SaleDateUtc >= from && s.SaleDateUtc < toExclusive && s.Status != SaleStatus.Cancelled);

        if (filter.BranchId.HasValue)
        {
            sales = sales.Where(s => s.BranchId == filter.BranchId);
        }

        if (filter.CustomerId.HasValue)
        {
            sales = sales.Where(s => s.CustomerId == filter.CustomerId);
        }

        var data = await sales
            .OrderByDescending(s => s.SaleDateUtc)
            .Select(s => new
            {
                s.SaleDateUtc,
                s.InvoiceNumber,
                CustomerName = s.Customer != null ? s.Customer.FirstName + " " + s.Customer.LastName : "Walk-in",
                ItemCount = s.Details.Count,
                s.SubTotal,
                s.DiscountAmount,
                s.TaxAmount,
                s.TotalAmount,
                s.PaidAmount,
                s.Status
            })
            .ToListAsync(cancellationToken);

        var rows = data.Select(s => (IReadOnlyList<object?>)new object?[]
        {
            s.SaleDateUtc, s.InvoiceNumber, s.CustomerName, s.ItemCount,
            s.SubTotal, s.DiscountAmount, s.TaxAmount, s.TotalAmount, s.PaidAmount, s.Status.ToString()
        }).ToList();

        return Table("Sales Report", subtitle,
            new[] { "Date", "Invoice", "Customer", "Items", "Subtotal", "Discount", "Tax", "Total", "Paid", "Status" },
            rows,
            new[]
            {
                new ReportSummaryItem("Invoices", data.Count.ToString()),
                new ReportSummaryItem("Total sales", data.Sum(s => s.TotalAmount).ToString("0.00")),
                new ReportSummaryItem("Total tax", data.Sum(s => s.TaxAmount).ToString("0.00")),
                new ReportSummaryItem("Total discount", data.Sum(s => s.DiscountAmount).ToString("0.00"))
            });
    }

    private async Task<ReportTableDto> BuildProfitLossAsync(ReportFilter filter, DateTime from, DateTime to, string subtitle, CancellationToken cancellationToken)
    {
        var toExclusive = to.AddDays(1);
        var details = _db.SaleDetails.AsNoTracking()
            .Where(d => d.Sale.SaleDateUtc >= from && d.Sale.SaleDateUtc < toExclusive && d.Sale.Status != SaleStatus.Cancelled);

        if (filter.BranchId.HasValue)
        {
            details = details.Where(d => d.Sale.BranchId == filter.BranchId);
        }

        var data = await details
            .Select(d => new
            {
                d.Sale.SaleDateUtc,
                Revenue = d.LineTotal - d.TaxAmount,
                Cost = d.UnitCost * d.Quantity
            })
            .ToListAsync(cancellationToken);

        var groupByMonth = string.Equals(filter.GroupBy, "month", StringComparison.OrdinalIgnoreCase);
        var grouped = data
            .GroupBy(d => groupByMonth
                ? new DateTime(d.SaleDateUtc.Year, d.SaleDateUtc.Month, 1).ToString("MMM yyyy")
                : d.SaleDateUtc.ToString("yyyy-MM-dd"))
            .OrderBy(g => g.Key)
            .Select(g => new
            {
                Period = g.Key,
                Revenue = g.Sum(x => x.Revenue),
                Cost = g.Sum(x => x.Cost)
            })
            .ToList();

        var rows = grouped.Select(g => (IReadOnlyList<object?>)new object?[]
        {
            g.Period,
            g.Revenue,
            g.Cost,
            g.Revenue - g.Cost,
            g.Revenue == 0 ? 0m : Math.Round((g.Revenue - g.Cost) / g.Revenue * 100m, 1)
        }).ToList();

        var totalRevenue = grouped.Sum(g => g.Revenue);
        var totalCost = grouped.Sum(g => g.Cost);

        return Table("Profit & Loss Report", subtitle,
            new[] { "Period", "Revenue (ex tax)", "Cost of goods", "Gross profit", "Margin %" },
            rows,
            new[]
            {
                new ReportSummaryItem("Revenue", totalRevenue.ToString("0.00")),
                new ReportSummaryItem("Cost of goods", totalCost.ToString("0.00")),
                new ReportSummaryItem("Gross profit", (totalRevenue - totalCost).ToString("0.00")),
                new ReportSummaryItem("Margin", totalRevenue == 0 ? "0%" : $"{Math.Round((totalRevenue - totalCost) / totalRevenue * 100m, 1)}%")
            });
    }

    private async Task<ReportTableDto> BuildPurchasesAsync(ReportFilter filter, DateTime from, DateTime to, string subtitle, CancellationToken cancellationToken)
    {
        var toExclusive = to.AddDays(1);
        var orders = _db.PurchaseOrders.AsNoTracking()
            .Where(o => o.OrderDateUtc >= from && o.OrderDateUtc < toExclusive);

        if (filter.BranchId.HasValue)
        {
            orders = orders.Where(o => o.BranchId == filter.BranchId);
        }

        if (filter.SupplierId.HasValue)
        {
            orders = orders.Where(o => o.SupplierId == filter.SupplierId);
        }

        var data = await orders
            .OrderByDescending(o => o.OrderDateUtc)
            .Select(o => new
            {
                o.OrderDateUtc,
                o.OrderNumber,
                o.Supplier.Name,
                BranchName = o.Branch.Name,
                o.Status,
                o.TotalAmount
            })
            .ToListAsync(cancellationToken);

        var rows = data.Select(o => (IReadOnlyList<object?>)new object?[]
        {
            o.OrderDateUtc, o.OrderNumber, o.Name, o.BranchName, o.Status.ToString(), o.TotalAmount
        }).ToList();

        return Table("Purchases Report", subtitle,
            new[] { "Date", "Order", "Supplier", "Branch", "Status", "Total" },
            rows,
            new[]
            {
                new ReportSummaryItem("Orders", data.Count.ToString()),
                new ReportSummaryItem("Total value", data.Sum(o => o.TotalAmount).ToString("0.00"))
            });
    }

    private async Task<ReportTableDto> BuildInventoryAsync(ReportFilter filter, string subtitle, CancellationToken cancellationToken)
    {
        var groups = await StockGroupsAsync(filter, cancellationToken);
        var rows = groups.Select(g => (IReadOnlyList<object?>)new object?[]
        {
            g.MedicineName, g.CategoryName, g.BranchName, g.Quantity, g.ReorderLevel,
            g.IsLowStock ? "Low stock" : "In stock", g.NearestExpiryDate
        }).ToList();

        return Table("Inventory Report", subtitle,
            new[] { "Medicine", "Category", "Branch", "Quantity", "Reorder level", "Status", "Nearest expiry" },
            rows,
            new[]
            {
                new ReportSummaryItem("Items", groups.Count.ToString()),
                new ReportSummaryItem("Total quantity", groups.Sum(g => g.Quantity).ToString()),
                new ReportSummaryItem("Low stock items", groups.Count(g => g.IsLowStock).ToString())
            });
    }

    private async Task<ReportTableDto> BuildStockValuationAsync(ReportFilter filter, string subtitle, CancellationToken cancellationToken)
    {
        var batches = _db.MedicineBatches.AsNoTracking()
            .Include(b => b.Medicine).ThenInclude(m => m.Category)
            .Include(b => b.Branch)
            .Where(b => b.CurrentQuantity > 0);

        if (filter.BranchId.HasValue)
        {
            batches = batches.Where(b => b.BranchId == filter.BranchId);
        }

        if (filter.CategoryId.HasValue)
        {
            batches = batches.Where(b => b.Medicine.CategoryId == filter.CategoryId);
        }

        var data = await batches
            .OrderBy(b => b.Medicine.Name)
            .ThenBy(b => b.ExpiryDate)
            .ToListAsync(cancellationToken);

        var rows = data.Select(b => (IReadOnlyList<object?>)new object?[]
        {
            b.Medicine.Name, b.BatchNumber, b.Branch.Name, b.CurrentQuantity, b.PurchasePrice,
            Math.Round(b.CurrentQuantity * b.PurchasePrice, 2), b.ExpiryDate
        }).ToList();

        return Table("Stock Valuation Report", subtitle,
            new[] { "Medicine", "Batch", "Branch", "Quantity", "Unit cost", "Value", "Expiry" },
            rows,
            new[]
            {
                new ReportSummaryItem("Batches", data.Count.ToString()),
                new ReportSummaryItem("Total quantity", data.Sum(b => b.CurrentQuantity).ToString()),
                new ReportSummaryItem("Stock value", data.Sum(b => b.CurrentQuantity * b.PurchasePrice).ToString("0.00"))
            });
    }

    private async Task<ReportTableDto> BuildExpiryAsync(ReportFilter filter, string subtitle, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var days = Math.Clamp(filter.ExpiringDays ?? 90, 1, 3650);
        var cutoff = today.AddDays(days);

        var batches = _db.MedicineBatches.AsNoTracking()
            .Include(b => b.Medicine)
            .Include(b => b.Branch)
            .Where(b => b.CurrentQuantity > 0 && b.ExpiryDate <= cutoff);

        if (filter.BranchId.HasValue)
        {
            batches = batches.Where(b => b.BranchId == filter.BranchId);
        }

        if (filter.CategoryId.HasValue)
        {
            batches = batches.Where(b => b.Medicine.CategoryId == filter.CategoryId);
        }

        var data = await batches
            .OrderBy(b => b.ExpiryDate)
            .ToListAsync(cancellationToken);

        var rows = data.Select(b => (IReadOnlyList<object?>)new object?[]
        {
            b.Medicine.Name, b.BatchNumber, b.Branch.Name, b.ExpiryDate,
            (b.ExpiryDate.Date - today).Days, b.CurrentQuantity,
            b.ExpiryDate.Date <= today ? "Expired" : "Expiring"
        }).ToList();

        return Table("Expiry Report", $"{subtitle} | within {days} days",
            new[] { "Medicine", "Batch", "Branch", "Expiry", "Days to expiry", "Quantity", "Status" },
            rows,
            new[]
            {
                new ReportSummaryItem("Batches", data.Count.ToString()),
                new ReportSummaryItem("Expired batches", data.Count(b => b.ExpiryDate.Date <= today).ToString()),
                new ReportSummaryItem("Quantity at risk", data.Sum(b => b.CurrentQuantity).ToString())
            });
    }

    private async Task<ReportTableDto> BuildLowStockAsync(ReportFilter filter, string subtitle, CancellationToken cancellationToken)
    {
        var groups = (await StockGroupsAsync(filter, cancellationToken)).Where(g => g.IsLowStock).ToList();
        var rows = groups.Select(g => (IReadOnlyList<object?>)new object?[]
        {
            g.MedicineName, g.CategoryName, g.BranchName, g.Quantity, g.ReorderLevel,
            Math.Max(0, g.ReorderLevel - g.Quantity)
        }).ToList();

        return Table("Low Stock Report", subtitle,
            new[] { "Medicine", "Category", "Branch", "Quantity", "Reorder level", "Shortage" },
            rows,
            new[]
            {
                new ReportSummaryItem("Low stock items", groups.Count.ToString()),
                new ReportSummaryItem("Total shortage", groups.Sum(g => Math.Max(0, g.ReorderLevel - g.Quantity)).ToString())
            });
    }

    private async Task<ReportTableDto> BuildSuppliersAsync(ReportFilter filter, string subtitle, CancellationToken cancellationToken)
    {
        var suppliers = _db.Suppliers.AsNoTracking().AsQueryable();
        if (filter.SupplierId.HasValue)
        {
            suppliers = suppliers.Where(s => s.Id == filter.SupplierId);
        }

        var supplierList = await suppliers.OrderBy(s => s.Name).ToListAsync(cancellationToken);
        var ids = supplierList.Select(s => s.Id).ToList();

        var purchases = await _db.GoodsReceipts.AsNoTracking()
            .Where(r => ids.Contains(r.PurchaseOrder.SupplierId))
            .GroupBy(r => r.PurchaseOrder.SupplierId)
            .Select(g => new { Id = g.Key, Total = g.Sum(x => x.TotalAmount) })
            .ToDictionaryAsync(x => x.Id, x => x.Total, cancellationToken);

        var payments = await _db.SupplierPayments.AsNoTracking()
            .Where(p => ids.Contains(p.SupplierId))
            .GroupBy(p => p.SupplierId)
            .Select(g => new { Id = g.Key, Total = g.Sum(x => x.Amount) })
            .ToDictionaryAsync(x => x.Id, x => x.Total, cancellationToken);

        var returns = await _db.Returns.AsNoTracking()
            .Where(r => r.SupplierId != null && ids.Contains(r.SupplierId.Value) &&
                        (r.Status == ReturnStatus.Approved || r.Status == ReturnStatus.Completed))
            .GroupBy(r => r.SupplierId!.Value)
            .Select(g => new { Id = g.Key, Total = g.Sum(x => x.TotalAmount) })
            .ToDictionaryAsync(x => x.Id, x => x.Total, cancellationToken);

        var rows = supplierList.Select(s =>
        {
            var purchased = purchases.GetValueOrDefault(s.Id);
            var paid = payments.GetValueOrDefault(s.Id);
            var returned = returns.GetValueOrDefault(s.Id);
            return (IReadOnlyList<object?>)new object?[]
            {
                s.Name, s.Phone ?? string.Empty, purchased, paid, returned,
                s.OpeningBalance + purchased - paid - returned
            };
        }).ToList();

        return Table("Suppliers Report", subtitle,
            new[] { "Supplier", "Phone", "Purchases", "Paid", "Returns", "Outstanding" },
            rows,
            new[]
            {
                new ReportSummaryItem("Suppliers", supplierList.Count.ToString()),
                new ReportSummaryItem("Purchases", purchases.Values.Sum().ToString("0.00")),
                new ReportSummaryItem("Paid", payments.Values.Sum().ToString("0.00")),
                new ReportSummaryItem("Outstanding", rows.Sum(r => Convert.ToDecimal(r[5])).ToString("0.00"))
            });
    }

    private async Task<ReportTableDto> BuildCustomersAsync(ReportFilter filter, string subtitle, CancellationToken cancellationToken)
    {
        var customers = _db.Customers.AsNoTracking().AsQueryable();
        if (filter.CustomerId.HasValue)
        {
            customers = customers.Where(c => c.Id == filter.CustomerId);
        }

        var customerList = await customers.OrderBy(c => c.FirstName).ThenBy(c => c.LastName).ToListAsync(cancellationToken);
        var ids = customerList.Select(c => c.Id).ToList();

        var sales = await _db.Sales.AsNoTracking()
            .Where(s => s.CustomerId != null && ids.Contains(s.CustomerId.Value) && s.Status != SaleStatus.Cancelled)
            .GroupBy(s => s.CustomerId!.Value)
            .Select(g => new { Id = g.Key, Total = g.Sum(x => x.TotalAmount), Paid = g.Sum(x => x.PaidAmount), Count = g.Count() })
            .ToDictionaryAsync(x => x.Id, x => x, cancellationToken);

        var payments = await _db.CustomerPayments.AsNoTracking()
            .Where(p => ids.Contains(p.CustomerId))
            .GroupBy(p => p.CustomerId)
            .Select(g => new { Id = g.Key, Total = g.Sum(x => x.Amount) })
            .ToDictionaryAsync(x => x.Id, x => x.Total, cancellationToken);

        var returns = await _db.Returns.AsNoTracking()
            .Where(r => r.CustomerId != null && ids.Contains(r.CustomerId.Value) &&
                        (r.Status == ReturnStatus.Approved || r.Status == ReturnStatus.Completed))
            .GroupBy(r => r.CustomerId!.Value)
            .Select(g => new { Id = g.Key, Total = g.Sum(x => x.TotalAmount) })
            .ToDictionaryAsync(x => x.Id, x => x.Total, cancellationToken);

        var refunds = await _db.Refunds.AsNoTracking()
            .Where(r => r.Return.CustomerId != null && ids.Contains(r.Return.CustomerId.Value) &&
                        (r.Return.Status == ReturnStatus.Approved || r.Return.Status == ReturnStatus.Completed))
            .GroupBy(r => r.Return.CustomerId!.Value)
            .Select(g => new { Id = g.Key, Total = g.Sum(x => x.Amount) })
            .ToDictionaryAsync(x => x.Id, x => x.Total, cancellationToken);

        var rows = customerList.Select(c =>
        {
            sales.TryGetValue(c.Id, out var sale);
            var purchased = sale?.Total ?? 0m;
            var paid = sale?.Paid ?? 0m;
            var accountPayments = payments.GetValueOrDefault(c.Id);
            var returned = returns.GetValueOrDefault(c.Id);
            var refunded = refunds.GetValueOrDefault(c.Id);
            return (IReadOnlyList<object?>)new object?[]
            {
                c.Code, c.FullName, c.Phone ?? string.Empty, sale?.Count ?? 0,
                purchased, paid + accountPayments, purchased - paid - accountPayments - returned + refunded
            };
        }).ToList();

        return Table("Customers Report", subtitle,
            new[] { "Code", "Customer", "Phone", "Invoices", "Purchases", "Paid", "Outstanding" },
            rows,
            new[]
            {
                new ReportSummaryItem("Customers", customerList.Count.ToString()),
                new ReportSummaryItem("Purchases", rows.Sum(r => Convert.ToDecimal(r[4])).ToString("0.00")),
                new ReportSummaryItem("Paid", rows.Sum(r => Convert.ToDecimal(r[5])).ToString("0.00")),
                new ReportSummaryItem("Outstanding", rows.Sum(r => Convert.ToDecimal(r[6])).ToString("0.00"))
            });
    }

    private async Task<ReportTableDto> BuildExpensesAsync(ReportFilter filter, DateTime from, DateTime to, string subtitle, CancellationToken cancellationToken)
    {
        var toExclusive = to.AddDays(1);
        var expenses = _db.Expenses.AsNoTracking()
            .Include(e => e.ExpenseCategory)
            .Include(e => e.Branch)
            .Where(e => e.ExpenseDateUtc >= from && e.ExpenseDateUtc < toExclusive);

        if (filter.BranchId.HasValue)
        {
            expenses = expenses.Where(e => e.BranchId == filter.BranchId);
        }

        if (filter.CategoryId.HasValue)
        {
            expenses = expenses.Where(e => e.ExpenseCategoryId == filter.CategoryId);
        }

        var data = await expenses.OrderByDescending(e => e.ExpenseDateUtc).ToListAsync(cancellationToken);
        var rows = data.Select(e => (IReadOnlyList<object?>)new object?[]
        {
            e.ExpenseDateUtc, e.ExpenseNumber, e.ExpenseCategory.Name, e.Branch.Name,
            e.PaymentMethod.ToString(), e.Amount, e.Description
        }).ToList();

        return Table("Expenses Report", subtitle,
            new[] { "Date", "Number", "Category", "Branch", "Method", "Amount", "Description" },
            rows,
            new[]
            {
                new ReportSummaryItem("Expenses", data.Count.ToString()),
                new ReportSummaryItem("Total", data.Sum(e => e.Amount).ToString("0.00"))
            });
    }

    private async Task<ReportTableDto> BuildReturnsAsync(ReportFilter filter, DateTime from, DateTime to, string subtitle, CancellationToken cancellationToken)
    {
        var toExclusive = to.AddDays(1);
        var returns = _db.Returns.AsNoTracking()
            .Include(r => r.OriginalSale)
            .Include(r => r.OriginalPurchaseOrder)
            .Include(r => r.Customer)
            .Include(r => r.Supplier)
            .Where(r => r.RequestedAtUtc >= from && r.RequestedAtUtc < toExclusive);

        if (filter.BranchId.HasValue)
        {
            returns = returns.Where(r => r.BranchId == filter.BranchId);
        }

        if (filter.SupplierId.HasValue)
        {
            returns = returns.Where(r => r.SupplierId == filter.SupplierId);
        }

        if (filter.CustomerId.HasValue)
        {
            returns = returns.Where(r => r.CustomerId == filter.CustomerId);
        }

        var data = await returns.OrderByDescending(r => r.RequestedAtUtc).ToListAsync(cancellationToken);
        var rows = data.Select(r => (IReadOnlyList<object?>)new object?[]
        {
            r.RequestedAtUtc,
            r.ReturnNumber,
            r.Type.ToString(),
            r.OriginalSale?.InvoiceNumber ?? r.OriginalPurchaseOrder?.OrderNumber ?? string.Empty,
            r.Customer?.FullName ?? r.Supplier?.Name ?? string.Empty,
            r.Status.ToString(),
            r.TotalAmount
        }).ToList();

        return Table("Returns Report", subtitle,
            new[] { "Date", "Number", "Type", "Reference", "Party", "Status", "Amount" },
            rows,
            new[]
            {
                new ReportSummaryItem("Returns", data.Count.ToString()),
                new ReportSummaryItem("Total", data.Sum(r => r.TotalAmount).ToString("0.00"))
            });
    }

    private async Task<ReportTableDto> BuildPaymentsAsync(ReportFilter filter, DateTime from, DateTime to, string subtitle, CancellationToken cancellationToken)
    {
        var toExclusive = to.AddDays(1);
        var rows = new List<PaymentRow>();

        var salePayments = _db.SalePayments.AsNoTracking()
            .Where(p => p.PaidAtUtc >= from && p.PaidAtUtc < toExclusive && p.Sale.Status != SaleStatus.Cancelled);

        if (filter.BranchId.HasValue)
        {
            salePayments = salePayments.Where(p => p.Sale.BranchId == filter.BranchId);
        }

        if (filter.CustomerId.HasValue)
        {
            salePayments = salePayments.Where(p => p.Sale.CustomerId == filter.CustomerId);
        }

        var saleData = await salePayments
            .Select(p => new
            {
                p.PaidAtUtc,
                p.Method,
                p.Amount,
                p.Reference,
                Invoice = p.Sale.InvoiceNumber,
                Customer = p.Sale.Customer != null ? p.Sale.Customer.FirstName + " " + p.Sale.Customer.LastName : "Walk-in"
            })
            .ToListAsync(cancellationToken);

        rows.AddRange(saleData.Select(p => new PaymentRow(p.PaidAtUtc, "Sale", p.Customer, p.Method, p.Invoice, p.Reference, p.Amount, true)));

        var customerPayments = _db.CustomerPayments.AsNoTracking()
            .Where(p => p.PaymentDateUtc >= from && p.PaymentDateUtc < toExclusive);

        if (filter.CustomerId.HasValue)
        {
            customerPayments = customerPayments.Where(p => p.CustomerId == filter.CustomerId);
        }

        var customerData = await customerPayments
            .Select(p => new
            {
                p.PaymentDateUtc,
                p.Method,
                p.Amount,
                p.Reference,
                Invoice = p.Sale != null ? p.Sale.InvoiceNumber : null,
                Customer = p.Customer.FirstName + " " + p.Customer.LastName
            })
            .ToListAsync(cancellationToken);

        rows.AddRange(customerData.Select(p => new PaymentRow(p.PaymentDateUtc, "Customer", p.Customer, p.Method, p.Invoice ?? string.Empty, p.Reference, p.Amount, true)));

        var supplierPayments = _db.SupplierPayments.AsNoTracking()
            .Where(p => p.PaymentDateUtc >= from && p.PaymentDateUtc < toExclusive);

        if (filter.SupplierId.HasValue)
        {
            supplierPayments = supplierPayments.Where(p => p.SupplierId == filter.SupplierId);
        }

        var supplierData = await supplierPayments
            .Select(p => new
            {
                p.PaymentDateUtc,
                p.Method,
                p.Amount,
                p.Reference,
                Order = p.PurchaseOrder != null ? p.PurchaseOrder.OrderNumber : null,
                Supplier = p.Supplier.Name
            })
            .ToListAsync(cancellationToken);

        rows.AddRange(supplierData.Select(p => new PaymentRow(p.PaymentDateUtc, "Supplier", p.Supplier, p.Method, p.Order ?? string.Empty, p.Reference, p.Amount, false)));

        var ordered = rows.OrderByDescending(r => r.DateUtc).ToList();
        var tableRows = ordered.Select(r => (IReadOnlyList<object?>)new object?[]
        {
            r.DateUtc, r.Type, r.Party, r.Method.ToString(), r.Reference, r.Amount, r.Inflow ? "In" : "Out"
        }).ToList();

        return Table("Payments Report", subtitle,
            new[] { "Date", "Type", "Party", "Method", "Reference", "Amount", "Direction" },
            tableRows,
            new[]
            {
                new ReportSummaryItem("Received", ordered.Where(r => r.Inflow).Sum(r => r.Amount).ToString("0.00")),
                new ReportSummaryItem("Paid out", ordered.Where(r => !r.Inflow).Sum(r => r.Amount).ToString("0.00")),
                new ReportSummaryItem("Net", (ordered.Where(r => r.Inflow).Sum(r => r.Amount) - ordered.Where(r => !r.Inflow).Sum(r => r.Amount)).ToString("0.00"))
            });
    }

    private async Task<ReportTableDto> BuildEmployeeActivityAsync(ReportFilter filter, DateTime from, DateTime to, string subtitle, CancellationToken cancellationToken)
    {
        var toExclusive = to.AddDays(1);
        var logs = await _db.AuditLogs.AsNoTracking()
            .Where(l => l.CreatedAtUtc >= from && l.CreatedAtUtc < toExclusive)
            .Select(l => new { l.UserName, l.Action, l.EntityName, l.CreatedAtUtc })
            .ToListAsync(cancellationToken);

        var grouped = logs
            .GroupBy(l => new { l.UserName, l.Action, l.EntityName })
            .Select(g => new
            {
                User = g.Key.UserName ?? "System",
                g.Key.Action,
                g.Key.EntityName,
                Count = g.Count(),
                LastAt = g.Max(x => x.CreatedAtUtc)
            })
            .OrderByDescending(g => g.Count)
            .ToList();

        var rows = grouped.Select(g => (IReadOnlyList<object?>)new object?[]
        {
            g.User, g.Action, g.EntityName, g.Count, g.LastAt
        }).ToList();

        return Table("Employee Activity Report", subtitle,
            new[] { "User", "Action", "Entity", "Count", "Last occurred" },
            rows,
            new[]
            {
                new ReportSummaryItem("Events", logs.Count.ToString()),
                new ReportSummaryItem("Users", logs.Select(l => l.UserName).Distinct().Count().ToString())
            });
    }

    private async Task<List<StockGroupRow>> StockGroupsAsync(ReportFilter filter, CancellationToken cancellationToken)
    {
        var groups = await _db.MedicineBatches.AsNoTracking()
            .Where(b => b.CurrentQuantity > 0)
            .GroupBy(b => new { b.MedicineId, b.BranchId })
            .Select(g => new
            {
                g.Key.MedicineId,
                g.Key.BranchId,
                Quantity = g.Sum(x => x.CurrentQuantity),
                NearestExpiry = g.Min(x => (DateTime?)x.ExpiryDate)
            })
            .ToListAsync(cancellationToken);

        var medicineIds = groups.Select(g => g.MedicineId).Distinct().ToList();
        var medicines = await _db.Medicines.AsNoTracking()
            .Include(m => m.Category)
            .Where(m => medicineIds.Contains(m.Id))
            .ToDictionaryAsync(m => m.Id, cancellationToken);

        var branchIds = groups.Select(g => g.BranchId).Distinct().ToList();
        var branches = await _db.Branches.AsNoTracking()
            .Where(b => branchIds.Contains(b.Id))
            .ToDictionaryAsync(b => b.Id, b => b.Name, cancellationToken);

        return groups
            .Where(g => medicines.ContainsKey(g.MedicineId))
            .Where(g => !filter.BranchId.HasValue || g.BranchId == filter.BranchId)
            .Where(g => !filter.CategoryId.HasValue || medicines[g.MedicineId].CategoryId == filter.CategoryId)
            .Select(g =>
            {
                var medicine = medicines[g.MedicineId];
                return new StockGroupRow(
                    medicine.Name,
                    medicine.Category?.Name ?? string.Empty,
                    branches.GetValueOrDefault(g.BranchId, string.Empty),
                    g.Quantity,
                    medicine.ReorderLevel,
                    medicine.ReorderLevel > 0 && g.Quantity <= medicine.ReorderLevel,
                    g.NearestExpiry);
            })
            .OrderBy(g => g.MedicineName)
            .ToList();
    }

    private static ReportTableDto Table(
        string title, string subtitle, IReadOnlyList<string> columns,
        IReadOnlyList<IReadOnlyList<object?>> rows, IReadOnlyList<ReportSummaryItem> summary)
    {
        return new ReportTableDto
        {
            Title = title,
            Subtitle = subtitle,
            GeneratedAtUtc = DateTime.UtcNow,
            Columns = columns,
            Rows = rows,
            Summary = summary
        };
    }

    private sealed record StockGroupRow(
        string MedicineName, string CategoryName, string BranchName, int Quantity,
        int ReorderLevel, bool IsLowStock, DateTime? NearestExpiryDate);

    private sealed record PaymentRow(
        DateTime DateUtc, string Type, string Party, PaymentMethod Method,
        string Reference, string? SecondaryReference, decimal Amount, bool Inflow);
}
