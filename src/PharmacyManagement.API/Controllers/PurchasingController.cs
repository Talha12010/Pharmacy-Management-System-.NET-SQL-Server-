using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Purchasing;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Enums;

namespace PharmacyManagement.API.Controllers;

[Route("api/purchase-orders")]
public sealed class PurchaseOrdersController : ApiControllerBase
{
    private readonly IPurchaseService _purchase;

    public PurchaseOrdersController(IPurchaseService purchase) => _purchase = purchase;

    [HttpGet]
    [Authorize(Policy = Permissions.PurchasesView)]
    public async Task<ActionResult<PagedResult<PurchaseOrderListItemDto>>> GetPaged(
        [FromQuery] QueryParameters query,
        [FromQuery] PurchaseOrderStatus? status,
        [FromQuery] int? supplierId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
        => Ok(await _purchase.GetPagedAsync(query, status, supplierId, from, to, Ct));

    [HttpGet("{id:int}")]
    [Authorize(Policy = Permissions.PurchasesView)]
    public async Task<ActionResult<PurchaseOrderDto>> GetById(int id) => Ok(await _purchase.GetByIdAsync(id, Ct));

    [HttpPost]
    [Authorize(Policy = Permissions.PurchasesCreate)]
    public async Task<ActionResult<PurchaseOrderDto>> Create([FromBody] CreatePurchaseOrderRequest request)
        => Ok(await _purchase.CreateAsync(request, Ct));

    [HttpPut("{id:int}")]
    [Authorize(Policy = Permissions.PurchasesCreate)]
    public async Task<ActionResult<PurchaseOrderDto>> Update(int id, [FromBody] CreatePurchaseOrderRequest request)
        => Ok(await _purchase.UpdateAsync(id, request, Ct));

    [HttpPost("{id:int}/submit")]
    [Authorize(Policy = Permissions.PurchasesCreate)]
    public async Task<ActionResult<PurchaseOrderDto>> Submit(int id) => Ok(await _purchase.SubmitAsync(id, Ct));

    [HttpPost("{id:int}/approve")]
    [Authorize(Policy = Permissions.PurchasesApprove)]
    public async Task<ActionResult<PurchaseOrderDto>> Approve(int id) => Ok(await _purchase.ApproveAsync(id, Ct));

    [HttpPost("{id:int}/reject")]
    [Authorize(Policy = Permissions.PurchasesApprove)]
    public async Task<ActionResult<PurchaseOrderDto>> Reject(int id, [FromBody] ReasonRequest request)
        => Ok(await _purchase.RejectAsync(id, request.Reason, Ct));

    [HttpPost("{id:int}/receive")]
    [Authorize(Policy = Permissions.PurchasesReceive)]
    public async Task<ActionResult<GoodsReceiptDto>> Receive(int id, [FromBody] ReceivePurchaseOrderRequest request)
        => Ok(await _purchase.ReceiveAsync(id, request, Ct));

    [HttpPost("{id:int}/cancel")]
    [Authorize(Policy = Permissions.PurchasesApprove)]
    public async Task<ActionResult<PurchaseOrderDto>> Cancel(int id) => Ok(await _purchase.CancelAsync(id, Ct));
}

[Route("api/goods-receipts")]
public sealed class GoodsReceiptsController : ApiControllerBase
{
    private readonly IPurchaseService _purchase;

    public GoodsReceiptsController(IPurchaseService purchase) => _purchase = purchase;

    [HttpGet]
    [Authorize(Policy = Permissions.PurchasesView)]
    public async Task<ActionResult<PagedResult<GoodsReceiptDto>>> GetPaged(
        [FromQuery] QueryParameters query, [FromQuery] int? purchaseOrderId)
        => Ok(await _purchase.GetReceiptsAsync(query, purchaseOrderId, Ct));

    [HttpGet("{id:int}")]
    [Authorize(Policy = Permissions.PurchasesView)]
    public async Task<ActionResult<GoodsReceiptDto>> GetById(int id) => Ok(await _purchase.GetReceiptByIdAsync(id, Ct));
}
