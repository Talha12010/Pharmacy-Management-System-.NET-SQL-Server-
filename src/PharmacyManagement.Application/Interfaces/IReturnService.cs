using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Returns;
using PharmacyManagement.Domain.Enums;

namespace PharmacyManagement.Application.Interfaces;

public interface IReturnService
{
    Task<PagedResult<ReturnDto>> GetPagedAsync(
        QueryParameters query, ReturnStatus? status, ReturnType? type, int? branchId,
        CancellationToken cancellationToken = default);

    Task<ReturnDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ReturnDto> CreateAsync(CreateReturnRequest request, CancellationToken cancellationToken = default);
    Task<ReturnDto> ApproveAsync(int id, ApproveReturnRequest request, CancellationToken cancellationToken = default);
    Task<ReturnDto> RejectAsync(int id, string reason, CancellationToken cancellationToken = default);
}
