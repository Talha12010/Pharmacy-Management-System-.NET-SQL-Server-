namespace PharmacyManagement.Domain.Enums;

public enum ReturnType
{
    SaleReturn = 1,
    PurchaseReturn = 2
}

public enum ReturnStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Completed = 4,
    Cancelled = 5
}

public enum ReturnCondition
{
    Resalable = 1,
    Damaged = 2,
    Expired = 3,
    Opened = 4,
    Unknown = 5
}

public enum RestockDecision
{
    PendingInspection = 1,
    Restock = 2,
    Discard = 3,
    ReturnToSupplier = 4
}
