using PharmacyManagement.Domain.Enums;

namespace PharmacyManagement.Application.DTOs.Returns;

public sealed class ReturnDto
{
    public int Id { get; set; }
    public string ReturnNumber { get; set; } = string.Empty;
    public ReturnType Type { get; set; }
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int? OriginalSaleId { get; set; }
    public string? InvoiceNumber { get; set; }
    public int? OriginalPurchaseOrderId { get; set; }
    public string? OrderNumber { get; set; }
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public int? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public string Reason { get; set; } = string.Empty;
    public ReturnStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public PaymentMethod? RefundMethod { get; set; }
    public string? Notes { get; set; }
    public Guid RequestedByUserId { get; set; }
    public DateTime RequestedAtUtc { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAtUtc { get; set; }
    public string? RejectionReason { get; set; }
    public IReadOnlyList<ReturnDetailDto> Details { get; set; } = Array.Empty<ReturnDetailDto>();
    public IReadOnlyList<RefundDto> Refunds { get; set; } = Array.Empty<RefundDto>();
}

public sealed class ReturnDetailDto
{
    public int Id { get; set; }
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public int BatchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public ReturnCondition Condition { get; set; }
    public RestockDecision RestockDecision { get; set; }
    public Guid? InspectedByUserId { get; set; }
    public DateTime? InspectedAtUtc { get; set; }
    public string? Notes { get; set; }
}

public sealed class RefundDto
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public DateTime RefundedAtUtc { get; set; }
    public Guid ProcessedByUserId { get; set; }
    public string? Notes { get; set; }
}

public sealed class CreateReturnRequest
{
    public ReturnType Type { get; set; }
    public int BranchId { get; set; }
    public int? OriginalSaleId { get; set; }
    public int? OriginalPurchaseOrderId { get; set; }
    public int? CustomerId { get; set; }
    public int? SupplierId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public PaymentMethod? RefundMethod { get; set; }
    public string? Notes { get; set; }
    public List<ReturnItemRequest> Items { get; set; } = new();
}

public sealed class ReturnItemRequest
{
    public int MedicineId { get; set; }
    public int BatchId { get; set; }
    public int Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public ReturnCondition Condition { get; set; } = ReturnCondition.Unknown;
    public RestockDecision RestockDecision { get; set; } = RestockDecision.PendingInspection;
    public string? Notes { get; set; }
}

public sealed class ApproveReturnRequest
{
    public PaymentMethod? RefundMethod { get; set; }
    public string? Notes { get; set; }
    public List<ReturnInspectionItem>? Inspections { get; set; }
}

public sealed class ReturnInspectionItem
{
    public int ReturnDetailId { get; set; }
    public ReturnCondition Condition { get; set; }
    public RestockDecision RestockDecision { get; set; }
    public string? Notes { get; set; }
}

public sealed class RejectReturnRequest
{
    public string Reason { get; set; } = string.Empty;
}
