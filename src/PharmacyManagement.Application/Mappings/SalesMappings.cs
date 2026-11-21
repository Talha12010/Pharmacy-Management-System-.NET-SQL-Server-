using PharmacyManagement.Application.DTOs.Prescriptions;
using PharmacyManagement.Application.DTOs.Returns;
using PharmacyManagement.Application.DTOs.Sales;
using PharmacyManagement.Domain.Entities;

namespace PharmacyManagement.Application.Mappings;

public static class SalesMappings
{
    public static SaleDto ToDto(this Sale sale) => new()
    {
        Id = sale.Id,
        InvoiceNumber = sale.InvoiceNumber,
        BranchId = sale.BranchId,
        BranchName = sale.Branch?.Name ?? string.Empty,
        CustomerId = sale.CustomerId,
        CustomerName = sale.Customer?.FullName,
        PrescriptionId = sale.PrescriptionId,
        PrescriptionNumber = sale.Prescription?.PrescriptionNumber,
        CashierUserId = sale.CashierUserId,
        SaleDateUtc = sale.SaleDateUtc,
        SubTotal = sale.SubTotal,
        DiscountAmount = sale.DiscountAmount,
        TaxAmount = sale.TaxAmount,
        TotalAmount = sale.TotalAmount,
        PaidAmount = sale.PaidAmount,
        ChangeAmount = sale.ChangeAmount,
        Status = sale.Status,
        PaymentStatus = sale.PaymentStatus,
        Notes = sale.Notes,
        CancelledAtUtc = sale.CancelledAtUtc,
        CancellationReason = sale.CancellationReason,
        Details = sale.Details.Select(d => new SaleDetailDto
        {
            Id = d.Id,
            MedicineId = d.MedicineId,
            MedicineName = d.Medicine?.Name ?? string.Empty,
            BatchId = d.BatchId,
            BatchNumber = d.BatchNumber,
            Quantity = d.Quantity,
            ReturnedQuantity = d.ReturnedQuantity,
            UnitPrice = d.UnitPrice,
            UnitCost = d.UnitCost,
            DiscountAmount = d.DiscountAmount,
            TaxPercentage = d.TaxPercentage,
            TaxAmount = d.TaxAmount,
            LineTotal = d.LineTotal
        }).ToList(),
        Payments = sale.Payments.Select(p => new SalePaymentDto
        {
            Id = p.Id,
            Method = p.Method,
            Amount = p.Amount,
            Reference = p.Reference,
            PaidAtUtc = p.PaidAtUtc
        }).ToList()
    };

    public static PrescriptionDto ToDto(this Prescription prescription) => new()
    {
        Id = prescription.Id,
        PrescriptionNumber = prescription.PrescriptionNumber,
        BranchId = prescription.BranchId,
        BranchName = prescription.Branch?.Name ?? string.Empty,
        CustomerId = prescription.CustomerId,
        CustomerName = prescription.Customer?.FullName,
        PatientName = prescription.PatientName,
        PatientPhone = prescription.PatientPhone,
        PatientAddress = prescription.PatientAddress,
        DoctorName = prescription.DoctorName,
        DoctorLicenseNumber = prescription.DoctorLicenseNumber,
        PrescriptionDate = prescription.PrescriptionDate,
        Status = prescription.Status,
        ImagePath = prescription.ImagePath,
        Notes = prescription.Notes,
        RejectionReason = prescription.RejectionReason,
        CreatedByUserId = prescription.CreatedByUserId,
        CreatedAtUtc = prescription.CreatedAtUtc,
        VerifiedByUserId = prescription.VerifiedByUserId,
        VerifiedAtUtc = prescription.VerifiedAtUtc,
        Details = prescription.Details.Select(d => new PrescriptionDetailDto
        {
            Id = d.Id,
            MedicineId = d.MedicineId,
            MedicineName = d.Medicine?.Name ?? string.Empty,
            Dosage = d.Dosage,
            Frequency = d.Frequency,
            DurationDays = d.DurationDays,
            Quantity = d.Quantity,
            Instructions = d.Instructions
        }).ToList()
    };

    public static ReturnDto ToDto(this ReturnRecord record) => new()
    {
        Id = record.Id,
        ReturnNumber = record.ReturnNumber,
        Type = record.Type,
        BranchId = record.BranchId,
        BranchName = record.Branch?.Name ?? string.Empty,
        OriginalSaleId = record.OriginalSaleId,
        InvoiceNumber = record.OriginalSale?.InvoiceNumber,
        OriginalPurchaseOrderId = record.OriginalPurchaseOrderId,
        OrderNumber = record.OriginalPurchaseOrder?.OrderNumber,
        CustomerId = record.CustomerId,
        CustomerName = record.Customer?.FullName,
        SupplierId = record.SupplierId,
        SupplierName = record.Supplier?.Name,
        Reason = record.Reason,
        Status = record.Status,
        TotalAmount = record.TotalAmount,
        RefundMethod = record.RefundMethod,
        Notes = record.Notes,
        RequestedByUserId = record.RequestedByUserId,
        RequestedAtUtc = record.RequestedAtUtc,
        ApprovedByUserId = record.ApprovedByUserId,
        ApprovedAtUtc = record.ApprovedAtUtc,
        RejectionReason = record.RejectionReason,
        Details = record.Details.Select(d => new ReturnDetailDto
        {
            Id = d.Id,
            MedicineId = d.MedicineId,
            MedicineName = d.Medicine?.Name ?? string.Empty,
            BatchId = d.BatchId,
            BatchNumber = d.Batch?.BatchNumber ?? string.Empty,
            Quantity = d.Quantity,
            UnitPrice = d.UnitPrice,
            LineTotal = d.LineTotal,
            Condition = d.Condition,
            RestockDecision = d.RestockDecision,
            InspectedByUserId = d.InspectedByUserId,
            InspectedAtUtc = d.InspectedAtUtc,
            Notes = d.Notes
        }).ToList(),
        Refunds = record.Refunds.Select(r => new RefundDto
        {
            Id = r.Id,
            Amount = r.Amount,
            Method = r.Method,
            RefundedAtUtc = r.RefundedAtUtc,
            ProcessedByUserId = r.ProcessedByUserId,
            Notes = r.Notes
        }).ToList()
    };
}
