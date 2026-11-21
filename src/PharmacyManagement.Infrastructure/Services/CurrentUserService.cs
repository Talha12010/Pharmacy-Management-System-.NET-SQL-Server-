using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.Common.Interfaces;

namespace PharmacyManagement.Infrastructure.Services;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var value = FindFirst(ClaimTypes.NameIdentifier, "sub");
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public string? UserName => FindFirst(ClaimTypes.Name, "preferred_username", ClaimTypes.Email, "email");

    public int? BranchId
    {
        get
        {
            var value = FindFirst(CustomClaimTypes.BranchId);
            return int.TryParse(value, out var id) ? id : null;
        }
    }

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

    public string? IpAddress
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;
            var forwarded = context?.Request.Headers["X-Forwarded-For"].ToString();
            if (!string.IsNullOrWhiteSpace(forwarded))
            {
                return forwarded.Split(',')[0].Trim();
            }

            return context?.Connection.RemoteIpAddress?.ToString();
        }
    }

    public string? UserAgent => _httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString();

    public bool HasPermission(string permission)
        => User?.Claims.Any(c => c.Type == CustomClaimTypes.Permission &&
                                 string.Equals(c.Value, permission, StringComparison.OrdinalIgnoreCase)) == true;

    private string? FindFirst(params string[] claimTypes)
    {
        if (User is null)
        {
            return null;
        }

        foreach (var claimType in claimTypes)
        {
            var value = User.FindFirst(claimType)?.Value;
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }
}
