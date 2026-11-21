using PharmacyManagement.Domain.Enums;

namespace PharmacyManagement.Application.DTOs.Expenses;

public sealed class ExpenseDto
{
    public int Id { get; set; }
    public string ExpenseNumber { get; set; } = string.Empty;
    public int ExpenseCategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public DateTime ExpenseDateUtc { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? AttachmentPath { get; set; }
    public Guid RecordedByUserId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class CreateExpenseRequest
{
    public int ExpenseCategoryId { get; set; }
    public int BranchId { get; set; }
    public DateTime ExpenseDateUtc { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string Description { get; set; } = string.Empty;
}

public sealed class UpdateExpenseRequest : CreateExpenseRequest
{
}

public sealed class ExpenseCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int ExpenseCount { get; set; }
}

public sealed class CreateExpenseCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
