using PharmacyManagement.Domain.Enums;

namespace PharmacyManagement.Application.DTOs.Suppliers;

public sealed class SupplierDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? TaxRegistrationNumber { get; set; }
    public decimal OpeningBalance { get; set; }
    public string? PaymentTerms { get; set; }
    public bool IsActive { get; set; }
    public decimal OutstandingBalance { get; set; }
    public decimal TotalPurchases { get; set; }
    public decimal TotalPaid { get; set; }
    public int PurchaseCount { get; set; }
}

public class CreateSupplierRequest
{
    public string Name { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? TaxRegistrationNumber { get; set; }
    public decimal OpeningBalance { get; set; }
    public string? PaymentTerms { get; set; }
}

public sealed class UpdateSupplierRequest : CreateSupplierRequest
{
    public bool IsActive { get; set; } = true;
}

public sealed class SupplierPaymentDto
{
    public int Id { get; set; }
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public int? PurchaseOrderId { get; set; }
    public string? OrderNumber { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDateUtc { get; set; }
    public PaymentMethod Method { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public Guid RecordedByUserId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public sealed class RecordSupplierPaymentRequest
{
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public int? PurchaseOrderId { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public DateTime? PaymentDateUtc { get; set; }
}

public sealed class StatementDto
{
    public int PartyId { get; set; }
    public string PartyName { get; set; } = string.Empty;
    public decimal OpeningBalance { get; set; }
    public decimal ClosingBalance { get; set; }
    public IReadOnlyList<StatementLineDto> Lines { get; set; } = Array.Empty<StatementLineDto>();
}

public sealed class StatementLineDto
{
    public DateTime DateUtc { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal Balance { get; set; }
}
