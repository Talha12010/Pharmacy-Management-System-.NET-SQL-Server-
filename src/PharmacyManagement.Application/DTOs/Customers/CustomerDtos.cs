using PharmacyManagement.Domain.Enums;

namespace PharmacyManagement.Application.DTOs.Customers;

public sealed class CustomerDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public Gender? Gender { get; set; }
    public int LoyaltyPoints { get; set; }
    public decimal CreditLimit { get; set; }
    public bool IsActive { get; set; }
    public decimal OutstandingBalance { get; set; }
    public decimal TotalPurchases { get; set; }
    public decimal TotalPaid { get; set; }
    public int InvoiceCount { get; set; }
}

public class CreateCustomerRequest
{
    public string Code { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public Gender? Gender { get; set; }
    public decimal CreditLimit { get; set; }
}

public sealed class UpdateCustomerRequest : CreateCustomerRequest
{
    public bool IsActive { get; set; } = true;
    public int LoyaltyPoints { get; set; }
}

public sealed class CustomerPaymentDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int? SaleId { get; set; }
    public string? InvoiceNumber { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDateUtc { get; set; }
    public PaymentMethod Method { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public Guid RecordedByUserId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public sealed class RecordCustomerPaymentRequest
{
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public int? SaleId { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public DateTime? PaymentDateUtc { get; set; }
}
