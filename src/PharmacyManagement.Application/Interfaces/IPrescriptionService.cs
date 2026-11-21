using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Prescriptions;
using PharmacyManagement.Domain.Enums;

namespace PharmacyManagement.Application.Interfaces;

public interface IPrescriptionService
{
    Task<PagedResult<PrescriptionDto>> GetPagedAsync(
        QueryParameters query, PrescriptionStatus? status, int? branchId, DateTime? from, DateTime? to,
        CancellationToken cancellationToken = default);

    Task<PrescriptionDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PrescriptionDto> CreateAsync(CreatePrescriptionRequest request, CancellationToken cancellationToken = default);
    Task<PrescriptionDto> UpdateAsync(int id, CreatePrescriptionRequest request, CancellationToken cancellationToken = default);
    Task<PrescriptionDto> VerifyAsync(int id, CancellationToken cancellationToken = default);
    Task<PrescriptionDto> RejectAsync(int id, string reason, CancellationToken cancellationToken = default);
    Task<string> UploadImageAsync(int id, Stream content, string fileName, CancellationToken cancellationToken = default);
}
