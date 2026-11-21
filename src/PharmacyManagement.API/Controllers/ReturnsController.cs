using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Returns;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Enums;

namespace PharmacyManagement.API.Controllers;

[Route("api/returns")]
public sealed class ReturnsController : ApiControllerBase
{
    private readonly IReturnService _returns;

    public ReturnsController(IReturnService returns) => _returns = returns;

    [HttpGet]
    [Authorize(Policy = Permissions.ReturnsView)]
    public async Task<ActionResult<PagedResult<ReturnDto>>> GetPaged(
        [FromQuery] QueryParameters query,
        [FromQuery] ReturnStatus? status,
        [FromQuery] ReturnType? type,
        [FromQuery] int? branchId)
        => Ok(await _returns.GetPagedAsync(query, status, type, branchId, Ct));

    [HttpGet("{id:int}")]
    [Authorize(Policy = Permissions.ReturnsView)]
    public async Task<ActionResult<ReturnDto>> GetById(int id) => Ok(await _returns.GetByIdAsync(id, Ct));

    [HttpGet("{id:int}/refund")]
    [Authorize(Policy = Permissions.ReturnsView)]
    public async Task<ActionResult<RefundDto>> GetRefund(int id)
    {
        var result = await _returns.GetByIdAsync(id, Ct);
        return Ok(result.Refunds.FirstOrDefault() ?? new RefundDto());
    }

    [HttpPost]
    [Authorize(Policy = Permissions.ReturnsCreate)]
    public async Task<ActionResult<ReturnDto>> Create([FromBody] CreateReturnRequest request)
        => Ok(await _returns.CreateAsync(request, Ct));

    [HttpPost("{id:int}/approve")]
    [Authorize(Policy = Permissions.ReturnsApprove)]
    public async Task<ActionResult<ReturnDto>> Approve(int id, [FromBody] ApproveReturnRequest request)
        => Ok(await _returns.ApproveAsync(id, request, Ct));

    [HttpPost("{id:int}/reject")]
    [Authorize(Policy = Permissions.ReturnsApprove)]
    public async Task<ActionResult<ReturnDto>> Reject(int id, [FromBody] RejectReturnRequest request)
        => Ok(await _returns.RejectAsync(id, request.Reason, Ct));
}
