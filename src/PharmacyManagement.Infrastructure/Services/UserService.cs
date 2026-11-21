using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Users;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Entities;
using PharmacyManagement.Domain.Exceptions;
using PharmacyManagement.Infrastructure.Persistence;

namespace PharmacyManagement.Infrastructure.Services;

public sealed class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ApplicationDbContext _db;

    public UserService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        ApplicationDbContext db)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _db = db;
    }

    public async Task<PagedResult<UserDto>> GetPagedAsync(
        QueryParameters query,
        string? role,
        bool? isActive,
        CancellationToken cancellationToken = default)
    {
        var dbQuery = _db.Users
            .AsNoTracking()
            .Include(u => u.Branch)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            dbQuery = dbQuery.Where(u =>
                u.Email!.ToLower().Contains(search) ||
                u.FirstName.ToLower().Contains(search) ||
                u.LastName.ToLower().Contains(search));
        }

        if (isActive.HasValue)
        {
            dbQuery = dbQuery.Where(u => u.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(role))
        {
            var roleEntity = await _db.Roles.FirstOrDefaultAsync(
                r => r.Name != null && r.Name.ToLower() == role.Trim().ToLower(), cancellationToken);
            if (roleEntity is null)
            {
                return PagedResult<UserDto>.Empty(query.Page, query.PageSize);
            }

            dbQuery = dbQuery.Where(u => _db.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == roleEntity.Id));
        }

        var paged = await dbQuery
            .ApplyOrdering(query.SortBy, query.SortDesc, nameof(ApplicationUser.Email))
            .ToPagedResultAsync(query.Page, query.PageSize, cancellationToken);

        var userIds = paged.Items.Select(u => u.Id).ToList();
        var rolesByUser = await (from userRole in _db.UserRoles
                                 join roleEntity in _db.Roles on userRole.RoleId equals roleEntity.Id
                                 where userIds.Contains(userRole.UserId)
                                 select new { userRole.UserId, RoleName = roleEntity.Name! })
            .ToListAsync(cancellationToken);

        var lookup = rolesByUser.ToLookup(x => x.UserId, x => x.RoleName);
        var items = paged.Items.Select(user => new UserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            BranchId = user.BranchId,
            BranchName = user.Branch?.Name,
            IsActive = user.IsActive,
            CreatedAtUtc = user.CreatedAtUtc,
            LastLoginAtUtc = user.LastLoginAtUtc,
            Roles = lookup[user.Id].ToList()
        }).ToList();

        return PagedResult<UserDto>.Create(items, paged.Page, paged.PageSize, paged.TotalCount);
    }

    public async Task<UserDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _db.Users
            .AsNoTracking()
            .Include(u => u.Branch)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken)
            ?? throw new NotFoundException("User", id);

        var roles = await _userManager.GetRolesAsync(user);
        return MapUser(user, roles);
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var roles = await ValidateRolesAsync(request.Roles, cancellationToken);
        await ValidateBranchAsync(request.BranchId, cancellationToken);

        var user = new ApplicationUser
        {
            UserName = request.Email.Trim(),
            NormalizedUserName = request.Email.Trim().ToUpperInvariant(),
            Email = request.Email.Trim(),
            NormalizedEmail = request.Email.Trim().ToUpperInvariant(),
            EmailConfirmed = true,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            PhoneNumber = request.PhoneNumber,
            BranchId = request.BranchId,
            IsActive = true,
            LockoutEnabled = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            throw new ValidationFailedException("User", ToErrorDictionary(result));
        }

        if (roles.Count > 0)
        {
            var roleResult = await _userManager.AddToRolesAsync(user, roles);
            if (!roleResult.Succeeded)
            {
                throw new ValidationFailedException("Roles", ToErrorDictionary(roleResult));
            }
        }

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            BranchId = user.BranchId,
            IsActive = user.IsActive,
            CreatedAtUtc = user.CreatedAtUtc,
            Roles = roles
        };
    }

    public async Task<UserDto> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id.ToString()) ?? throw new NotFoundException("User", id);
        var roles = await ValidateRolesAsync(request.Roles, cancellationToken);
        await ValidateBranchAsync(request.BranchId, cancellationToken);

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.PhoneNumber = request.PhoneNumber;
        user.BranchId = request.BranchId;
        user.IsActive = request.IsActive;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new ValidationFailedException("User", ToErrorDictionary(result));
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        var rolesToRemove = currentRoles.Except(roles, StringComparer.OrdinalIgnoreCase).ToArray();
        var rolesToAdd = roles.Except(currentRoles, StringComparer.OrdinalIgnoreCase).ToArray();

        if (rolesToRemove.Length > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            if (!removeResult.Succeeded)
            {
                throw new ValidationFailedException("Roles", ToErrorDictionary(removeResult));
            }
        }

        if (rolesToAdd.Length > 0)
        {
            var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
            if (!addResult.Succeeded)
            {
                throw new ValidationFailedException("Roles", ToErrorDictionary(addResult));
            }
        }

        user = await _db.Users.Include(u => u.Branch).FirstAsync(u => u.Id == id, cancellationToken);
        return MapUser(user, await _userManager.GetRolesAsync(user));
    }

    public async Task ResetPasswordAsync(Guid id, ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id.ToString()) ?? throw new NotFoundException("User", id);
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);
        if (!result.Succeeded)
        {
            throw new ValidationFailedException("Password", ToErrorDictionary(result));
        }
    }

    public async Task ToggleActiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id.ToString()) ?? throw new NotFoundException("User", id);
        user.IsActive = !user.IsActive;
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new ValidationFailedException("User", ToErrorDictionary(result));
        }
    }

    public async Task<IReadOnlyList<RoleDto>> GetRolesAsync(CancellationToken cancellationToken = default)
        => await _db.Roles
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name!,
                Description = r.Description,
                IsSystemRole = r.IsSystemRole
            })
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<PermissionDto>> GetPermissionsAsync(CancellationToken cancellationToken = default)
        => await _db.Permissions
            .AsNoTracking()
            .OrderBy(p => p.Module).ThenBy(p => p.Name)
            .Select(p => new PermissionDto
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                Module = p.Module
            })
            .ToListAsync(cancellationToken);

    private async Task<IReadOnlyList<string>> ValidateRolesAsync(IEnumerable<string> roleNames, CancellationToken cancellationToken)
    {
        var requested = roleNames.Where(r => !string.IsNullOrWhiteSpace(r)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var valid = await _db.Roles
            .Where(r => requested.Contains(r.Name!))
            .Select(r => r.Name!)
            .ToListAsync(cancellationToken);

        if (valid.Count != requested.Count)
        {
            throw new ValidationFailedException(
                "Roles",
                new Dictionary<string, string[]> { ["roles"] = new[] { "One or more selected roles do not exist." } });
        }

        return valid;
    }

    private async Task ValidateBranchAsync(int? branchId, CancellationToken cancellationToken)
    {
        if (branchId.HasValue && !await _db.Branches.AnyAsync(b => b.Id == branchId.Value, cancellationToken))
        {
            throw new ValidationFailedException("Branch", "The selected branch does not exist.");
        }
    }

    private static UserDto MapUser(ApplicationUser user, IList<string> roles)
        => new()
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            BranchId = user.BranchId,
            BranchName = user.Branch?.Name,
            IsActive = user.IsActive,
            CreatedAtUtc = user.CreatedAtUtc,
            LastLoginAtUtc = user.LastLoginAtUtc,
            Roles = roles.ToList()
        };

    private static IReadOnlyDictionary<string, string[]> ToErrorDictionary(IdentityResult result)
        => result.Errors
            .GroupBy(e => e.Code)
            .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());
}
