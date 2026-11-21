using System.Text.Json;

namespace PharmacyManagement.WinForms.Services;

public static class TokenStore
{
    private static readonly string DirectoryPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "PharmacyManagement");

    private static readonly string FilePath = Path.Combine(DirectoryPath, "session.json");

    public static string? Load()
    {
        if (!File.Exists(FilePath))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(FilePath));
            return document.RootElement.GetProperty("accessToken").GetString();
        }
        catch
        {
            return null;
        }
    }

    public static void Save(string token)
    {
        Directory.CreateDirectory(DirectoryPath);
        var json = JsonSerializer.Serialize(new { accessToken = token }, AppConfigJson.Options);
        File.WriteAllText(FilePath, json);
    }

    public static void Clear()
    {
        if (File.Exists(FilePath))
        {
            File.Delete(FilePath);
        }
    }
}

internal static class AppConfigJson
{
    internal static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
}
