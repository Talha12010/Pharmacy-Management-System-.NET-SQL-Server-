using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Expenses;
using PharmacyManagement.Application.Interfaces;

namespace PharmacyManagement.API.Controllers;

[Route("api/expenses")]
public sealed class ExpensesController : ApiControllerBase
{
    private readonly IExpenseService _expenses;

    public ExpensesController(IExpenseService expenses) => _expenses = expenses;

    [HttpGet]
    [Authorize(Policy = Permissions.ExpensesView)]
    public async Task<ActionResult<PagedResult<ExpenseDto>>> GetPaged(
        [FromQuery] QueryParameters query,
        [FromQuery] int? categoryId,
        [FromQuery] int? branchId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
        => Ok(await _expenses.GetPagedAsync(query, categoryId, branchId, from, to, Ct));

    [HttpGet("{id:int}")]
    [Authorize(Policy = Permissions.ExpensesView)]
    public async Task<ActionResult<ExpenseDto>> GetById(int id) => Ok(await _expenses.GetByIdAsync(id, Ct));

    [HttpPost]
    [Authorize(Policy = Permissions.ExpensesManage)]
    public async Task<ActionResult<ExpenseDto>> Create([FromBody] CreateExpenseRequest request)
        => Ok(await _expenses.CreateAsync(request, Ct));

    [HttpPut("{id:int}")]
    [Authorize(Policy = Permissions.ExpensesManage)]
    public async Task<ActionResult<ExpenseDto>> Update(int id, [FromBody] UpdateExpenseRequest request)
        => Ok(await _expenses.UpdateAsync(id, request, Ct));

    [HttpDelete("{id:int}")]
    [Authorize(Policy = Permissions.ExpensesManage)]
    public async Task<IActionResult> Delete(int id)
    {
        await _expenses.DeleteAsync(id, Ct);
        return NoContent();
    }

    [HttpPost("{id:int}/attachment")]
    [Authorize(Policy = Permissions.ExpensesManage)]
    public async Task<ActionResult<ExpenseDto>> UploadAttachment(int id, IFormFile file)
    {
        var (stream, fileName) = await ReadUploadAsync(file);
        await using (stream)
        {
            var path = await _expenses.UploadAttachmentAsync(id, stream, fileName, Ct);
            return Ok(new { attachmentPath = path });
        }
    }
}

[Route("api/expense-categories")]
public sealed class ExpenseCategoriesController : ApiControllerBase
{
    private readonly IExpenseService _expenses;

    public ExpenseCategoriesController(IExpenseService expenses) => _expenses = expenses;

    [HttpGet]
    [Authorize(Policy = Permissions.ExpensesView)]
    public async Task<ActionResult<IReadOnlyList<ExpenseCategoryDto>>> GetAll([FromQuery] bool includeInactive = false)
        => Ok(await _expenses.GetCategoriesAsync(includeInactive, Ct));

    [HttpPost]
    [Authorize(Policy = Permissions.ExpensesManage)]
    public async Task<ActionResult<ExpenseCategoryDto>> Create([FromBody] CreateExpenseCategoryRequest request)
        => Ok(await _expenses.CreateCategoryAsync(request, Ct));

    [HttpPut("{id:int}")]
    [Authorize(Policy = Permissions.ExpensesManage)]
    public async Task<ActionResult<ExpenseCategoryDto>> Update(int id, [FromBody] CreateExpenseCategoryRequest request)
        => Ok(await _expenses.UpdateCategoryAsync(id, request, Ct));

    [HttpDelete("{id:int}")]
    [Authorize(Policy = Permissions.ExpensesManage)]
    public async Task<IActionResult> Delete(int id)
    {
        await _expenses.DeleteCategoryAsync(id, Ct);
        return NoContent();
    }
}
