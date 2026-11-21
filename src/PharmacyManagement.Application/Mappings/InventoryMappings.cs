using PharmacyManagement.Application.DTOs.Inventory;
using PharmacyManagement.Domain.Entities;

namespace PharmacyManagement.Application.Mappings;

public static class InventoryMappings
{
    public static BatchDto ToDto(this MedicineBatch batch) => new()
    {
        Id = batch.Id,
        MedicineId = batch.MedicineId,
        MedicineName = batch.Medicine?.Name ?? string.Empty,
        BranchId = batch.BranchId,
        BranchName = batch.Branch?.Name ?? string.Empty,
        SupplierId = batch.SupplierId,
        SupplierName = batch.Supplier?.Name,
        BatchNumber = batch.BatchNumber,
        ManufacturingDate = batch.ManufacturingDate,
        ExpiryDate = batch.ExpiryDate,
        PurchasePrice = batch.PurchasePrice,
        SellingPrice = batch.SellingPrice,
        InitialQuantity = batch.InitialQuantity,
        CurrentQuantity = batch.CurrentQuantity,
        ReceivedAtUtc = batch.ReceivedAtUtc,
        IsActive = batch.IsActive,
        IsExpired = batch.ExpiryDate.Date <= DateTime.UtcNow.Date,
        DaysToExpiry = (batch.ExpiryDate.Date - DateTime.UtcNow.Date).Days
    };

    public static InventoryTransactionDto ToDto(this InventoryTransaction transaction) => new()
    {
        Id = transaction.Id,
        MedicineId = transaction.MedicineId,
        MedicineName = transaction.Medicine?.Name ?? string.Empty,
        BatchNumber = transaction.Batch?.BatchNumber,
        BranchName = transaction.Branch?.Name ?? string.Empty,
        Type = transaction.Type,
        QuantityChange = transaction.QuantityChange,
        QuantityBefore = transaction.QuantityBefore,
        QuantityAfter = transaction.QuantityAfter,
        UnitCost = transaction.UnitCost,
        ReferenceType = transaction.ReferenceType,
        ReferenceId = transaction.ReferenceId,
        Notes = transaction.Notes,
        PerformedByUserId = transaction.PerformedByUserId,
        CreatedAtUtc = transaction.CreatedAtUtc
    };

    public static StockTransferDto ToDto(this StockTransfer transfer) => new()
    {
        Id = transfer.Id,
        TransferNumber = transfer.TransferNumber,
        FromBranchId = transfer.FromBranchId,
        FromBranchName = transfer.FromBranch?.Name ?? string.Empty,
        ToBranchId = transfer.ToBranchId,
        ToBranchName = transfer.ToBranch?.Name ?? string.Empty,
        Status = transfer.Status,
        RequestedByUserId = transfer.RequestedByUserId,
        ApprovedByUserId = transfer.ApprovedByUserId,
        TransferDateUtc = transfer.TransferDateUtc,
        CreatedAtUtc = transfer.CreatedAtUtc,
        Notes = transfer.Notes,
        Details = transfer.Details.Select(d => new StockTransferDetailDto
        {
            Id = d.Id,
            MedicineId = d.MedicineId,
            MedicineName = d.Medicine?.Name ?? string.Empty,
            BatchId = d.BatchId,
            BatchNumber = d.Batch?.BatchNumber ?? string.Empty,
            ExpiryDate = d.Batch?.ExpiryDate ?? default,
            Quantity = d.Quantity,
            ReceivedQuantity = d.ReceivedQuantity
        }).ToList()
    };
}
