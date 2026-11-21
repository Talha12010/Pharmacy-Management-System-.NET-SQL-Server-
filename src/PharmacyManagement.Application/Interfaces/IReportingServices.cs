using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.Common.Interfaces;
using PharmacyManagement.Application.DTOs.Audit;
using PharmacyManagement.Application.DTOs.Dashboard;
using PharmacyManagement.Application.DTOs.Reports;
using PharmacyManagement.Application.DTOs.Settings;

namespace PharmacyManagement.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardDto> GetAsync(int? branchId, CancellationToken cancellationToken = default);
}

public interface IReportService
{
    Task<ReportTableDto> GetAsync(string type, ReportFilter filter, CancellationToken cancellationToken = default);
}

public interface ISettingsService
{
    Task<IReadOnlyList<SystemSettingDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<SystemSettingDto> UpdateAsync(UpdateSystemSettingRequest request, CancellationToken cancellationToken = default);
}

public interface IAuditQueryService
{
    Task<PagedResult<AuditLogDto>> GetPagedAsync(
        QueryParameters query, Guid? userId, string? entityName, string? action, DateTime? from, DateTime? to,
        CancellationToken cancellationToken = default);

    Task<AuditLogDto> GetByIdAsync(long id, CancellationToken cancellationToken = default);
}
