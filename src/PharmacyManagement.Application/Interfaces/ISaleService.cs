using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Sales;
using PharmacyManagement.Domain.Enums;

namespace PharmacyManagement.Application.Interfaces;

public interface ISaleService
{
    Task<PagedResult<SaleListItemDto>> GetPagedAsync(
        QueryParameters query, DateTime? from, DateTime? to, int? customerId, int? branchId,
        SaleStatus? status, CancellationToken cancellationToken = default);

    Task<SaleDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<SaleDto> CreateAsync(CreateSaleRequest request, CancellationToken cancellationToken = default);
    Task<SaleDto> CancelAsync(int id, string reason, CancellationToken cancellationToken = default);
    Task<SalePaymentDto> RecordPaymentAsync(int id, RecordSalePaymentRequest request, CancellationToken cancellationToken = default);
    Task<SaleInvoiceDto> GetInvoiceAsync(int id, CancellationToken cancellationToken = default);
    Task<TodaySalesSummaryDto> GetTodaySummaryAsync(int? branchId, CancellationToken cancellationToken = default);
}
