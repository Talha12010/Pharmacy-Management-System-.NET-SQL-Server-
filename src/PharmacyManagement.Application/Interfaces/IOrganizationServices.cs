using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Audit;
using PharmacyManagement.Application.DTOs.Catalog;
using PharmacyManagement.Application.DTOs.Organization;

namespace PharmacyManagement.Application.Interfaces;

public interface IBranchService
{
    Task<IReadOnlyList<BranchDto>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LookupDto>> GetLookupAsync(CancellationToken cancellationToken = default);
    Task<BranchDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<BranchDto> CreateAsync(CreateBranchRequest request, CancellationToken cancellationToken = default);
    Task<BranchDto> UpdateAsync(int id, UpdateBranchRequest request, CancellationToken cancellationToken = default);
    Task DeactivateAsync(int id, CancellationToken cancellationToken = default);
}

public interface IEmployeeService
{
    Task<PagedResult<EmployeeDto>> GetPagedAsync(QueryParameters query, int? branchId, CancellationToken cancellationToken = default);
    Task<EmployeeDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<EmployeeDto> CreateAsync(CreateEmployeeRequest request, CancellationToken cancellationToken = default);
    Task<EmployeeDto> UpdateAsync(int id, UpdateEmployeeRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<PagedResult<AuditLogDto>> GetActivityAsync(Guid userId, QueryParameters query, CancellationToken cancellationToken = default);
}

public interface ICatalogService
{
    Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default);
    Task<CategoryDto> UpdateCategoryAsync(int id, UpdateCategoryRequest request, CancellationToken cancellationToken = default);
    Task DeleteCategoryAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ManufacturerDto>> GetManufacturersAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<ManufacturerDto> CreateManufacturerAsync(CreateManufacturerRequest request, CancellationToken cancellationToken = default);
    Task<ManufacturerDto> UpdateManufacturerAsync(int id, UpdateManufacturerRequest request, CancellationToken cancellationToken = default);
    Task DeleteManufacturerAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UnitDto>> GetUnitsAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<UnitDto> CreateUnitAsync(CreateUnitRequest request, CancellationToken cancellationToken = default);
    Task<UnitDto> UpdateUnitAsync(int id, UpdateUnitRequest request, CancellationToken cancellationToken = default);
    Task DeleteUnitAsync(int id, CancellationToken cancellationToken = default);
}
