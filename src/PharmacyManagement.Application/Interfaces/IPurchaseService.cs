using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Purchasing;
using PharmacyManagement.Domain.Enums;

namespace PharmacyManagement.Application.Interfaces;

public interface IPurchaseService
{
    Task<PagedResult<PurchaseOrderListItemDto>> GetPagedAsync(
        QueryParameters query, PurchaseOrderStatus? status, int? supplierId, DateTime? from, DateTime? to,
        CancellationToken cancellationToken = default);

    Task<PurchaseOrderDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PurchaseOrderDto> CreateAsync(CreatePurchaseOrderRequest request, CancellationToken cancellationToken = default);
    Task<PurchaseOrderDto> UpdateAsync(int id, CreatePurchaseOrderRequest request, CancellationToken cancellationToken = default);
    Task<PurchaseOrderDto> SubmitAsync(int id, CancellationToken cancellationToken = default);
    Task<PurchaseOrderDto> ApproveAsync(int id, CancellationToken cancellationToken = default);
    Task<PurchaseOrderDto> RejectAsync(int id, string reason, CancellationToken cancellationToken = default);
    Task<PurchaseOrderDto> CancelAsync(int id, CancellationToken cancellationToken = default);
    Task<GoodsReceiptDto> ReceiveAsync(int id, ReceivePurchaseOrderRequest request, CancellationToken cancellationToken = default);
    Task<PagedResult<GoodsReceiptDto>> GetReceiptsAsync(QueryParameters query, int? purchaseOrderId, CancellationToken cancellationToken = default);
    Task<GoodsReceiptDto> GetReceiptByIdAsync(int id, CancellationToken cancellationToken = default);
}
