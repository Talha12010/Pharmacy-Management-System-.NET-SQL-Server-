using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Dashboard;
using PharmacyManagement.Application.DTOs.Inventory;
using PharmacyManagement.Domain.Enums;

namespace PharmacyManagement.Application.Interfaces;

public interface IInventoryService
{
    Task<PagedResult<BatchDto>> GetBatchesAsync(
        QueryParameters query, int? medicineId, int? branchId, bool expiringOnly, bool expiredOnly, int days,
        CancellationToken cancellationToken = default);

    Task<PagedResult<StockSummaryDto>> GetStockAsync(
        QueryParameters query, int? branchId, bool lowStockOnly, CancellationToken cancellationToken = default);

    Task<PagedResult<InventoryTransactionDto>> GetMovementsAsync(
        QueryParameters query, int? medicineId, int? branchId, InventoryTransactionType? type,
        DateTime? from, DateTime? to, CancellationToken cancellationToken = default);

    Task<BatchDto> StockInAsync(StockInRequest request, CancellationToken cancellationToken = default);
    Task<BatchDto> StockOutAsync(StockOutRequest request, CancellationToken cancellationToken = default);
    Task<BatchDto> AdjustAsync(StockAdjustmentRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LowStockItemDto>> GetLowStockAsync(int? branchId, int limit = 50, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExpiringItemDto>> GetExpiringAsync(int days, int? branchId, int limit = 50, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExpiringItemDto>> GetExpiredAsync(int? branchId, int limit = 50, CancellationToken cancellationToken = default);
}

public interface IStockTransferService
{
    Task<PagedResult<StockTransferDto>> GetPagedAsync(QueryParameters query, StockTransferStatus? status, CancellationToken cancellationToken = default);
    Task<StockTransferDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<StockTransferDto> CreateAsync(CreateStockTransferRequest request, CancellationToken cancellationToken = default);
    Task<StockTransferDto> ApproveAsync(int id, CancellationToken cancellationToken = default);
    Task<StockTransferDto> CompleteAsync(int id, CancellationToken cancellationToken = default);
    Task<StockTransferDto> CancelAsync(int id, CancellationToken cancellationToken = default);
}
