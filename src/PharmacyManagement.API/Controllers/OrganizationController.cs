using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Audit;
using PharmacyManagement.Application.DTOs.Catalog;
using PharmacyManagement.Application.DTOs.Organization;
using PharmacyManagement.Application.Interfaces;

namespace PharmacyManagement.API.Controllers;

[Route("api/branches")]
public sealed class BranchesController : ApiControllerBase
{
    private readonly IBranchService _branches;

    public BranchesController(IBranchService branches) => _branches = branches;

    [HttpGet]
    [Authorize(Policy = Permissions.BranchesView)]
    public async Task<ActionResult<IReadOnlyList<BranchDto>>> GetAll([FromQuery] bool includeInactive = false)
        => Ok(await _branches.GetAllAsync(includeInactive, Ct));

    [HttpGet("lookup")]
    [Authorize(Policy = Permissions.BranchesView)]
    public async Task<ActionResult<IReadOnlyList<LookupDto>>> GetLookup()
        => Ok(await _branches.GetLookupAsync(Ct));

    [HttpGet("{id:int}")]
    [Authorize(Policy = Permissions.BranchesView)]
    public async Task<ActionResult<BranchDto>> GetById(int id) => Ok(await _branches.GetByIdAsync(id, Ct));

    [HttpPost]
    [Authorize(Policy = Permissions.BranchesManage)]
    public async Task<ActionResult<BranchDto>> Create([FromBody] CreateBranchRequest request)
        => Ok(await _branches.CreateAsync(request, Ct));

    [HttpPut("{id:int}")]
    [Authorize(Policy = Permissions.BranchesManage)]
    public async Task<ActionResult<BranchDto>> Update(int id, [FromBody] UpdateBranchRequest request)
        => Ok(await _branches.UpdateAsync(id, request, Ct));

    [HttpDelete("{id:int}")]
    [Authorize(Policy = Permissions.BranchesManage)]
    public async Task<IActionResult> Deactivate(int id)
    {
        await _branches.DeactivateAsync(id, Ct);
        return NoContent();
    }
}

[Route("api/employees")]
public sealed class EmployeesController : ApiControllerBase
{
    private readonly IEmployeeService _employees;

    public EmployeesController(IEmployeeService employees) => _employees = employees;

    [HttpGet]
    [Authorize(Policy = Permissions.EmployeesView)]
    public async Task<ActionResult<PagedResult<EmployeeDto>>> GetPaged(
        [FromQuery] QueryParameters query, [FromQuery] int? branchId)
        => Ok(await _employees.GetPagedAsync(query, branchId, Ct));

    [HttpGet("{id:int}")]
    [Authorize(Policy = Permissions.EmployeesView)]
    public async Task<ActionResult<EmployeeDto>> GetById(int id) => Ok(await _employees.GetByIdAsync(id, Ct));

    [HttpGet("{id:int}/activity")]
    [Authorize(Policy = Permissions.EmployeesView)]
    public async Task<ActionResult<PagedResult<AuditLogDto>>> GetActivity(int id, [FromQuery] QueryParameters query)
    {
        var employee = await _employees.GetByIdAsync(id, Ct);
        if (!employee.UserId.HasValue)
        {
            return Ok(PagedResult<AuditLogDto>.Empty(query.Page, query.PageSize));
        }

        return Ok(await _employees.GetActivityAsync(employee.UserId.Value, query, Ct));
    }

    [HttpPost]
    [Authorize(Policy = Permissions.EmployeesManage)]
    public async Task<ActionResult<EmployeeDto>> Create([FromBody] CreateEmployeeRequest request)
        => Ok(await _employees.CreateAsync(request, Ct));

    [HttpPut("{id:int}")]
    [Authorize(Policy = Permissions.EmployeesManage)]
    public async Task<ActionResult<EmployeeDto>> Update(int id, [FromBody] UpdateEmployeeRequest request)
        => Ok(await _employees.UpdateAsync(id, request, Ct));

