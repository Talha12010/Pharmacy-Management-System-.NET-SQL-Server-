using PharmacyManagement.Application.DTOs.Auth;

namespace PharmacyManagement.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, string? ipAddress, CancellationToken cancellationToken = default);
    Task<LoginResponse> RefreshAsync(string refreshToken, string? ipAddress, CancellationToken cancellationToken = default);
    Task LogoutAsync(string refreshToken, string? ipAddress, CancellationToken cancellationToken = default);
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
    Task<UserProfileDto> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default);
}
