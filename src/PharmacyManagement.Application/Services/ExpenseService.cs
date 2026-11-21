using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.Common.Interfaces;
using PharmacyManagement.Application.DTOs.Expenses;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Application.Mappings;
using PharmacyManagement.Domain.Entities;
using PharmacyManagement.Domain.Enums;
using PharmacyManagement.Domain.Exceptions;

namespace PharmacyManagement.Application.Services;

public sealed class ExpenseService : IExpenseService
{
    private readonly IApplicationDbContext _db;
    private readonly IDocumentNumberService _documents;
    private readonly IFileStorageService _storage;
    private readonly IAuditService _audit;
    private readonly ICurrentUserService _currentUser;

    public ExpenseService(
        IApplicationDbContext db,
        IDocumentNumberService documents,
        IFileStorageService storage,
        IAuditService audit,
        ICurrentUserService currentUser)
    {
        _db = db;
        _documents = documents;
        _storage = storage;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<ExpenseDto>> GetPagedAsync(
        QueryParameters query, int? categoryId, int? branchId, DateTime? from, DateTime? to,
        CancellationToken cancellationToken = default)
    {
        var expenses = _db.Expenses.AsNoTracking()
            .Include(e => e.ExpenseCategory)
            .Include(e => e.Branch)
            .AsQueryable();

        if (categoryId.HasValue)
        {
            expenses = expenses.Where(e => e.ExpenseCategoryId == categoryId);
        }

        if (branchId.HasValue)
        {
            expenses = expenses.Where(e => e.BranchId == branchId);
        }

        if (from.HasValue)
        {
            var fromUtc = from.Value.Date;
            expenses = expenses.Where(e => e.ExpenseDateUtc >= fromUtc);
        }

        if (to.HasValue)
        {
            var toUtc = to.Value.Date.AddDays(1);
            expenses = expenses.Where(e => e.ExpenseDateUtc < toUtc);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            expenses = expenses.Where(e =>
                e.ExpenseNumber.Contains(term) ||
                e.Description.Contains(term) ||
                e.ExpenseCategory.Name.Contains(term));
        }

        var total = await expenses.CountAsync(cancellationToken);
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 200);
        var items = await expenses
            .OrderByDescending(e => e.ExpenseDateUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<ExpenseDto>.Create(items.Select(e => e.ToDto()).ToList(), page, pageSize, total);
    }

    public async Task<ExpenseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var expense = await _db.Expenses.AsNoTracking()
            .Include(e => e.ExpenseCategory)
            .Include(e => e.Branch)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken)
            ?? throw new NotFoundException("Expense", id);

