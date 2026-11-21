using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Customers;
using PharmacyManagement.Application.DTOs.Purchasing;
using PharmacyManagement.Application.DTOs.Sales;
using PharmacyManagement.Application.DTOs.Suppliers;

namespace PharmacyManagement.Application.Interfaces;

public interface ISupplierService
{
    Task<PagedResult<SupplierDto>> GetPagedAsync(QueryParameters query, bool includeInactive, CancellationToken cancellationToken = default);
    Task<SupplierDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<SupplierDto> CreateAsync(CreateSupplierRequest request, CancellationToken cancellationToken = default);
    Task<SupplierDto> UpdateAsync(int id, UpdateSupplierRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<PagedResult<PurchaseOrderListItemDto>> GetPurchasesAsync(int supplierId, QueryParameters query, CancellationToken cancellationToken = default);
    Task<PagedResult<SupplierPaymentDto>> GetPaymentsAsync(int supplierId, QueryParameters query, CancellationToken cancellationToken = default);
    Task<SupplierPaymentDto> RecordPaymentAsync(int supplierId, RecordSupplierPaymentRequest request, CancellationToken cancellationToken = default);
    Task<StatementDto> GetStatementAsync(int supplierId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
}

public interface ICustomerService
{
    Task<PagedResult<CustomerDto>> GetPagedAsync(QueryParameters query, bool includeInactive, CancellationToken cancellationToken = default);
    Task<CustomerDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default);
    Task<CustomerDto> UpdateAsync(int id, UpdateCustomerRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<PagedResult<SaleListItemDto>> GetPurchasesAsync(int customerId, QueryParameters query, CancellationToken cancellationToken = default);
    Task<PagedResult<CustomerPaymentDto>> GetPaymentsAsync(int customerId, QueryParameters query, CancellationToken cancellationToken = default);
    Task<CustomerPaymentDto> RecordPaymentAsync(int customerId, RecordCustomerPaymentRequest request, CancellationToken cancellationToken = default);
    Task<StatementDto> GetStatementAsync(int customerId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
}
