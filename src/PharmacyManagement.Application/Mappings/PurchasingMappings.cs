using PharmacyManagement.Application.DTOs.Purchasing;
using PharmacyManagement.Domain.Entities;

namespace PharmacyManagement.Application.Mappings;

public static class PurchasingMappings
{
    public static PurchaseOrderDto ToDto(this PurchaseOrder order) => new()
    {
        Id = order.Id,
        OrderNumber = order.OrderNumber,
        SupplierId = order.SupplierId,
        SupplierName = order.Supplier?.Name ?? string.Empty,
        BranchId = order.BranchId,
        BranchName = order.Branch?.Name ?? string.Empty,
        OrderDateUtc = order.OrderDateUtc,
        ExpectedDeliveryDate = order.ExpectedDeliveryDate,
        Status = order.Status,
        SubTotal = order.SubTotal,
        TaxAmount = order.TaxAmount,
        DiscountAmount = order.DiscountAmount,
        TotalAmount = order.TotalAmount,
        Notes = order.Notes,
        CreatedByUserId = order.CreatedByUserId,
        ApprovedByUserId = order.ApprovedByUserId,
        ApprovedAtUtc = order.ApprovedAtUtc,
        CreatedAtUtc = order.CreatedAtUtc,
        RejectionReason = order.RejectionReason,
        Details = order.Details.Select(d => new PurchaseOrderDetailDto
        {
            Id = d.Id,
            MedicineId = d.MedicineId,
            MedicineName = d.Medicine?.Name ?? string.Empty,
            OrderedQuantity = d.OrderedQuantity,
            ReceivedQuantity = d.ReceivedQuantity,
            UnitPrice = d.UnitPrice,
            TaxPercentage = d.TaxPercentage,
            LineTotal = d.LineTotal,
            BatchNumber = d.BatchNumber,
            ManufacturingDate = d.ManufacturingDate,
            ExpiryDate = d.ExpiryDate
        }).ToList(),
        Receipts = order.GoodsReceipts.Select(r => new GoodsReceiptSummaryDto
        {
            Id = r.Id,
            ReceiptNumber = r.ReceiptNumber,
            ReceivedAtUtc = r.ReceivedAtUtc,
            TotalAmount = r.TotalAmount
        }).ToList()
    };

    public static GoodsReceiptDto ToDto(this GoodsReceipt receipt) => new()
    {
        Id = receipt.Id,
        ReceiptNumber = receipt.ReceiptNumber,
        PurchaseOrderId = receipt.PurchaseOrderId,
        OrderNumber = receipt.PurchaseOrder?.OrderNumber ?? string.Empty,
        SupplierName = receipt.PurchaseOrder?.Supplier?.Name ?? string.Empty,
        BranchName = receipt.PurchaseOrder?.Branch?.Name ?? string.Empty,
        ReceivedAtUtc = receipt.ReceivedAtUtc,
        ReceivedByUserId = receipt.ReceivedByUserId,
        TotalAmount = receipt.TotalAmount,
        Notes = receipt.Notes,
        Details = receipt.Details.Select(d => new GoodsReceiptDetailDto
        {
            Id = d.Id,
            MedicineId = d.MedicineId,
            MedicineName = d.Medicine?.Name ?? string.Empty,
            BatchId = d.BatchId,
            BatchNumber = d.BatchNumber,
            ManufacturingDate = d.ManufacturingDate,
            ExpiryDate = d.ExpiryDate,
            Quantity = d.Quantity,
            UnitCost = d.UnitCost,
            LineTotal = d.LineTotal
        }).ToList()
    };
}
