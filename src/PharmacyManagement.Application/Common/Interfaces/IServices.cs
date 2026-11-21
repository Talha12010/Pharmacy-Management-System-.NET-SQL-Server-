using PharmacyManagement.Domain.Enums;

namespace PharmacyManagement.Application.Common.Interfaces;

public interface IDocumentNumberService
{
    Task<string> NextAsync(DocumentType type, DateTime utcNow, CancellationToken cancellationToken = default);
}

public interface IAuditService
{
    Task LogAsync(
        string action,
        string entityName,
        string? entityId = null,
        object? oldValues = null,
        object? newValues = null,
        CancellationToken cancellationToken = default);
}

public interface IFileStorageService
{
    Task<string> SaveAsync(Stream content, string originalFileName, string folder, CancellationToken cancellationToken = default);
    Task<Stream> OpenReadAsync(string relativePath, CancellationToken cancellationToken = default);
    Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default);
}

public interface IPermissionService
{
    Task<IReadOnlyList<string>> GetPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);
}

public interface ITokenService
{
    (string AccessToken, DateTime ExpiresAtUtc) CreateAccessToken(
        Guid userId, string email, string fullName, int? branchId, IEnumerable<string> roles, IEnumerable<string> permissions);
    string CreateRefreshToken();
    string HashRefreshToken(string token);
}

public interface IReportExportService
{
    byte[] ExportExcel(ReportTableDto report);
    byte[] ExportPdf(ReportTableDto report);
    string GetContentType(ReportExportFormat format);
    string GetFileExtension(ReportExportFormat format);
}

public enum ReportExportFormat
{
    Excel = 1,
    Pdf = 2
}

public sealed class ReportTableDto
{
    public string Title { get; init; } = string.Empty;
    public string Subtitle { get; init; } = string.Empty;
    public DateTime GeneratedAtUtc { get; init; }
    public IReadOnlyList<string> Columns { get; init; } = Array.Empty<string>();
    public IReadOnlyList<IReadOnlyList<object?>> Rows { get; init; } = Array.Empty<IReadOnlyList<object?>>();
    public IReadOnlyList<ReportSummaryItem> Summary { get; init; } = Array.Empty<ReportSummaryItem>();
}

public sealed record ReportSummaryItem(string Label, string Value);
