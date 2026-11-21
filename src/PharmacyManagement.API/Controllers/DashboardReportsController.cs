using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.Common.Interfaces;
using PharmacyManagement.Application.DTOs.Audit;
using PharmacyManagement.Application.DTOs.Dashboard;
using PharmacyManagement.Application.DTOs.Reports;
using PharmacyManagement.Application.DTOs.Settings;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Exceptions;

namespace PharmacyManagement.API.Controllers;

[Route("api/dashboard")]
public sealed class DashboardController : ApiControllerBase
{
    private readonly IDashboardService _dashboard;

    public DashboardController(IDashboardService dashboard) => _dashboard = dashboard;

    [HttpGet]
    [Authorize(Policy = Permissions.DashboardView)]
    public async Task<ActionResult<DashboardDto>> Get([FromQuery] int? branchId)
        => Ok(await _dashboard.GetAsync(branchId, Ct));

    [HttpGet("sales-chart")]
    [Authorize(Policy = Permissions.DashboardView)]
    public async Task<ActionResult<IReadOnlyList<ChartPointDto>>> SalesChart([FromQuery] int? branchId, [FromQuery] int days = 14)
    {
        var dashboard = await _dashboard.GetAsync(branchId, Ct);
        return Ok(dashboard.DailySales.TakeLast(Math.Clamp(days, 1, 90)).ToList());
    }

    [HttpGet("top-selling")]
    [Authorize(Policy = Permissions.DashboardView)]
    public async Task<ActionResult<IReadOnlyList<TopSellingDto>>> TopSelling([FromQuery] int? branchId, [FromQuery] int count = 10)
    {
        var dashboard = await _dashboard.GetAsync(branchId, Ct);
        return Ok(dashboard.TopSelling.Take(Math.Clamp(count, 1, 50)).ToList());
    }
}

[Route("api/reports")]
[Authorize(Policy = Permissions.ReportsView)]
public sealed class ReportsController : ApiControllerBase
{
    private readonly IReportService _reports;
    private readonly IReportExportService _export;

    public ReportsController(IReportService reports, IReportExportService export)
    {
        _reports = reports;
        _export = export;
    }

    [HttpGet("{type}")]
    [Authorize(Policy = Permissions.ReportsView)]
    public async Task<ActionResult<ReportTableDto>> Get(string type, [FromQuery] ReportFilter filter)
        => Ok(await GetReportAsync(type, filter));

    [HttpGet("sales")] public Task<ActionResult<ReportTableDto>> Sales([FromQuery] ReportFilter filter) => Get(ReportTypes.Sales, filter);
    [HttpGet("profit-loss")] public Task<ActionResult<ReportTableDto>> ProfitLoss([FromQuery] ReportFilter filter) => Get(ReportTypes.ProfitLoss, filter);
    [HttpGet("purchases")] public Task<ActionResult<ReportTableDto>> Purchases([FromQuery] ReportFilter filter) => Get(ReportTypes.Purchases, filter);
    [HttpGet("inventory")] public Task<ActionResult<ReportTableDto>> Inventory([FromQuery] ReportFilter filter) => Get(ReportTypes.Inventory, filter);
    [HttpGet("stock-valuation")] public Task<ActionResult<ReportTableDto>> StockValuation([FromQuery] ReportFilter filter) => Get(ReportTypes.StockValuation, filter);
    [HttpGet("expiry")] public Task<ActionResult<ReportTableDto>> Expiry([FromQuery] ReportFilter filter) => Get(ReportTypes.Expiry, filter);
    [HttpGet("low-stock")] public Task<ActionResult<ReportTableDto>> LowStock([FromQuery] ReportFilter filter) => Get(ReportTypes.LowStock, filter);
    [HttpGet("suppliers")] public Task<ActionResult<ReportTableDto>> Suppliers([FromQuery] ReportFilter filter) => Get(ReportTypes.Suppliers, filter);
    [HttpGet("customers")] public Task<ActionResult<ReportTableDto>> Customers([FromQuery] ReportFilter filter) => Get(ReportTypes.Customers, filter);
    [HttpGet("expenses")] public Task<ActionResult<ReportTableDto>> Expenses([FromQuery] ReportFilter filter) => Get(ReportTypes.Expenses, filter);
    [HttpGet("returns")] public Task<ActionResult<ReportTableDto>> Returns([FromQuery] ReportFilter filter) => Get(ReportTypes.Returns, filter);
    [HttpGet("payments")] public Task<ActionResult<ReportTableDto>> Payments([FromQuery] ReportFilter filter) => Get(ReportTypes.Payments, filter);
    [HttpGet("employee-activity")] public Task<ActionResult<ReportTableDto>> EmployeeActivity([FromQuery] ReportFilter filter) => Get(ReportTypes.EmployeeActivity, filter);

