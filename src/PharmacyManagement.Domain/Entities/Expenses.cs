using PharmacyManagement.Domain.Common;
using PharmacyManagement.Domain.Enums;

namespace PharmacyManagement.Domain.Entities;

public class ExpenseCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}

public class Expense : SoftDeletableEntity
{
    public string ExpenseNumber { get; set; } = string.Empty;
    public int ExpenseCategoryId { get; set; }
    public ExpenseCategory ExpenseCategory { get; set; } = null!;
    public int BranchId { get; set; }
    public Branch Branch { get; set; } = null!;
    public DateTime ExpenseDateUtc { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? AttachmentPath { get; set; }
    public Guid RecordedByUserId { get; set; }
}
