using PharmacyManagement.Application.DTOs.Expenses;
using PharmacyManagement.Domain.Entities;

namespace PharmacyManagement.Application.Mappings;

public static class ExpenseMappings
{
    public static ExpenseDto ToDto(this Expense expense) => new()
    {
        Id = expense.Id,
        ExpenseNumber = expense.ExpenseNumber,
        ExpenseCategoryId = expense.ExpenseCategoryId,
        CategoryName = expense.ExpenseCategory?.Name ?? string.Empty,
        BranchId = expense.BranchId,
        BranchName = expense.Branch?.Name ?? string.Empty,
        ExpenseDateUtc = expense.ExpenseDateUtc,
        Amount = expense.Amount,
        PaymentMethod = expense.PaymentMethod,
        Description = expense.Description,
        AttachmentPath = expense.AttachmentPath,
        RecordedByUserId = expense.RecordedByUserId,
        CreatedAtUtc = expense.CreatedAtUtc
    };

    public static ExpenseCategoryDto ToDto(this ExpenseCategory category, int expenseCount = 0) => new()
    {
        Id = category.Id,
        Name = category.Name,
        Description = category.Description,
        IsActive = category.IsActive,
        ExpenseCount = expenseCount
    };
}
