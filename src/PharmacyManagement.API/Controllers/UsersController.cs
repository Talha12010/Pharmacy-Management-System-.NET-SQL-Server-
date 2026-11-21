using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Users;
using PharmacyManagement.Application.Interfaces;

namespace PharmacyManagement.API.Controllers;

[Route("api/users")]
public sealed class UsersController : ApiControllerBase
{
    private readonly IUserService _users;

    public UsersController(IUserService users) => _users = users;

    [HttpGet]
    [Authorize(Policy = Permissions.UsersView)]
    public async Task<ActionResult<PagedResult<UserDto>>> GetPaged(
        [FromQuery] QueryParameters query, [FromQuery] string? role, [FromQuery] bool? isActive)
        => Ok(await _users.GetPagedAsync(query, role, isActive, Ct));

    [HttpGet("roles")]
    [Authorize(Policy = Permissions.UsersView)]
    public async Task<ActionResult<IReadOnlyList<RoleDto>>> GetRoles()
        => Ok(await _users.GetRolesAsync(Ct));

    [HttpGet("permissions")]
    [Authorize(Policy = Permissions.UsersView)]
    public async Task<ActionResult<IReadOnlyList<PermissionDto>>> GetPermissions()
        => Ok(await _users.GetPermissionsAsync(Ct));

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.UsersView)]
    public async Task<ActionResult<UserDto>> GetById(Guid id)
        => Ok(await _users.GetByIdAsync(id, Ct));

    [HttpPost]
    [Authorize(Policy = Permissions.UsersManage)]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserRequest request)
        => Ok(await _users.CreateAsync(request, Ct));

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.UsersManage)]
    public async Task<ActionResult<UserDto>> Update(Guid id, [FromBody] UpdateUserRequest request)
        => Ok(await _users.UpdateAsync(id, request, Ct));

    [HttpPost("{id:guid}/reset-password")]
    [Authorize(Policy = Permissions.UsersManage)]
    public async Task<IActionResult> ResetPassword(Guid id, [FromBody] ResetPasswordRequest request)
    {
        await _users.ResetPasswordAsync(id, request, Ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/toggle-active")]
    [Authorize(Policy = Permissions.UsersManage)]
    public async Task<IActionResult> ToggleActive(Guid id)
    {
        await _users.ToggleActiveAsync(id, Ct);
        return NoContent();
    }
}
