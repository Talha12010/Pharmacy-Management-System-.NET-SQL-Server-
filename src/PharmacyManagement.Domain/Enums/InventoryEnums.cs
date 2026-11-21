namespace PharmacyManagement.Domain.Enums;

public enum InventoryTransactionType
{
    StockIn = 1,
    StockOut = 2,
    AdjustmentIncrease = 3,
    AdjustmentDecrease = 4,
    TransferOut = 5,
    TransferIn = 6,
    Sale = 7,
    SaleReturn = 8,
    PurchaseReceipt = 9,
    PurchaseReturn = 10,
    Damaged = 11,
    Expired = 12,
    SaleCancellation = 13
}

public enum StockTransferStatus
{
    Draft = 1,
    PendingApproval = 2,
    Approved = 3,
    InTransit = 4,
    Completed = 5,
    Cancelled = 6
}
