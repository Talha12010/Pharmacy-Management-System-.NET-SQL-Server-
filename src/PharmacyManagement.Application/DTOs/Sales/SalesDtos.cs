using PharmacyManagement.Domain.Enums;

namespace PharmacyManagement.Application.DTOs.Sales;

public sealed class SaleDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public int? PrescriptionId { get; set; }
    public string? PrescriptionNumber { get; set; }
    public Guid CashierUserId { get; set; }
    public string? CashierName { get; set; }
    public DateTime SaleDateUtc { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal ChangeAmount { get; set; }
    public SaleStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public string? Notes { get; set; }
    public DateTime? CancelledAtUtc { get; set; }
    public string? CancellationReason { get; set; }
    public IReadOnlyList<SaleDetailDto> Details { get; set; } = Array.Empty<SaleDetailDto>();
    public IReadOnlyList<SalePaymentDto> Payments { get; set; } = Array.Empty<SalePaymentDto>();
}

public sealed class SaleListItemDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime SaleDateUtc { get; set; }
    public string? CustomerName { get; set; }
    public string? CashierName { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public SaleStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public int ItemCount { get; set; }
}

public sealed class SaleDetailDto
{
    public int Id { get; set; }
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public int BatchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int ReturnedQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal UnitCost { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxPercentage { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }
}

public sealed class SalePaymentDto
{
    public int Id { get; set; }
    public PaymentMethod Method { get; set; }
    public decimal Amount { get; set; }
    public string? Reference { get; set; }
    public DateTime PaidAtUtc { get; set; }
}

public sealed class CreateSaleRequest
{
    public int BranchId { get; set; }
    public int? CustomerId { get; set; }
    public int? PrescriptionId { get; set; }
    public decimal DiscountAmount { get; set; }
    public string? Notes { get; set; }
    public List<CreateSaleItemRequest> Items { get; set; } = new();
    public List<CreateSalePaymentRequest> Payments { get; set; } = new();
}

public sealed class CreateSaleItemRequest
{
    public int MedicineId { get; set; }
    public int? BatchId { get; set; }
    public int Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
}

public sealed class CreateSalePaymentRequest
{
    public PaymentMethod Method { get; set; }
    public decimal Amount { get; set; }
    public string? Reference { get; set; }
}

public sealed class CancelSaleRequest
{
    public string Reason { get; set; } = string.Empty;
}

public sealed class RecordSalePaymentRequest
{
    public PaymentMethod Method { get; set; }
    public decimal Amount { get; set; }
    public string? Reference { get; set; }
}

public sealed class SaleInvoiceDto
{
    public SaleDto Sale { get; set; } = new();
    public string BranchAddress { get; set; } = string.Empty;
    public string? BranchPhone { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerAddress { get; set; }
}

public sealed class TodaySalesSummaryDto
{
    public DateTime DateUtc { get; set; }
    public int InvoiceCount { get; set; }
    public decimal TotalSales { get; set; }
    public decimal TotalTax { get; set; }
    public decimal TotalDiscount { get; set; }
    public IReadOnlyList<PaymentBreakdownDto> PaymentBreakdown { get; set; } = Array.Empty<PaymentBreakdownDto>();
}

public sealed class PaymentBreakdownDto
{
    public PaymentMethod Method { get; set; }
    public int Count { get; set; }
    public decimal Amount { get; set; }
}