        return expense.ToDto();
    }

    public async Task<ExpenseDto> CreateAsync(CreateExpenseRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAsync(request, cancellationToken);

        var expense = new Expense
        {
            ExpenseNumber = await _documents.NextAsync(DocumentType.Expense, DateTime.UtcNow, cancellationToken),
            ExpenseCategoryId = request.ExpenseCategoryId,
            BranchId = request.BranchId,
            ExpenseDateUtc = request.ExpenseDateUtc.Date,
            Amount = request.Amount,
            PaymentMethod = request.PaymentMethod,
            Description = request.Description.Trim(),
            RecordedByUserId = _currentUser.UserId ?? Guid.Empty,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.Expenses.Add(expense);
        await _db.SaveChangesAsync(cancellationToken);
        var dto = await GetByIdAsync(expense.Id, cancellationToken);
        await _audit.LogAsync("ExpenseCreated", nameof(Expense), expense.Id.ToString(), null, dto, cancellationToken);
        return dto;
    }

    public async Task<ExpenseDto> UpdateAsync(int id, UpdateExpenseRequest request, CancellationToken cancellationToken = default)
    {
        var expense = await _db.Expenses.FirstOrDefaultAsync(e => e.Id == id, cancellationToken)
            ?? throw new NotFoundException("Expense", id);

        await ValidateAsync(request, cancellationToken);
        var before = await GetByIdAsync(id, cancellationToken);

        expense.ExpenseCategoryId = request.ExpenseCategoryId;
        expense.BranchId = request.BranchId;
        expense.ExpenseDateUtc = request.ExpenseDateUtc.Date;
        expense.Amount = request.Amount;
        expense.PaymentMethod = request.PaymentMethod;
        expense.Description = request.Description.Trim();

        await _db.SaveChangesAsync(cancellationToken);
        var after = await GetByIdAsync(id, cancellationToken);
        await _audit.LogAsync("ExpenseUpdated", nameof(Expense), id.ToString(), before, after, cancellationToken);
        return after;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var expense = await _db.Expenses.FirstOrDefaultAsync(e => e.Id == id, cancellationToken)
            ?? throw new NotFoundException("Expense", id);

        expense.IsDeleted = true;
        expense.DeletedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("ExpenseDeleted", nameof(Expense), id.ToString(), null, new { expense.ExpenseNumber, expense.Amount }, cancellationToken);
    }

    public async Task<string> UploadAttachmentAsync(int id, Stream content, string fileName, CancellationToken cancellationToken = default)
    {
        var expense = await _db.Expenses.FirstOrDefaultAsync(e => e.Id == id, cancellationToken)
            ?? throw new NotFoundException("Expense", id);

        var path = await _storage.SaveAsync(content, fileName, "expenses", cancellationToken);
        var previous = expense.AttachmentPath;
        expense.AttachmentPath = path;
        await _db.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(previous))
        {
            await _storage.DeleteAsync(previous, cancellationToken);
        }

        await _audit.LogAsync("ExpenseAttachmentUploaded", nameof(Expense), id.ToString(), null, new { AttachmentPath = path }, cancellationToken);
        return path;
    }

    public async Task<IReadOnlyList<ExpenseCategoryDto>> GetCategoriesAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = _db.ExpenseCategories.AsNoTracking();
        if (!includeInactive)
        {
            query = query.Where(c => c.IsActive);
        }

        return await query
            .OrderBy(c => c.Name)
            .Select(c => new ExpenseCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive,
                ExpenseCount = c.Expenses.Count()
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ExpenseCategoryDto> CreateCategoryAsync(CreateExpenseCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();
        if (await _db.ExpenseCategories.AnyAsync(c => c.Name == name, cancellationToken))
        {
            throw new ConflictException($"Expense category '{name}' already exists.");
        }

        var category = new ExpenseCategory
        {
            Name = name,
            Description = request.Description?.Trim(),
            IsActive = request.IsActive
        };

        _db.ExpenseCategories.Add(category);
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("ExpenseCategoryCreated", nameof(ExpenseCategory), category.Id.ToString(), null, category.ToDto(), cancellationToken);
        return category.ToDto();
    }

    public async Task<ExpenseCategoryDto> UpdateCategoryAsync(int id, CreateExpenseCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = await _db.ExpenseCategories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new NotFoundException("ExpenseCategory", id);

        var name = request.Name.Trim();
        if (await _db.ExpenseCategories.AnyAsync(c => c.Id != id && c.Name == name, cancellationToken))
        {
            throw new ConflictException($"Expense category '{name}' already exists.");
        }

        var before = category.ToDto();
        category.Name = name;
        category.Description = request.Description?.Trim();
        category.IsActive = request.IsActive;
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("ExpenseCategoryUpdated", nameof(ExpenseCategory), id.ToString(), before, category.ToDto(), cancellationToken);
        return category.ToDto();
    }

    public async Task DeleteCategoryAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _db.ExpenseCategories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new NotFoundException("ExpenseCategory", id);

        if (await _db.Expenses.IgnoreQueryFilters().AnyAsync(e => e.ExpenseCategoryId == id, cancellationToken))
        {
            throw new BusinessRuleException("This category is used by expenses and cannot be deleted. Deactivate it instead.");
        }

        _db.ExpenseCategories.Remove(category);
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("ExpenseCategoryDeleted", nameof(ExpenseCategory), id.ToString(), category.ToDto(), null, cancellationToken);
    }

    private async Task ValidateAsync(CreateExpenseRequest request, CancellationToken cancellationToken)
    {
        if (!await _db.ExpenseCategories.AnyAsync(c => c.Id == request.ExpenseCategoryId && c.IsActive, cancellationToken))
        {
            throw new NotFoundException("Active expense category", request.ExpenseCategoryId);
        }

        if (!await _db.Branches.AnyAsync(b => b.Id == request.BranchId, cancellationToken))
        {
            throw new NotFoundException("Branch", request.BranchId);
        }

        if (request.Amount <= 0)
        {
            throw new BusinessRuleException("Expense amount must be greater than zero.");
        }

        if (request.ExpenseDateUtc.Date > DateTime.UtcNow.Date.AddDays(1))
        {
            throw new BusinessRuleException("Expense date cannot be in the future.");
        }
    }
}
