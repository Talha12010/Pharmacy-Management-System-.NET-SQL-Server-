using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Users;

namespace PharmacyManagement.Application.Interfaces;

public interface IUserService
{
    Task<PagedResult<UserDto>> GetPagedAsync(QueryParameters query, string? role, bool? isActive, CancellationToken cancellationToken = default);
    Task<UserDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<UserDto> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task ResetPasswordAsync(Guid id, ResetPasswordRequest request, CancellationToken cancellationToken = default);
    Task ToggleActiveAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RoleDto>> GetRolesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PermissionDto>> GetPermissionsAsync(CancellationToken cancellationToken = default);
}
