using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Dashboard;
using PharmacyManagement.Application.DTOs.Inventory;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Enums;

namespace PharmacyManagement.API.Controllers;

[Route("api/inventory")]
public sealed class InventoryController : ApiControllerBase
{
    private readonly IInventoryService _inventory;

    public InventoryController(IInventoryService inventory) => _inventory = inventory;

    [HttpGet("batches")]
    [Authorize(Policy = Permissions.InventoryView)]
    public async Task<ActionResult<PagedResult<BatchDto>>> GetBatches(
        [FromQuery] QueryParameters query,
        [FromQuery] int? medicineId,
        [FromQuery] int? branchId,
        [FromQuery] bool expiringOnly = false,
        [FromQuery] bool expiredOnly = false,
        [FromQuery] int days = 90)
        => Ok(await _inventory.GetBatchesAsync(query, medicineId, branchId, expiringOnly, expiredOnly, days, Ct));

    [HttpGet("stock")]
    [Authorize(Policy = Permissions.InventoryView)]
    public async Task<ActionResult<PagedResult<StockSummaryDto>>> GetStock(
        [FromQuery] QueryParameters query, [FromQuery] int? branchId, [FromQuery] bool lowStockOnly = false)
        => Ok(await _inventory.GetStockAsync(query, branchId, lowStockOnly, Ct));

    [HttpGet("movements")]
    [Authorize(Policy = Permissions.InventoryView)]
    public async Task<ActionResult<PagedResult<InventoryTransactionDto>>> GetMovements(
        [FromQuery] QueryParameters query,
        [FromQuery] int? medicineId,
        [FromQuery] int? branchId,
        [FromQuery] InventoryTransactionType? type,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
        => Ok(await _inventory.GetMovementsAsync(query, medicineId, branchId, type, from, to, Ct));

    [HttpGet("valuation")]
    [Authorize(Policy = Permissions.InventoryView)]
    public async Task<ActionResult<PagedResult<StockSummaryDto>>> GetValuation([FromQuery] QueryParameters query, [FromQuery] int? branchId)
        => Ok(await _inventory.GetStockAsync(query, branchId, false, Ct));

    [HttpGet("alerts/low-stock")]
    [Authorize(Policy = Permissions.InventoryView)]
    public async Task<ActionResult<IReadOnlyList<LowStockItemDto>>> LowStock([FromQuery] int? branchId, [FromQuery] int limit = 50)
        => Ok(await _inventory.GetLowStockAsync(branchId, limit, Ct));

    [HttpGet("alerts/expiring")]
    [Authorize(Policy = Permissions.InventoryView)]
    public async Task<ActionResult<IReadOnlyList<ExpiringItemDto>>> Expiring(
        [FromQuery] int days = 90, [FromQuery] int? branchId = null, [FromQuery] int limit = 50)
        => Ok(await _inventory.GetExpiringAsync(days, branchId, limit, Ct));

    [HttpGet("alerts/expired")]
    [Authorize(Policy = Permissions.InventoryView)]
    public async Task<ActionResult<IReadOnlyList<ExpiringItemDto>>> Expired([FromQuery] int? branchId, [FromQuery] int limit = 50)
        => Ok(await _inventory.GetExpiredAsync(branchId, limit, Ct));

    [HttpPost("stock-in")]
    [Authorize(Policy = Permissions.InventoryAdjust)]
    public async Task<ActionResult<BatchDto>> StockIn([FromBody] StockInRequest request)
        => Ok(await _inventory.StockInAsync(request, Ct));

    [HttpPost("stock-out")]
    [Authorize(Policy = Permissions.InventoryAdjust)]
    public async Task<ActionResult<BatchDto>> StockOut([FromBody] StockOutRequest request)
        => Ok(await _inventory.StockOutAsync(request, Ct));

    [HttpPost("adjustments")]
    [Authorize(Policy = Permissions.InventoryAdjust)]
    public async Task<ActionResult<BatchDto>> Adjust([FromBody] StockAdjustmentRequest request)
        => Ok(await _inventory.AdjustAsync(request, Ct));
}

[Route("api/stock-transfers")]
public sealed class StockTransfersController : ApiControllerBase
{
    private readonly IStockTransferService _transfers;

    public StockTransfersController(IStockTransferService transfers) => _transfers = transfers;

    [HttpGet]
    [Authorize(Policy = Permissions.InventoryView)]
    public async Task<ActionResult<PagedResult<StockTransferDto>>> GetPaged(
        [FromQuery] QueryParameters query, [FromQuery] StockTransferStatus? status)
        => Ok(await _transfers.GetPagedAsync(query, status, Ct));

    [HttpGet("{id:int}")]
    [Authorize(Policy = Permissions.InventoryView)]
    public async Task<ActionResult<StockTransferDto>> GetById(int id) => Ok(await _transfers.GetByIdAsync(id, Ct));

    [HttpPost]
    [Authorize(Policy = Permissions.InventoryTransfer)]
    public async Task<ActionResult<StockTransferDto>> Create([FromBody] CreateStockTransferRequest request)
        => Ok(await _transfers.CreateAsync(request, Ct));

    [HttpPost("{id:int}/approve")]
    [Authorize(Policy = Permissions.InventoryTransfer)]
    public async Task<ActionResult<StockTransferDto>> Approve(int id) => Ok(await _transfers.ApproveAsync(id, Ct));

    [HttpPost("{id:int}/complete")]
    [Authorize(Policy = Permissions.InventoryTransfer)]
    public async Task<ActionResult<StockTransferDto>> Complete(int id) => Ok(await _transfers.CompleteAsync(id, Ct));

    [HttpPost("{id:int}/cancel")]
    [Authorize(Policy = Permissions.InventoryTransfer)]
    public async Task<ActionResult<StockTransferDto>> Cancel(int id) => Ok(await _transfers.CancelAsync(id, Ct));
}
