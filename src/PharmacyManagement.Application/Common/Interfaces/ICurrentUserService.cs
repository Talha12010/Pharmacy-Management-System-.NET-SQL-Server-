namespace PharmacyManagement.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? UserName { get; }
    int? BranchId { get; }
    bool IsAuthenticated { get; }
    string? IpAddress { get; }
    string? UserAgent { get; }
    bool HasPermission(string permission);
}

public sealed class CurrentUser
{
    public Guid? UserId { get; init; }
    public string? UserName { get; init; }
    public int? BranchId { get; init; }
    public bool IsAuthenticated { get; init; }
    public IReadOnlyCollection<string> Permissions { get; init; } = Array.Empty<string>();
}
