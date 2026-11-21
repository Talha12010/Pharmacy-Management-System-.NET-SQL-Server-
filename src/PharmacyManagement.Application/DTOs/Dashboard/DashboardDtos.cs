using PharmacyManagement.Domain.Enums;

namespace PharmacyManagement.Application.DTOs.Dashboard;

public sealed class DashboardDto
{
    public decimal TotalSalesToday { get; set; }
    public decimal TotalSalesThisMonth { get; set; }
    public decimal TotalPurchases { get; set; }
    public decimal TotalPurchasesThisMonth { get; set; }
    public decimal TotalProfit { get; set; }
    public decimal TotalProfitThisMonth { get; set; }
    public int TotalProducts { get; set; }
    public int LowStockProducts { get; set; }
    public int ExpiringMedicines { get; set; }
    public int ExpiredMedicines { get; set; }
    public int TotalSuppliers { get; set; }
    public int TotalCustomers { get; set; }
    public int PendingPurchaseOrders { get; set; }
    public IReadOnlyList<RecentSaleDto> RecentTransactions { get; set; } = Array.Empty<RecentSaleDto>();
    public IReadOnlyList<ChartPointDto> DailySales { get; set; } = Array.Empty<ChartPointDto>();
    public IReadOnlyList<ChartPointDto> MonthlySales { get; set; } = Array.Empty<ChartPointDto>();
    public IReadOnlyList<ChartPointDto> MonthlyPurchases { get; set; } = Array.Empty<ChartPointDto>();
    public IReadOnlyList<TopSellingDto> TopSelling { get; set; } = Array.Empty<TopSellingDto>();
    public IReadOnlyList<LowStockItemDto> LowStockItems { get; set; } = Array.Empty<LowStockItemDto>();
    public IReadOnlyList<ExpiringItemDto> ExpiringItems { get; set; } = Array.Empty<ExpiringItemDto>();
}

public sealed class ChartPointDto
{
    public string Label { get; set; } = string.Empty;
    public decimal Value { get; set; }
}

public sealed class TopSellingDto
{
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
    public decimal Revenue { get; set; }
}

public sealed class RecentSaleDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public decimal TotalAmount { get; set; }
    public SaleStatus Status { get; set; }
    public DateTime SaleDateUtc { get; set; }
}

public sealed class LowStockItemDto
{
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int ReorderLevel { get; set; }
}

public sealed class ExpiringItemDto
{
    public int BatchId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public string BatchNumber { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public int Quantity { get; set; }
    public int DaysToExpiry { get; set; }
}
