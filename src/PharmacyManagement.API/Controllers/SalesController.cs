using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Sales;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Enums;

namespace PharmacyManagement.API.Controllers;

[Route("api/sales")]
public sealed class SalesController : ApiControllerBase
{
    private readonly ISaleService _sales;

    public SalesController(ISaleService sales) => _sales = sales;

    [HttpGet]
    [Authorize(Policy = Permissions.SalesView)]
    public async Task<ActionResult<PagedResult<SaleListItemDto>>> GetPaged(
        [FromQuery] QueryParameters query,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int? customerId,
        [FromQuery] int? branchId,
        [FromQuery] SaleStatus? status)
        => Ok(await _sales.GetPagedAsync(query, from, to, customerId, branchId, status, Ct));

    [HttpGet("today-summary")]
    [Authorize(Policy = Permissions.SalesView)]
    public async Task<ActionResult<TodaySalesSummaryDto>> TodaySummary([FromQuery] int? branchId)
        => Ok(await _sales.GetTodaySummaryAsync(branchId, Ct));

    [HttpGet("{id:int}")]
    [Authorize(Policy = Permissions.SalesView)]
    public async Task<ActionResult<SaleDto>> GetById(int id) => Ok(await _sales.GetByIdAsync(id, Ct));

    [HttpGet("{id:int}/invoice")]
    [Authorize(Policy = Permissions.SalesView)]
    public async Task<ActionResult<SaleInvoiceDto>> GetInvoice(int id) => Ok(await _sales.GetInvoiceAsync(id, Ct));

    [HttpPost]
    [Authorize(Policy = Permissions.SalesCreate)]
    public async Task<ActionResult<SaleDto>> Create([FromBody] CreateSaleRequest request)
        => Ok(await _sales.CreateAsync(request, Ct));

    [HttpPost("{id:int}/cancel")]
    [Authorize(Policy = Permissions.SalesCancel)]
    public async Task<ActionResult<SaleDto>> Cancel(int id, [FromBody] CancelSaleRequest request)
        => Ok(await _sales.CancelAsync(id, request.Reason, Ct));

    [HttpPost("{id:int}/payments")]
    [Authorize(Policy = Permissions.SalesCreate)]
    public async Task<ActionResult<SalePaymentDto>> RecordPayment(int id, [FromBody] RecordSalePaymentRequest request)
        => Ok(await _sales.RecordPaymentAsync(id, request, Ct));
}
