namespace PharmacyManagement.Application.DTOs.Settings;

public sealed class SystemSettingDto
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Group { get; set; } = string.Empty;
    public bool IsEditable { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class UpdateSystemSettingRequest
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
