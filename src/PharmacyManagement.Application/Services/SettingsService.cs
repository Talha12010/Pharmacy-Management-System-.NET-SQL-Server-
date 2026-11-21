using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.Common.Interfaces;
using PharmacyManagement.Application.DTOs.Audit;
using PharmacyManagement.Application.DTOs.Settings;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Application.Mappings;
using PharmacyManagement.Domain.Exceptions;

namespace PharmacyManagement.Application.Services;

public sealed class SettingsService : ISettingsService
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _audit;

    public SettingsService(IApplicationDbContext db, ICurrentUserService currentUser, IAuditService audit)
    {
        _db = db;
        _currentUser = currentUser;
        _audit = audit;
    }

    public async Task<IReadOnlyList<SystemSettingDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var settings = await _db.SystemSettings
            .AsNoTracking()
            .OrderBy(s => s.Group)
            .ThenBy(s => s.Key)
            .ToListAsync(cancellationToken);

        return settings.Select(s => s.ToDto()).ToList();
    }

    public async Task<SystemSettingDto> UpdateAsync(UpdateSystemSettingRequest request, CancellationToken cancellationToken = default)
    {
        var setting = await _db.SystemSettings.FirstOrDefaultAsync(s => s.Key == request.Key, cancellationToken)
            ?? throw new NotFoundException("SystemSetting", request.Key);

        if (!setting.IsEditable)
        {
            throw new BusinessRuleException("This setting is read-only.");
        }

        var oldValue = setting.Value;
        setting.Value = request.Value;
        setting.UpdatedAtUtc = DateTime.UtcNow;
        setting.UpdatedByUserId = _currentUser.UserId;

        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("SettingUpdated", nameof(Domain.Entities.SystemSetting), setting.Id.ToString(),
            new { Value = oldValue }, new { Value = setting.Value }, cancellationToken);

        return setting.ToDto();
    }
}

public sealed class AuditQueryService : IAuditQueryService
{
    private readonly IApplicationDbContext _db;

    public AuditQueryService(IApplicationDbContext db) => _db = db;

    public async Task<PagedResult<AuditLogDto>> GetPagedAsync(
        QueryParameters query, Guid? userId, string? entityName, string? action, DateTime? from, DateTime? to,
        CancellationToken cancellationToken = default)
    {
        var logs = _db.AuditLogs.AsNoTracking();

        if (userId.HasValue)
        {
            logs = logs.Where(l => l.UserId == userId);
        }

        if (!string.IsNullOrWhiteSpace(entityName))
        {
            logs = logs.Where(l => l.EntityName == entityName);
        }

        if (!string.IsNullOrWhiteSpace(action))
        {
            logs = logs.Where(l => l.Action.Contains(action));
        }

        if (from.HasValue)
        {
            var fromUtc = from.Value.Date;
            logs = logs.Where(l => l.CreatedAtUtc >= fromUtc);
        }

        if (to.HasValue)
        {
            var toUtc = to.Value.Date.AddDays(1);
            logs = logs.Where(l => l.CreatedAtUtc < toUtc);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            logs = logs.Where(l =>
                (l.UserName != null && l.UserName.Contains(term)) ||
                l.EntityName.Contains(term) ||
                l.Action.Contains(term) ||
                (l.EntityId != null && l.EntityId.Contains(term)));
        }

        var total = await logs.CountAsync(cancellationToken);
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 200);
        var items = await logs
            .OrderByDescending(l => l.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<AuditLogDto>.Create(items.Select(l => l.ToDto()).ToList(), page, pageSize, total);
    }

    public async Task<AuditLogDto> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var log = await _db.AuditLogs.AsNoTracking().FirstOrDefaultAsync(l => l.Id == id, cancellationToken)
            ?? throw new NotFoundException("AuditLog", id);
        return log.ToDto();
    }
}
