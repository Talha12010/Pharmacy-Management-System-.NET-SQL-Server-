using System.Text.Json;
using PharmacyManagement.Application.Common.Interfaces;
using PharmacyManagement.Domain.Entities;

namespace PharmacyManagement.Infrastructure.Services;

public sealed class AuditService : IAuditService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public AuditService(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task LogAsync(
        string action,
        string entityName,
        string? entityId = null,
        object? oldValues = null,
        object? newValues = null,
        CancellationToken cancellationToken = default)
    {
        _db.AuditLogs.Add(new AuditLog
        {
            UserId = _currentUser.UserId,
            UserName = _currentUser.UserName,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            OldValues = Serialize(oldValues),
            NewValues = Serialize(newValues),
            IpAddress = _currentUser.IpAddress,
            UserAgent = _currentUser.UserAgent,
            CreatedAtUtc = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(cancellationToken);
    }

    private static string? Serialize(object? value)
        => value is null ? null : JsonSerializer.Serialize(value, JsonOptions);
}
