using PharmacyManagement.Domain.Enums;

namespace PharmacyManagement.Application.DTOs.Purchasing;

public sealed class PurchaseOrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public DateTime OrderDateUtc { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public PurchaseOrderStatus Status { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public Guid CreatedByUserId { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string? RejectionReason { get; set; }
    public IReadOnlyList<PurchaseOrderDetailDto> Details { get; set; } = Array.Empty<PurchaseOrderDetailDto>();
    public IReadOnlyList<GoodsReceiptSummaryDto> Receipts { get; set; } = Array.Empty<GoodsReceiptSummaryDto>();
}

public sealed class PurchaseOrderDetailDto
{
    public int Id { get; set; }
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public int OrderedQuantity { get; set; }
    public int ReceivedQuantity { get; set; }
    public int PendingQuantity => Math.Max(0, OrderedQuantity - ReceivedQuantity);
    public decimal UnitPrice { get; set; }
    public decimal TaxPercentage { get; set; }
    public decimal LineTotal { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime? ManufacturingDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
}

public sealed class PurchaseOrderListItemDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public DateTime OrderDateUtc { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public PurchaseOrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public int ItemCount { get; set; }
}

public sealed class CreatePurchaseOrderRequest
{
    public int SupplierId { get; set; }
    public int BranchId { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public decimal DiscountAmount { get; set; }
    public string? Notes { get; set; }
    public List<PurchaseOrderItemRequest> Items { get; set; } = new();
}

public sealed class PurchaseOrderItemRequest
{
    public int MedicineId { get; set; }
    public int OrderedQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxPercentage { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime? ManufacturingDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
}

public sealed class ReceivePurchaseOrderRequest
{
    public string? Notes { get; set; }
    public List<ReceivePurchaseOrderItemRequest> Items { get; set; } = new();
}

public sealed class ReceivePurchaseOrderItemRequest
{
    public int PurchaseOrderDetailId { get; set; }
    public int Quantity { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public DateTime ManufacturingDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public decimal? UnitCost { get; set; }
}

public sealed class GoodsReceiptDto
{
    public int Id { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public int PurchaseOrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public DateTime ReceivedAtUtc { get; set; }
    public Guid ReceivedByUserId { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public IReadOnlyList<GoodsReceiptDetailDto> Details { get; set; } = Array.Empty<GoodsReceiptDetailDto>();
}

public sealed class GoodsReceiptSummaryDto
{
    public int Id { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public DateTime ReceivedAtUtc { get; set; }
    public decimal TotalAmount { get; set; }
}

public sealed class GoodsReceiptDetailDto
{
    public int Id { get; set; }
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public int BatchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public DateTime ManufacturingDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal LineTotal { get; set; }
}

public sealed class RejectRequest
{
    public string Reason { get; set; } = string.Empty;
}
