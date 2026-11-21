using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Expenses;

namespace PharmacyManagement.Application.Interfaces;

public interface IExpenseService
{
    Task<PagedResult<ExpenseDto>> GetPagedAsync(
        QueryParameters query, int? categoryId, int? branchId, DateTime? from, DateTime? to,
        CancellationToken cancellationToken = default);

    Task<ExpenseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ExpenseDto> CreateAsync(CreateExpenseRequest request, CancellationToken cancellationToken = default);
    Task<ExpenseDto> UpdateAsync(int id, UpdateExpenseRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<string> UploadAttachmentAsync(int id, Stream content, string fileName, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ExpenseCategoryDto>> GetCategoriesAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<ExpenseCategoryDto> CreateCategoryAsync(CreateExpenseCategoryRequest request, CancellationToken cancellationToken = default);
    Task<ExpenseCategoryDto> UpdateCategoryAsync(int id, CreateExpenseCategoryRequest request, CancellationToken cancellationToken = default);
    Task DeleteCategoryAsync(int id, CancellationToken cancellationToken = default);
}
