using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PharmacyManagement.Application.Common.Interfaces;
using PharmacyManagement.Application.DTOs.Auth;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Entities;
using PharmacyManagement.Domain.Exceptions;

namespace PharmacyManagement.Infrastructure.Services;

public sealed class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IApplicationDbContext _db;
    private readonly ITokenService _tokenService;
    private readonly IPermissionService _permissionService;
    private readonly IConfiguration _configuration;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IApplicationDbContext db,
        ITokenService tokenService,
        IPermissionService permissionService,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _db = db;
        _tokenService = tokenService;
        _permissionService = permissionService;
        _configuration = configuration;
    }

    public async Task<LoginResponse> LoginAsync(
        LoginRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedException();
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (result.IsLockedOut)
        {
            throw new UnauthorizedException("The account is locked. Please try again later.");
        }

        if (!result.Succeeded)
        {
            throw new UnauthorizedException();
        }

        user.LastLoginAtUtc = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        return await IssueTokensAsync(user, ipAddress, cancellationToken);
    }

    public async Task<LoginResponse> RefreshAsync(
        string refreshToken,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new UnauthorizedException("Refresh token is required.");
        }

        var tokenHash = _tokenService.HashRefreshToken(refreshToken);
        var stored = await _db.RefreshTokens
            .Include(t => t.User)
            .SingleOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

        if (stored is null || !stored.IsActive || stored.User is null || !stored.User.IsActive)
        {
            throw new UnauthorizedException("The refresh token is invalid or expired.");
        }

        var now = DateTime.UtcNow;
        var newRefreshToken = _tokenService.CreateRefreshToken();
        stored.RevokedAtUtc = now;
        stored.RevokedByIp = ipAddress;
        stored.ReplacedByTokenHash = _tokenService.HashRefreshToken(newRefreshToken);

        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = stored.UserId,
            TokenHash = stored.ReplacedByTokenHash,
            CreatedAtUtc = now,
            ExpiresAtUtc = now.AddDays(RefreshTokenDays),
            CreatedByIp = ipAddress
        });

        await _db.SaveChangesAsync(cancellationToken);
        return await BuildResponseAsync(stored.User, newRefreshToken, now, cancellationToken);
    }

    public async Task LogoutAsync(
        string refreshToken,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return;
        }

        var tokenHash = _tokenService.HashRefreshToken(refreshToken);
        var stored = await _db.RefreshTokens.SingleOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);
        if (stored is null || stored.RevokedAtUtc is not null)
        {
            return;
        }

        stored.RevokedAtUtc = DateTime.UtcNow;
        stored.RevokedByIp = ipAddress;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task ChangePasswordAsync(
        Guid userId,
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            throw new NotFoundException("User", userId);
        }

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            throw new ValidationFailedException("Password", ToErrorDictionary(result));
        }
    }

    public Task<UserProfileDto> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default)
        => BuildProfileAsync(userId, cancellationToken);

    private async Task<LoginResponse> IssueTokensAsync(
        ApplicationUser user,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var refreshToken = _tokenService.CreateRefreshToken();
        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = _tokenService.HashRefreshToken(refreshToken),
            CreatedAtUtc = now,
            ExpiresAtUtc = now.AddDays(RefreshTokenDays),
            CreatedByIp = ipAddress
        });
        await _db.SaveChangesAsync(cancellationToken);

        return await BuildResponseAsync(user, refreshToken, now, cancellationToken);
    }

    private async Task<LoginResponse> BuildResponseAsync(
        ApplicationUser user,
        string refreshToken,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var permissions = await _permissionService.GetPermissionsAsync(user.Id, cancellationToken);
        var (accessToken, expiresAtUtc) = _tokenService.CreateAccessToken(
            user.Id,
            user.Email ?? string.Empty,
            user.FullName,
            user.BranchId,
            roles,
            permissions);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiresAtUtc = expiresAtUtc,
            RefreshTokenExpiresAtUtc = now.AddDays(RefreshTokenDays),
            User = new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                BranchId = user.BranchId,
                BranchName = await GetBranchNameAsync(user.BranchId, cancellationToken),
                Roles = roles.ToList(),
                Permissions = permissions
            }
        };
    }

    private async Task<UserProfileDto> BuildProfileAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            throw new NotFoundException("User", userId);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var permissions = await _permissionService.GetPermissionsAsync(user.Id, cancellationToken);

        return new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            BranchId = user.BranchId,
            BranchName = await GetBranchNameAsync(user.BranchId, cancellationToken),
            Roles = roles.ToList(),
            Permissions = permissions
        };
    }

    private async Task<string?> GetBranchNameAsync(int? branchId, CancellationToken cancellationToken)
    {
        if (!branchId.HasValue)
        {
            return null;
        }

        return await _db.Branches.AsNoTracking()
            .Where(b => b.Id == branchId.Value)
            .Select(b => b.Name)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private int RefreshTokenDays
        => int.TryParse(_configuration["Jwt:RefreshTokenDays"], out var days) ? days : 7;

    private static IReadOnlyDictionary<string, string[]> ToErrorDictionary(IdentityResult result)
        => result.Errors
            .GroupBy(e => e.Code)
            .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());
}
