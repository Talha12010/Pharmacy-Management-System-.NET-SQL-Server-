namespace PharmacyManagement.Domain.Enums;

public enum PurchaseOrderStatus
{
    Draft = 1,
    PendingApproval = 2,
    Approved = 3,
    PartiallyReceived = 4,
    Received = 5,
    Rejected = 6,
    Cancelled = 7
}
