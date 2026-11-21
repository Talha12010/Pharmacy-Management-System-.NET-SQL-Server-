using System.Text.Json;
using System.Text.Json.Serialization;
using PharmacyManagement.Application.DTOs.Auth;

namespace PharmacyManagement.Tests.Common;

public sealed class LoginResponseDeserializationTests
{
    [Fact]
    public void User_Permissions_Are_Deserialized()
    {
        const string json = """
        {
          "accessToken": "token",
          "refreshToken": "refresh",
          "accessTokenExpiresAtUtc": "2026-11-21T12:00:00Z",
          "refreshTokenExpiresAtUtc": "2026-11-28T12:00:00Z",
          "user": {
            "id": "762e49ce-eb8c-4f9f-cbf3-08df47bf7280",
            "email": "admin@pharmacy.local",
            "fullName": "System Administrator",
            "branchId": 1,
            "branchName": "Main Branch",
            "roles": ["SuperAdmin"],
            "permissions": ["Dashboard.View", "Sales.Create"]
          }
        }
        """;

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        var response = JsonSerializer.Deserialize<LoginResponse>(json, options);

        Assert.NotNull(response);
        Assert.Equal(2, response.User.Permissions.Count);
        Assert.Contains("Dashboard.View", response.User.Permissions);
    }
}
