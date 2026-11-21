using PharmacyManagement.Application.DTOs.Customers;
using PharmacyManagement.Application.DTOs.Sales;
using PharmacyManagement.Application.DTOs.Suppliers;
using PharmacyManagement.Domain.Entities;

namespace PharmacyManagement.Application.Mappings;

public static class PartnerMappings
{
    public static SupplierDto ToDto(this Supplier supplier) => new()
    {
        Id = supplier.Id,
        Name = supplier.Name,
        CompanyName = supplier.CompanyName,
        ContactPerson = supplier.ContactPerson,
        Phone = supplier.Phone,
        Email = supplier.Email,
        Address = supplier.Address,
        TaxRegistrationNumber = supplier.TaxRegistrationNumber,
        OpeningBalance = supplier.OpeningBalance,
        PaymentTerms = supplier.PaymentTerms,
        IsActive = supplier.IsActive
    };

    public static SupplierPaymentDto ToDto(this SupplierPayment payment) => new()
    {
        Id = payment.Id,
        SupplierId = payment.SupplierId,
        SupplierName = payment.Supplier?.Name ?? string.Empty,
        PurchaseOrderId = payment.PurchaseOrderId,
        OrderNumber = payment.PurchaseOrder?.OrderNumber,
        Amount = payment.Amount,
        PaymentDateUtc = payment.PaymentDateUtc,
        Method = payment.Method,
        Reference = payment.Reference,
        Notes = payment.Notes,
        RecordedByUserId = payment.RecordedByUserId,
        CreatedAtUtc = payment.CreatedAtUtc
    };

    public static CustomerDto ToDto(this Customer customer) => new()
    {
        Id = customer.Id,
        Code = customer.Code,
        FirstName = customer.FirstName,
        LastName = customer.LastName,
        Phone = customer.Phone,
        Email = customer.Email,
        Address = customer.Address,
        DateOfBirth = customer.DateOfBirth,
        Gender = customer.Gender,
        LoyaltyPoints = customer.LoyaltyPoints,
        CreditLimit = customer.CreditLimit,
        IsActive = customer.IsActive
    };

    public static CustomerPaymentDto ToDto(this CustomerPayment payment) => new()
    {
        Id = payment.Id,
        CustomerId = payment.CustomerId,
        CustomerName = payment.Customer?.FullName ?? string.Empty,
        SaleId = payment.SaleId,
        InvoiceNumber = payment.Sale?.InvoiceNumber,
        Amount = payment.Amount,
        PaymentDateUtc = payment.PaymentDateUtc,
        Method = payment.Method,
        Reference = payment.Reference,
        Notes = payment.Notes,
        RecordedByUserId = payment.RecordedByUserId,
        CreatedAtUtc = payment.CreatedAtUtc
    };
}
