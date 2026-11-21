using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.Common.Interfaces;

namespace PharmacyManagement.Infrastructure.Services;

public sealed class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (string AccessToken, DateTime ExpiresAtUtc) CreateAccessToken(
        Guid userId,
        string email,
        string fullName,
        int? branchId,
        IEnumerable<string> roles,
        IEnumerable<string> permissions)
    {
        var secret = _configuration["Jwt:Secret"] ?? "ChangeThisDevelopmentSecretKeyToSomethingLongAndSecure123!";
        var issuer = _configuration["Jwt:Issuer"] ?? "PharmacyManagement.Api";
        var audience = _configuration["Jwt:Audience"] ?? "PharmacyManagement.Clients";
        var minutes = int.TryParse(_configuration["Jwt:AccessTokenMinutes"], out var parsed) ? parsed : 15;

        var expiresAtUtc = DateTime.UtcNow.AddMinutes(minutes);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(ClaimTypes.Name, fullName),
            new(ClaimTypes.Email, email),
            new(JwtRegisteredClaimNames.Email, email),
            new(CustomClaimTypes.FullName, fullName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (branchId.HasValue)
        {
            claims.Add(new Claim(CustomClaimTypes.BranchId, branchId.Value.ToString()));
        }

        foreach (var role in roles.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
            claims.Add(new Claim("role", role));
        }

        foreach (var permission in permissions.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            claims.Add(new Claim(CustomClaimTypes.Permission, permission));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow.AddMinutes(-1),
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc);
    }

    public string CreateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(48);
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    public string HashRefreshToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }
}
