using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common.Interfaces;
using PharmacyManagement.Domain.Entities;

namespace PharmacyManagement.Infrastructure.Services;

public sealed class PermissionService : IPermissionService
{
    private readonly IApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public PermissionService(IApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IReadOnlyList<string>> GetPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Array.Empty<string>();
        }

        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Count == 0)
        {
            return Array.Empty<string>();
        }

        return await _db.RolePermissions
            .AsNoTracking()
            .Where(rp => roles.Contains(rp.Role.Name!) && rp.Permission.Code != null)
            .Select(rp => rp.Permission.Code)
            .Distinct()
            .ToListAsync(cancellationToken);
    }
}
