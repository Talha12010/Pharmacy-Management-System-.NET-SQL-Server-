using System.Text.Json;

namespace PharmacyManagement.WinForms.Configuration;

public sealed class AppConfig
{
    public string ApiBaseUrl { get; set; } = "http://localhost:5098/api";
    public bool RememberSession { get; set; } = true;

    public static AppConfig Load()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (!File.Exists(path))
        {
            return new AppConfig();
        }

        try
        {
            return JsonSerializer.Deserialize<AppConfig>(File.ReadAllText(path), JsonOptions) ?? new AppConfig();
        }
        catch
        {
            return new AppConfig();
        }
    }

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };
}
