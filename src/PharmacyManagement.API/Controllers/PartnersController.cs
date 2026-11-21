using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Customers;
using PharmacyManagement.Application.DTOs.Purchasing;
using PharmacyManagement.Application.DTOs.Sales;
using PharmacyManagement.Application.DTOs.Suppliers;
using PharmacyManagement.Application.Interfaces;

namespace PharmacyManagement.API.Controllers;

[Route("api/suppliers")]
public sealed class SuppliersController : ApiControllerBase
{
    private readonly ISupplierService _suppliers;

    public SuppliersController(ISupplierService suppliers) => _suppliers = suppliers;

    [HttpGet]
    [Authorize(Policy = Permissions.SuppliersView)]
    public async Task<ActionResult<PagedResult<SupplierDto>>> GetPaged(
        [FromQuery] QueryParameters query, [FromQuery] bool includeInactive = false)
        => Ok(await _suppliers.GetPagedAsync(query, includeInactive, Ct));

    [HttpGet("{id:int}")]
    [Authorize(Policy = Permissions.SuppliersView)]
    public async Task<ActionResult<SupplierDto>> GetById(int id) => Ok(await _suppliers.GetByIdAsync(id, Ct));

    [HttpPost]
    [Authorize(Policy = Permissions.SuppliersManage)]
    public async Task<ActionResult<SupplierDto>> Create([FromBody] CreateSupplierRequest request)
        => Ok(await _suppliers.CreateAsync(request, Ct));

    [HttpPut("{id:int}")]
    [Authorize(Policy = Permissions.SuppliersManage)]
    public async Task<ActionResult<SupplierDto>> Update(int id, [FromBody] UpdateSupplierRequest request)
        => Ok(await _suppliers.UpdateAsync(id, request, Ct));

    [HttpDelete("{id:int}")]
    [Authorize(Policy = Permissions.SuppliersManage)]
    public async Task<IActionResult> Delete(int id)
    {
        await _suppliers.DeleteAsync(id, Ct);
        return NoContent();
    }

    [HttpGet("{id:int}/purchases")]
    [Authorize(Policy = Permissions.SuppliersView)]
    public async Task<ActionResult<PagedResult<PurchaseOrderListItemDto>>> GetPurchases(int id, [FromQuery] QueryParameters query)
        => Ok(await _suppliers.GetPurchasesAsync(id, query, Ct));

    [HttpGet("{id:int}/payments")]
    [Authorize(Policy = Permissions.SuppliersView)]
    public async Task<ActionResult<PagedResult<SupplierPaymentDto>>> GetPayments(int id, [FromQuery] QueryParameters query)
        => Ok(await _suppliers.GetPaymentsAsync(id, query, Ct));

    [HttpPost("{id:int}/payments")]
    [Authorize(Policy = Permissions.SuppliersManage)]
    public async Task<ActionResult<SupplierPaymentDto>> RecordPayment(int id, [FromBody] RecordSupplierPaymentRequest request)
        => Ok(await _suppliers.RecordPaymentAsync(id, request, Ct));

    [HttpGet("{id:int}/statement")]
    [Authorize(Policy = Permissions.SuppliersView)]
    public async Task<ActionResult<StatementDto>> GetStatement(int id, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        => Ok(await _suppliers.GetStatementAsync(id, from, to, Ct));
}

[Route("api/customers")]
public sealed class CustomersController : ApiControllerBase
{
    private readonly ICustomerService _customers;

    public CustomersController(ICustomerService customers) => _customers = customers;

    [HttpGet]
    [Authorize(Policy = Permissions.CustomersView)]
    public async Task<ActionResult<PagedResult<CustomerDto>>> GetPaged(
        [FromQuery] QueryParameters query, [FromQuery] bool includeInactive = false)
        => Ok(await _customers.GetPagedAsync(query, includeInactive, Ct));

    [HttpGet("{id:int}")]
    [Authorize(Policy = Permissions.CustomersView)]
    public async Task<ActionResult<CustomerDto>> GetById(int id) => Ok(await _customers.GetByIdAsync(id, Ct));

    [HttpPost]
    [Authorize(Policy = Permissions.CustomersManage)]
    public async Task<ActionResult<CustomerDto>> Create([FromBody] CreateCustomerRequest request)
        => Ok(await _customers.CreateAsync(request, Ct));

    [HttpPut("{id:int}")]
    [Authorize(Policy = Permissions.CustomersManage)]
    public async Task<ActionResult<CustomerDto>> Update(int id, [FromBody] UpdateCustomerRequest request)
        => Ok(await _customers.UpdateAsync(id, request, Ct));

    [HttpDelete("{id:int}")]
    [Authorize(Policy = Permissions.CustomersManage)]
    public async Task<IActionResult> Delete(int id)
    {
        await _customers.DeleteAsync(id, Ct);
        return NoContent();
    }

    [HttpGet("{id:int}/purchases")]
    [Authorize(Policy = Permissions.CustomersView)]
    public async Task<ActionResult<PagedResult<SaleListItemDto>>> GetPurchases(int id, [FromQuery] QueryParameters query)
        => Ok(await _customers.GetPurchasesAsync(id, query, Ct));

    [HttpGet("{id:int}/payments")]
    [Authorize(Policy = Permissions.CustomersView)]
    public async Task<ActionResult<PagedResult<CustomerPaymentDto>>> GetPayments(int id, [FromQuery] QueryParameters query)
        => Ok(await _customers.GetPaymentsAsync(id, query, Ct));

    [HttpPost("{id:int}/payments")]
    [Authorize(Policy = Permissions.CustomersManage)]
    public async Task<ActionResult<CustomerPaymentDto>> RecordPayment(int id, [FromBody] RecordCustomerPaymentRequest request)
        => Ok(await _customers.RecordPaymentAsync(id, request, Ct));

    [HttpGet("{id:int}/statement")]
    [Authorize(Policy = Permissions.CustomersView)]
    public async Task<ActionResult<StatementDto>> GetStatement(int id, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        => Ok(await _customers.GetStatementAsync(id, from, to, Ct));
}
