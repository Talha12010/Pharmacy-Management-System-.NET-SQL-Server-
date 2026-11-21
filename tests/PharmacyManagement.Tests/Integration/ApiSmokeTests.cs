using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace PharmacyManagement.Tests.Integration;

public sealed class ApiSmokeTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ApiSmokeTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_Unlocks_Protected_Dashboard_Lookup_And_Reports()
    {
        var client = _factory.CreateClient();

        var health = await client.GetAsync("/health");
        health.EnsureSuccessStatusCode();

        var login = await client.PostAsJsonAsync(
            "/api/auth/login",
            new { email = "admin@pharmacy.local", password = "Admin@12345" });
        login.EnsureSuccessStatusCode();

        using var loginDocument = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
        var token = loginDocument.RootElement.GetProperty("accessToken").GetString();
        Assert.False(string.IsNullOrWhiteSpace(token));

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var dashboard = await client.GetAsync("/api/dashboard");
        dashboard.EnsureSuccessStatusCode();
        using var dashboardDocument = JsonDocument.Parse(await dashboard.Content.ReadAsStringAsync());
        Assert.True(dashboardDocument.RootElement.GetProperty("totalProducts").GetInt32() >= 1);

        var lookup = await client.GetAsync("/api/medicines/lookup?q=&branchId=1&limit=5");
        lookup.EnsureSuccessStatusCode();
        using var lookupDocument = JsonDocument.Parse(await lookup.Content.ReadAsStringAsync());
        Assert.NotEmpty(lookupDocument.RootElement.EnumerateArray());

        var report = await client.GetAsync("/api/reports/sales");
        report.EnsureSuccessStatusCode();
        using var reportDocument = JsonDocument.Parse(await report.Content.ReadAsStringAsync());
        Assert.Equal("Sales Report", reportDocument.RootElement.GetProperty("title").GetString());
    }
}
