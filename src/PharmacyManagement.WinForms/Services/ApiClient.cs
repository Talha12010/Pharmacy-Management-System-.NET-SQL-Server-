using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Auth;
using PharmacyManagement.WinForms.Configuration;

namespace PharmacyManagement.WinForms.Services;

public sealed class ApiClient
{
    private readonly HttpClient _http;
    private readonly AppConfig _config;
    private string? _token;

    public ApiClient(AppConfig config)
    {
        _config = config;
        _http = new HttpClient { BaseAddress = new Uri(config.ApiBaseUrl.TrimEnd('/') + "/") };
        _token = TokenStore.Load();
        ApplyToken();
    }

    public LoginResponse? Session { get; private set; }

    public UserProfileDto? CurrentUser => Session?.User;

    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(_token);

    public string BaseUrl => _config.ApiBaseUrl;

    public async Task LoginAsync(string email, string password, bool remember)
    {
        var response = await PostAsync<LoginResponse>("auth/login", new LoginRequest { Email = email, Password = password }, authenticated: false);
        Session = response;
        _token = response.AccessToken;
        ApplyToken();

        if (remember && _config.RememberSession)
        {
            TokenStore.Save(_token);
        }
        else
        {
            TokenStore.Clear();
        }
    }

    public async Task<bool> TryRestoreSessionAsync()
    {
        if (string.IsNullOrWhiteSpace(_token))
        {
            return false;
        }

        try
        {
            var user = await GetAsync<UserProfileDto>("auth/me");
            Session = new LoginResponse
            {
                AccessToken = _token!,
                RefreshToken = string.Empty,
                AccessTokenExpiresAtUtc = DateTime.UtcNow.AddMinutes(15),
                RefreshTokenExpiresAtUtc = DateTime.UtcNow.AddDays(7),
                User = user
            };
            return true;
        }
        catch
        {
            Logout();
            return false;
        }
    }

    public void Logout()
    {
        Session = null;
        _token = null;
        ApplyToken();
        TokenStore.Clear();
    }

    public async Task<T> GetAsync<T>(string path, CancellationToken cancellationToken = default)
        => await SendAsync<T>(HttpMethod.Get, path, null, cancellationToken);

    public async Task<PagedResult<T>> GetPagedAsync<T>(
        string path,
        QueryParameters query,
        IEnumerable<KeyValuePair<string, string?>>? extra = null,
        CancellationToken cancellationToken = default)
    {
        var parameters = new List<KeyValuePair<string, string?>>
        {
            new("page", query.Page.ToString()),
            new("pageSize", query.PageSize.ToString()),
            new("search", query.Search),
            new("sortBy", query.SortBy),
            new("sortDesc", query.SortDesc ? "true" : null)
        };

        if (extra is not null)
        {
            parameters.AddRange(extra);
        }

        var fullPath = path + BuildQuery(parameters);
        using var request = new HttpRequestMessage(HttpMethod.Get, fullPath);
        ApplyAuth(request);
        using var response = await _http.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response);

        if (response.StatusCode == System.Net.HttpStatusCode.NoContent || response.Content.Headers.ContentLength == 0)
        {
            return PagedResult<T>.Empty(query.Page, query.PageSize);
        }

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(json))
        {
            return PagedResult<T>.Empty(query.Page, query.PageSize);
        }

        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.ValueKind == JsonValueKind.Array)
        {
            var list = JsonSerializer.Deserialize<List<T>>(json, JsonOptions) ?? new List<T>();
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var s = query.Search.Trim();
                list = list.Where(item =>
                    item?.ToString()?.Contains(s, StringComparison.OrdinalIgnoreCase) == true ||
                    typeof(T).GetProperties().Any(p => p.GetValue(item)?.ToString()?.Contains(s, StringComparison.OrdinalIgnoreCase) == true)).ToList();
            }
            return PagedResult<T>.Create(list, query.Page, query.PageSize, list.Count);
        }

        return JsonSerializer.Deserialize<PagedResult<T>>(json, JsonOptions) ?? PagedResult<T>.Empty(query.Page, query.PageSize);
    }

    public async Task<T> PostAsync<T>(string path, object? body, bool authenticated = true, CancellationToken cancellationToken = default)
        => await SendAsync<T>(HttpMethod.Post, path, body, cancellationToken, authenticated);

    public async Task<T> PutAsync<T>(string path, object? body, CancellationToken cancellationToken = default)
        => await SendAsync<T>(HttpMethod.Put, path, body, cancellationToken);

    public async Task DeleteAsync(string path, CancellationToken cancellationToken = default)
        => await SendAsync<object>(HttpMethod.Delete, path, null, cancellationToken);

    public async Task<byte[]> GetBytesAsync(string path, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        ApplyAuth(request);
        using var response = await _http.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }

    private async Task<T> SendAsync<T>(HttpMethod method, string path, object? body, CancellationToken cancellationToken, bool authenticated = true)
    {
        using var request = new HttpRequestMessage(method, path);
        if (authenticated)
        {
            ApplyAuth(request);
        }

        if (body is not null)
        {
            request.Content = new StringContent(JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json");
        }

        using var response = await _http.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response);

        if (response.StatusCode == System.Net.HttpStatusCode.NoContent || response.Content.Headers.ContentLength == 0)
        {
            return default!;
        }

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<T>(json, JsonOptions) ?? default!;
    }

    private async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var content = await response.Content.ReadAsStringAsync();
        var detail = ExtractError(content);
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            Logout();
        }

        throw new ApiException(detail ?? $"API request failed with {(int)response.StatusCode} {response.ReasonPhrase}");
    }

    private static string? ExtractError(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(content);
            var root = document.RootElement;
            if (root.TryGetProperty("detail", out var detail))
            {
                return detail.GetString();
            }

            if (root.TryGetProperty("title", out var title))
            {
                return title.GetString();
            }

            if (root.TryGetProperty("errors", out var errors))
            {
                var messages = new List<string>();
                foreach (var property in errors.EnumerateObject())
                {
                    foreach (var message in property.Value.EnumerateArray())
                    {
                        messages.Add($"{property.Name}: {message.GetString()}");
                    }
                }

                if (messages.Count > 0)
                {
                    return string.Join(Environment.NewLine, messages);
                }
            }
        }
        catch
        {
            return content.Length > 500 ? content[..500] : content;
        }

        return null;
    }

    private void ApplyAuth(HttpRequestMessage request)
    {
        if (!string.IsNullOrWhiteSpace(_token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        }
    }

    private void ApplyToken()
    {
        _http.DefaultRequestHeaders.Authorization = string.IsNullOrWhiteSpace(_token)
            ? null
            : new AuthenticationHeaderValue("Bearer", _token);
    }

    private static string BuildQuery(IEnumerable<KeyValuePair<string, string?>> parameters)
    {
        var parts = parameters
            .Where(p => !string.IsNullOrWhiteSpace(p.Value))
            .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value!)}")
            .ToList();
        return parts.Count == 0 ? string.Empty : "?" + string.Join("&", parts);
    }

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };
}

public sealed class ApiException : Exception
{
    public ApiException(string message) : base(message)
    {
    }
}
