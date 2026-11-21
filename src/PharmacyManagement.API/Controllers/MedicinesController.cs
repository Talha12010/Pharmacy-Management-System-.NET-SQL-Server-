using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Dashboard;
using PharmacyManagement.Application.DTOs.Inventory;
using PharmacyManagement.Application.DTOs.Medicines;
using PharmacyManagement.Application.Interfaces;

namespace PharmacyManagement.API.Controllers;

[Route("api/medicines")]
public sealed class MedicinesController : ApiControllerBase
{
    private readonly IMedicineService _medicines;
    private readonly IInventoryService _inventory;

    public MedicinesController(IMedicineService medicines, IInventoryService inventory)
    {
        _medicines = medicines;
        _inventory = inventory;
    }

    [HttpGet]
    [Authorize(Policy = Permissions.MedicinesView)]
    public async Task<ActionResult<PagedResult<MedicineListItemDto>>> GetPaged(
        [FromQuery] QueryParameters query,
        [FromQuery] int? categoryId,
        [FromQuery] int? manufacturerId,
        [FromQuery] bool? isActive)
        => Ok(await _medicines.GetPagedAsync(query, categoryId, manufacturerId, isActive, Ct));

    [HttpGet("barcode/{barcode}")]
    [Authorize(Policy = Permissions.MedicinesView)]
    public async Task<ActionResult<MedicineDto>> GetByBarcode(string barcode)
        => Ok(await _medicines.GetByBarcodeAsync(barcode, Ct));

    [HttpGet("lookup")]
    [Authorize(Policy = Permissions.MedicinesView)]
    public async Task<ActionResult<IReadOnlyList<MedicineLookupDto>>> Lookup(
        [FromQuery] string? q, [FromQuery] int? branchId, [FromQuery] int limit = 20)
        => Ok(await _medicines.LookupAsync(q, branchId, limit, Ct));

    [HttpGet("low-stock")]
    [Authorize(Policy = Permissions.InventoryView)]
    public async Task<ActionResult<IReadOnlyList<LowStockItemDto>>> LowStock([FromQuery] int? branchId, [FromQuery] int limit = 50)
        => Ok(await _inventory.GetLowStockAsync(branchId, limit, Ct));

    [HttpGet("expiring")]
    [Authorize(Policy = Permissions.InventoryView)]
    public async Task<ActionResult<IReadOnlyList<ExpiringItemDto>>> Expiring(
        [FromQuery] int days = 90, [FromQuery] int? branchId = null, [FromQuery] int limit = 50)
        => Ok(await _inventory.GetExpiringAsync(days, branchId, limit, Ct));

    [HttpGet("expired")]
    [Authorize(Policy = Permissions.InventoryView)]
    public async Task<ActionResult<IReadOnlyList<ExpiringItemDto>>> Expired([FromQuery] int? branchId, [FromQuery] int limit = 50)
        => Ok(await _inventory.GetExpiredAsync(branchId, limit, Ct));

    [HttpGet("{id:int}")]
    [Authorize(Policy = Permissions.MedicinesView)]
    public async Task<ActionResult<MedicineDto>> GetById(int id) => Ok(await _medicines.GetByIdAsync(id, Ct));

    [HttpGet("{id:int}/batches")]
    [Authorize(Policy = Permissions.InventoryView)]
    public async Task<ActionResult<IReadOnlyList<BatchDto>>> GetBatches(int id, [FromQuery] int? branchId)
    {
        var result = await _inventory.GetBatchesAsync(
            new QueryParameters { PageSize = 200 }, id, branchId, false, false, 90, Ct);
        return Ok(result.Items);
    }

    [HttpGet("{id:int}/stock-history")]
    [Authorize(Policy = Permissions.InventoryView)]
    public async Task<ActionResult<IReadOnlyList<InventoryTransactionDto>>> GetStockHistory(int id, [FromQuery] int? branchId)
    {
        var result = await _inventory.GetMovementsAsync(
            new QueryParameters { PageSize = 200 }, id, branchId, null, null, null, Ct);
        return Ok(result.Items);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.MedicinesManage)]
    public async Task<ActionResult<MedicineDto>> Create([FromBody] CreateMedicineRequest request)
        => Ok(await _medicines.CreateAsync(request, Ct));

    [HttpPut("{id:int}")]
    [Authorize(Policy = Permissions.MedicinesManage)]
    public async Task<ActionResult<MedicineDto>> Update(int id, [FromBody] UpdateMedicineRequest request)
        => Ok(await _medicines.UpdateAsync(id, request, Ct));

    [HttpDelete("{id:int}")]
    [Authorize(Policy = Permissions.MedicinesManage)]
    public async Task<IActionResult> Delete(int id)
    {
        await _medicines.DeleteAsync(id, Ct);
        return NoContent();
    }
}
