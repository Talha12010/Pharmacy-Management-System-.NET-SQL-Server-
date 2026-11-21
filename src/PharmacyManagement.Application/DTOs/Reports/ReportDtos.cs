namespace PharmacyManagement.Application.DTOs.Reports;

public sealed class ReportFilter
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int? BranchId { get; set; }
    public int? CategoryId { get; set; }
    public int? SupplierId { get; set; }
    public int? CustomerId { get; set; }
    public int? ExpiringDays { get; set; }
    public string? GroupBy { get; set; }
}

public static class ReportTypes
{
    public const string Sales = "sales";
    public const string ProfitLoss = "profit-loss";
    public const string Purchases = "purchases";
    public const string Inventory = "inventory";
    public const string StockValuation = "stock-valuation";
    public const string Expiry = "expiry";
    public const string LowStock = "low-stock";
    public const string Suppliers = "suppliers";
    public const string Customers = "customers";
    public const string Expenses = "expenses";
    public const string Returns = "returns";
    public const string Payments = "payments";
    public const string EmployeeActivity = "employee-activity";

    public static readonly IReadOnlyList<string> All = new[]
    {
        Sales, ProfitLoss, Purchases, Inventory, StockValuation, Expiry, LowStock,
        Suppliers, Customers, Expenses, Returns, Payments, EmployeeActivity
    };
}
