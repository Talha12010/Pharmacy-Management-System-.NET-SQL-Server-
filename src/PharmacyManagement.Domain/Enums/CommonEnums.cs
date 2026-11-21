namespace PharmacyManagement.Domain.Enums;

public enum Gender
{
    Male = 1,
    Female = 2,
    Other = 3
}

public enum EmployeeStatus
{
    Active = 1,
    OnLeave = 2,
    Terminated = 3
}

public enum DocumentType
{
    Sale = 1,
    PurchaseOrder = 2,
    GoodsReceipt = 3,
    Return = 4,
    StockTransfer = 5,
    Prescription = 6,
    Expense = 7
}
