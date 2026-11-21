using PharmacyManagement.Domain.Enums;

namespace PharmacyManagement.Application.DTOs.Inventory;

public sealed class BatchDto
{
    public int Id { get; set; }
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public DateTime ManufacturingDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SellingPrice { get; set; }
    public int InitialQuantity { get; set; }
    public int CurrentQuantity { get; set; }
    public DateTime ReceivedAtUtc { get; set; }
    public bool IsActive { get; set; }
    public bool IsExpired { get; set; }
    public int DaysToExpiry { get; set; }
}

public sealed class StockSummaryDto
{
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string UnitAbbreviation { get; set; } = string.Empty;
    public int TotalQuantity { get; set; }
    public int ReorderLevel { get; set; }
    public bool IsLowStock { get; set; }
    public DateTime? NearestExpiryDate { get; set; }
}

public sealed class InventoryTransactionDto
{
    public long Id { get; set; }
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public string? BatchNumber { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public InventoryTransactionType Type { get; set; }
    public int QuantityChange { get; set; }
    public int QuantityBefore { get; set; }
    public int QuantityAfter { get; set; }
    public decimal? UnitCost { get; set; }
    public string? ReferenceType { get; set; }
    public int? ReferenceId { get; set; }
    public string? Notes { get; set; }
    public Guid? PerformedByUserId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public sealed class StockInRequest
{
    public int MedicineId { get; set; }
    public int BranchId { get; set; }
    public int? SupplierId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public DateTime ManufacturingDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public int Quantity { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SellingPrice { get; set; }
    public string? Notes { get; set; }
}

public sealed class StockOutRequest
{
    public int BatchId { get; set; }
    public int Quantity { get; set; }
    public InventoryTransactionType Type { get; set; } = InventoryTransactionType.StockOut;
    public string Reason { get; set; } = string.Empty;
}

public sealed class StockAdjustmentRequest
{
    public int BatchId { get; set; }
    public int NewQuantity { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public sealed class StockTransferDto
{
    public int Id { get; set; }
    public string TransferNumber { get; set; } = string.Empty;
    public int FromBranchId { get; set; }
    public string FromBranchName { get; set; } = string.Empty;
    public int ToBranchId { get; set; }
    public string ToBranchName { get; set; } = string.Empty;
    public StockTransferStatus Status { get; set; }
    public Guid? RequestedByUserId { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public DateTime TransferDateUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string? Notes { get; set; }
    public IReadOnlyList<StockTransferDetailDto> Details { get; set; } = Array.Empty<StockTransferDetailDto>();
}

public sealed class StockTransferDetailDto
{
    public int Id { get; set; }
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public int BatchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public int Quantity { get; set; }
    public int ReceivedQuantity { get; set; }
}

public sealed class CreateStockTransferRequest
{
    public int FromBranchId { get; set; }
    public int ToBranchId { get; set; }
    public string? Notes { get; set; }
    public List<StockTransferItemRequest> Items { get; set; } = new();
}

public sealed class StockTransferItemRequest
{
    public int BatchId { get; set; }
    public int Quantity { get; set; }
}
