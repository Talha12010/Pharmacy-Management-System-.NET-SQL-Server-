using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Medicines;

namespace PharmacyManagement.Application.Interfaces;

public interface IMedicineService
{
    Task<PagedResult<MedicineListItemDto>> GetPagedAsync(
        QueryParameters query, int? categoryId, int? manufacturerId, bool? isActive,
        CancellationToken cancellationToken = default);

    Task<MedicineDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<MedicineDto> GetByBarcodeAsync(string barcode, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MedicineLookupDto>> LookupAsync(string? search, int? branchId, int limit = 20, CancellationToken cancellationToken = default);
    Task<MedicineDto> CreateAsync(CreateMedicineRequest request, CancellationToken cancellationToken = default);
    Task<MedicineDto> UpdateAsync(int id, UpdateMedicineRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
