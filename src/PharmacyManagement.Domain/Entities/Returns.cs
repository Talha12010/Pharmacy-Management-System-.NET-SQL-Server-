using PharmacyManagement.Domain.Enums;

namespace PharmacyManagement.Domain.Entities;

public class ReturnRecord
{
    public int Id { get; set; }
    public string ReturnNumber { get; set; } = string.Empty;
    public ReturnType Type { get; set; }
    public int BranchId { get; set; }
    public Branch Branch { get; set; } = null!;
    public int? OriginalSaleId { get; set; }
    public Sale? OriginalSale { get; set; }
    public int? OriginalPurchaseOrderId { get; set; }
    public PurchaseOrder? OriginalPurchaseOrder { get; set; }
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public int? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public string Reason { get; set; } = string.Empty;
    public ReturnStatus Status { get; set; } = ReturnStatus.Pending;
    public decimal TotalAmount { get; set; }
    public PaymentMethod? RefundMethod { get; set; }
    public string? Notes { get; set; }
    public Guid RequestedByUserId { get; set; }
    public DateTime RequestedAtUtc { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAtUtc { get; set; }
    public string? RejectionReason { get; set; }

    public ICollection<ReturnDetail> Details { get; set; } = new List<ReturnDetail>();
    public ICollection<Refund> Refunds { get; set; } = new List<Refund>();
}

public class ReturnDetail
{
    public int Id { get; set; }
    public int ReturnId { get; set; }
    public ReturnRecord Return { get; set; } = null!;
    public int MedicineId { get; set; }
    public Medicine Medicine { get; set; } = null!;
    public int BatchId { get; set; }
    public MedicineBatch Batch { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public ReturnCondition Condition { get; set; } = ReturnCondition.Unknown;
    public RestockDecision RestockDecision { get; set; } = RestockDecision.PendingInspection;
    public Guid? InspectedByUserId { get; set; }
    public DateTime? InspectedAtUtc { get; set; }
    public string? Notes { get; set; }
}

public class Refund
{
    public int Id { get; set; }
    public int ReturnId { get; set; }
    public ReturnRecord Return { get; set; } = null!;
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public DateTime RefundedAtUtc { get; set; }
    public Guid ProcessedByUserId { get; set; }
    public string? Notes { get; set; }
}
