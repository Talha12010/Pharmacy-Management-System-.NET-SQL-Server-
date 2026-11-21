namespace PharmacyManagement.Domain.Enums;

public enum PaymentMethod
{
    Cash = 1,
    Card = 2,
    BankTransfer = 3,
    MobileWallet = 4,
    Credit = 5
}

public enum SaleStatus
{
    Completed = 1,
    PartiallyReturned = 2,
    Returned = 3,
    Cancelled = 4
}

public enum PaymentStatus
{
    Unpaid = 1,
    PartiallyPaid = 2,
    Paid = 3,
    Refunded = 4
}
