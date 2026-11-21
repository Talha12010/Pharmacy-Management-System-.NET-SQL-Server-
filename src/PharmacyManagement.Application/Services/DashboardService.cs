using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common.Interfaces;
using PharmacyManagement.Application.DTOs.Dashboard;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Enums;

namespace PharmacyManagement.Application.Services;

public sealed class DashboardService : IDashboardService
{
    private readonly IApplicationDbContext _db;
    private readonly IInventoryService _inventory;

    public DashboardService(IApplicationDbContext db, IInventoryService inventory)
    {
        _db = db;
        _inventory = inventory;
    }

    public async Task<DashboardDto> GetAsync(int? branchId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var today = now.Date;
        var tomorrow = today.AddDays(1);
        var monthStart = new DateTime(now.Year, now.Month, 1);

        var sales = _db.Sales.AsNoTracking()
            .Where(s => s.Status != SaleStatus.Cancelled && (!branchId.HasValue || s.BranchId == branchId));

        var purchases = _db.GoodsReceipts.AsNoTracking()
            .Where(r => !branchId.HasValue || r.PurchaseOrder.BranchId == branchId);

        var salesToday = await sales
            .Where(s => s.SaleDateUtc >= today && s.SaleDateUtc < tomorrow)
            .SumAsync(s => (decimal?)s.TotalAmount, cancellationToken) ?? 0m;

        var salesMonth = await sales
            .Where(s => s.SaleDateUtc >= monthStart)
            .SumAsync(s => (decimal?)s.TotalAmount, cancellationToken) ?? 0m;

        var purchasesTotal = await purchases
            .SumAsync(r => (decimal?)r.TotalAmount, cancellationToken) ?? 0m;

        var purchasesMonth = await purchases
            .Where(r => r.ReceivedAtUtc >= monthStart)
            .SumAsync(r => (decimal?)r.TotalAmount, cancellationToken) ?? 0m;

        var profitQuery = _db.SaleDetails.AsNoTracking()
            .Where(d => d.Sale.Status != SaleStatus.Cancelled && (!branchId.HasValue || d.Sale.BranchId == branchId));

        var profitTotal = await profitQuery
            .SumAsync(d => (decimal?)(d.LineTotal - d.TaxAmount - d.UnitCost * d.Quantity), cancellationToken) ?? 0m;

        var profitMonth = await profitQuery
            .Where(d => d.Sale.SaleDateUtc >= monthStart)
            .SumAsync(d => (decimal?)(d.LineTotal - d.TaxAmount - d.UnitCost * d.Quantity), cancellationToken) ?? 0m;

        var totalProducts = await _db.Medicines.CountAsync(m => m.IsActive, cancellationToken);
        var totalSuppliers = await _db.Suppliers.CountAsync(s => s.IsActive, cancellationToken);
        var totalCustomers = await _db.Customers.CountAsync(c => c.IsActive, cancellationToken);

        var pendingOrders = await _db.PurchaseOrders.CountAsync(o =>
            o.Status == PurchaseOrderStatus.PendingApproval && (!branchId.HasValue || o.BranchId == branchId), cancellationToken);

        var stockGroups = await _db.MedicineBatches.AsNoTracking()
            .Where(b => b.CurrentQuantity > 0 && (!branchId.HasValue || b.BranchId == branchId))
            .GroupBy(b => new { b.MedicineId, b.BranchId })
            .Select(g => new { g.Key.MedicineId, g.Key.BranchId, Quantity = g.Sum(x => x.CurrentQuantity) })
            .ToListAsync(cancellationToken);

        var medicineIds = stockGroups.Select(g => g.MedicineId).Distinct().ToList();
        var reorderLevels = await _db.Medicines.AsNoTracking()
            .Where(m => medicineIds.Contains(m.Id))
            .ToDictionaryAsync(m => m.Id, m => m.ReorderLevel, cancellationToken);

        var lowStockCount = stockGroups.Count(g =>
            reorderLevels.TryGetValue(g.MedicineId, out var reorder) && reorder > 0 && g.Quantity <= reorder);

        var expiringCutoff = today.AddDays(90);
        var expiringCount = await _db.MedicineBatches.AsNoTracking()
            .Where(b => b.CurrentQuantity > 0 && b.ExpiryDate > today && b.ExpiryDate <= expiringCutoff &&
                        (!branchId.HasValue || b.BranchId == branchId))
            .Select(b => b.MedicineId)
            .Distinct()
            .CountAsync(cancellationToken);

        var expiredCount = await _db.MedicineBatches.AsNoTracking()
            .Where(b => b.CurrentQuantity > 0 && b.ExpiryDate <= today &&
                        (!branchId.HasValue || b.BranchId == branchId))
            .Select(b => b.MedicineId)
            .Distinct()
            .CountAsync(cancellationToken);

        var recent = await sales
            .OrderByDescending(s => s.SaleDateUtc)
            .Take(10)
            .Select(s => new RecentSaleDto
            {
                Id = s.Id,
                InvoiceNumber = s.InvoiceNumber,
                CustomerName = s.Customer != null ? s.Customer.FirstName + " " + s.Customer.LastName : null,
                TotalAmount = s.TotalAmount,
                Status = s.Status,
                SaleDateUtc = s.SaleDateUtc
            })
            .ToListAsync(cancellationToken);

        var dailyStart = today.AddDays(-6);
        var dailyRows = await sales
            .Where(s => s.SaleDateUtc >= dailyStart && s.SaleDateUtc < tomorrow)
            .GroupBy(s => s.SaleDateUtc.Date)
            .Select(g => new { Date = g.Key, Total = g.Sum(s => s.TotalAmount) })
            .ToListAsync(cancellationToken);

        var dailySales = new List<ChartPointDto>();
        for (var i = 0; i < 7; i++)
        {
            var date = dailyStart.AddDays(i);
            dailySales.Add(new ChartPointDto
            {
                Label = date.ToString("MMM dd"),
                Value = dailyRows.FirstOrDefault(r => r.Date == date)?.Total ?? 0m
            });
        }

        var monthlyStart = new DateTime(now.Year, now.Month, 1).AddMonths(-11);
        var monthlySalesRows = await sales
            .Where(s => s.SaleDateUtc >= monthlyStart)
            .GroupBy(s => new { s.SaleDateUtc.Year, s.SaleDateUtc.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(s => s.TotalAmount) })
            .ToListAsync(cancellationToken);

        var monthlyPurchaseRows = await purchases
            .Where(r => r.ReceivedAtUtc >= monthlyStart)
            .GroupBy(r => new { r.ReceivedAtUtc.Year, r.ReceivedAtUtc.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(r => r.TotalAmount) })
            .ToListAsync(cancellationToken);

        var monthlySales = new List<ChartPointDto>();
        var monthlyPurchases = new List<ChartPointDto>();
        for (var i = 0; i < 12; i++)
        {
            var month = monthlyStart.AddMonths(i);
            var label = month.ToString("MMM yyyy");
            monthlySales.Add(new ChartPointDto
            {
                Label = label,
                Value = monthlySalesRows.FirstOrDefault(r => r.Year == month.Year && r.Month == month.Month)?.Total ?? 0m
            });
            monthlyPurchases.Add(new ChartPointDto
            {
                Label = label,
                Value = monthlyPurchaseRows.FirstOrDefault(r => r.Year == month.Year && r.Month == month.Month)?.Total ?? 0m
            });
        }

        var topStart = today.AddDays(-30);
        var topRows = await _db.SaleDetails.AsNoTracking()
            .Where(d => d.Sale.Status != SaleStatus.Cancelled && d.Sale.SaleDateUtc >= topStart &&
                        (!branchId.HasValue || d.Sale.BranchId == branchId))
            .GroupBy(d => d.MedicineId)
            .Select(g => new
            {
                MedicineId = g.Key,
                Quantity = g.Sum(d => d.Quantity),
                Revenue = g.Sum(d => d.LineTotal)
            })
            .OrderByDescending(x => x.Quantity)
            .Take(5)
            .ToListAsync(cancellationToken);

        var topMedicineIds = topRows.Select(r => r.MedicineId).ToList();
        var topNames = await _db.Medicines.AsNoTracking()
            .Where(m => topMedicineIds.Contains(m.Id))
            .ToDictionaryAsync(m => m.Id, m => m.Name, cancellationToken);

        var topSelling = topRows.Select(r => new TopSellingDto
        {
            MedicineId = r.MedicineId,
            MedicineName = topNames.GetValueOrDefault(r.MedicineId, string.Empty),
            QuantitySold = r.Quantity,
            Revenue = r.Revenue
        }).ToList();

        return new DashboardDto
        {
            TotalSalesToday = salesToday,
            TotalSalesThisMonth = salesMonth,
            TotalPurchases = purchasesTotal,
            TotalPurchasesThisMonth = purchasesMonth,
            TotalProfit = profitTotal,
            TotalProfitThisMonth = profitMonth,
            TotalProducts = totalProducts,
            LowStockProducts = lowStockCount,
            ExpiringMedicines = expiringCount,
            ExpiredMedicines = expiredCount,
            TotalSuppliers = totalSuppliers,
            TotalCustomers = totalCustomers,
            PendingPurchaseOrders = pendingOrders,
            RecentTransactions = recent,
            DailySales = dailySales,
            MonthlySales = monthlySales,
            MonthlyPurchases = monthlyPurchases,
            TopSelling = topSelling,
            LowStockItems = await _inventory.GetLowStockAsync(branchId, 10, cancellationToken),
            ExpiringItems = await _inventory.GetExpiringAsync(90, branchId, 10, cancellationToken)
        };
    }
}