    [HttpGet("export")]
    [Authorize(Policy = Permissions.ReportsExport)]
    public async Task<IActionResult> Export(
        [FromQuery] string type,
        [FromQuery] string format,
        [FromQuery] ReportFilter filter)
    {
        var report = await GetReportAsync(type, filter);
        var exportFormat = format.Trim().Equals("pdf", StringComparison.OrdinalIgnoreCase)
            ? ReportExportFormat.Pdf
            : format.Trim().Equals("excel", StringComparison.OrdinalIgnoreCase) ||
              format.Trim().Equals("xlsx", StringComparison.OrdinalIgnoreCase)
                ? ReportExportFormat.Excel
                : throw new ValidationFailedException("format", "Supported formats are excel and pdf.");

        var bytes = exportFormat == ReportExportFormat.Pdf
            ? _export.ExportPdf(report)
            : _export.ExportExcel(report);

        var fileName = $"{type}-{DateTime.UtcNow:yyyyMMddHHmmss}{_export.GetFileExtension(exportFormat)}";
        return File(bytes, _export.GetContentType(exportFormat), fileName);
    }

    private async Task<ReportTableDto> GetReportAsync(string type, ReportFilter filter)
    {
        if (!ReportTypes.All.Any(t => t.Equals(type, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ValidationFailedException("type", $"Unknown report type. Valid types: {string.Join(", ", ReportTypes.All)}");
        }

        return await _reports.GetAsync(type.ToLowerInvariant(), filter, Ct);
    }
}

[Route("api/settings")]
public sealed class SettingsController : ApiControllerBase
{
    private readonly ISettingsService _settings;

    public SettingsController(ISettingsService settings) => _settings = settings;

    [HttpGet]
    [Authorize(Policy = Permissions.SettingsView)]
    public async Task<ActionResult<IReadOnlyList<SystemSettingDto>>> GetAll()
        => Ok(await _settings.GetAllAsync(Ct));

    [HttpPut]
    [Authorize(Policy = Permissions.SettingsManage)]
    public async Task<ActionResult<SystemSettingDto>> Update([FromBody] UpdateSystemSettingRequest request)
        => Ok(await _settings.UpdateAsync(request, Ct));
}

[Route("api/audit-logs")]
public sealed class AuditController : ApiControllerBase
{
    private readonly IAuditQueryService _audit;

    public AuditController(IAuditQueryService audit) => _audit = audit;

    [HttpGet]
    [Authorize(Policy = Permissions.AuditView)]
    public async Task<ActionResult<PagedResult<AuditLogDto>>> GetPaged(
        [FromQuery] QueryParameters query,
        [FromQuery] Guid? userId,
        [FromQuery] string? entityName,
        [FromQuery] string? action,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
        => Ok(await _audit.GetPagedAsync(query, userId, entityName, action, from, to, Ct));

    [HttpGet("{id:long}")]
    [Authorize(Policy = Permissions.AuditView)]
    public async Task<ActionResult<AuditLogDto>> GetById(long id) => Ok(await _audit.GetByIdAsync(id, Ct));
}