    [HttpDelete("{id:int}")]
    [Authorize(Policy = Permissions.EmployeesManage)]
    public async Task<IActionResult> Delete(int id)
    {
        await _employees.DeleteAsync(id, Ct);
        return NoContent();
    }
}

[Route("api/catalog")]
public sealed class CatalogController : ApiControllerBase
{
    private readonly ICatalogService _catalog;

    public CatalogController(ICatalogService catalog) => _catalog = catalog;

    [HttpGet("categories")]
    [Authorize(Policy = Permissions.MedicinesView)]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> GetCategories([FromQuery] bool includeInactive = false)
        => Ok(await _catalog.GetCategoriesAsync(includeInactive, Ct));

    [HttpPost("categories")]
    [Authorize(Policy = Permissions.MedicinesManage)]
    public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryRequest request)
        => Ok(await _catalog.CreateCategoryAsync(request, Ct));

    [HttpPut("categories/{id:int}")]
    [Authorize(Policy = Permissions.MedicinesManage)]
    public async Task<ActionResult<CategoryDto>> UpdateCategory(int id, [FromBody] UpdateCategoryRequest request)
        => Ok(await _catalog.UpdateCategoryAsync(id, request, Ct));

    [HttpDelete("categories/{id:int}")]
    [Authorize(Policy = Permissions.MedicinesManage)]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        await _catalog.DeleteCategoryAsync(id, Ct);
        return NoContent();
    }

    [HttpGet("manufacturers")]
    [Authorize(Policy = Permissions.MedicinesView)]
    public async Task<ActionResult<IReadOnlyList<ManufacturerDto>>> GetManufacturers([FromQuery] bool includeInactive = false)
        => Ok(await _catalog.GetManufacturersAsync(includeInactive, Ct));

    [HttpPost("manufacturers")]
    [Authorize(Policy = Permissions.MedicinesManage)]
    public async Task<ActionResult<ManufacturerDto>> CreateManufacturer([FromBody] CreateManufacturerRequest request)
        => Ok(await _catalog.CreateManufacturerAsync(request, Ct));

    [HttpPut("manufacturers/{id:int}")]
    [Authorize(Policy = Permissions.MedicinesManage)]
    public async Task<ActionResult<ManufacturerDto>> UpdateManufacturer(int id, [FromBody] UpdateManufacturerRequest request)
        => Ok(await _catalog.UpdateManufacturerAsync(id, request, Ct));

    [HttpDelete("manufacturers/{id:int}")]
    [Authorize(Policy = Permissions.MedicinesManage)]
    public async Task<IActionResult> DeleteManufacturer(int id)
    {
        await _catalog.DeleteManufacturerAsync(id, Ct);
        return NoContent();
    }

    [HttpGet("units")]
    [Authorize(Policy = Permissions.MedicinesView)]
    public async Task<ActionResult<IReadOnlyList<UnitDto>>> GetUnits([FromQuery] bool includeInactive = false)
        => Ok(await _catalog.GetUnitsAsync(includeInactive, Ct));

    [HttpPost("units")]
    [Authorize(Policy = Permissions.MedicinesManage)]
    public async Task<ActionResult<UnitDto>> CreateUnit([FromBody] CreateUnitRequest request)
        => Ok(await _catalog.CreateUnitAsync(request, Ct));

    [HttpPut("units/{id:int}")]
    [Authorize(Policy = Permissions.MedicinesManage)]
    public async Task<ActionResult<UnitDto>> UpdateUnit(int id, [FromBody] UpdateUnitRequest request)
        => Ok(await _catalog.UpdateUnitAsync(id, request, Ct));

    [HttpDelete("units/{id:int}")]
    [Authorize(Policy = Permissions.MedicinesManage)]
    public async Task<IActionResult> DeleteUnit(int id)
    {
        await _catalog.DeleteUnitAsync(id, Ct);
        return NoContent();
    }
}
