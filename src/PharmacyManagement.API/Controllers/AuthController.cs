using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.Application.Common.Interfaces;
using PharmacyManagement.Application.DTOs.Auth;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Infrastructure.Services;

namespace PharmacyManagement.API.Controllers;

[Route("api/auth")]
public sealed class AuthController : ApiControllerBase
{
    private readonly IAuthService _auth;
    private readonly ICurrentUserService _currentUser;

    public AuthController(IAuthService auth, ICurrentUserService currentUser)
    {
        _auth = auth;
        _currentUser = currentUser;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        => Ok(await _auth.LoginAsync(request, _currentUser.IpAddress, Ct));

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Refresh([FromBody] RefreshTokenRequest request)
        => Ok(await _auth.RefreshAsync(request.RefreshToken, _currentUser.IpAddress, Ct));

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        await _auth.LogoutAsync(request.RefreshToken, _currentUser.IpAddress, Ct);
        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserProfileDto>> Me()
        => Ok(await _auth.GetProfileAsync(CurrentUserId, Ct));

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        await _auth.ChangePasswordAsync(CurrentUserId, request, Ct);
        return NoContent();
    }
}
