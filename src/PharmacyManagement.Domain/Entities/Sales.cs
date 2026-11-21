using PharmacyManagement.Domain.Common;
using PharmacyManagement.Domain.Enums;

namespace PharmacyManagement.Domain.Entities;

public class Customer : SoftDeletableEntity
{
    public string Code { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public Gender? Gender { get; set; }
    public int LoyaltyPoints { get; set; }
    public decimal CreditLimit { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
    public ICollection<CustomerPayment> Payments { get; set; } = new List<CustomerPayment>();
    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();

    public string FullName => $"{FirstName} {LastName}".Trim();
}

public class Sale : BaseEntity
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public Branch Branch { get; set; } = null!;
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public int? PrescriptionId { get; set; }
    public Prescription? Prescription { get; set; }
    public Guid CashierUserId { get; set; }
    public DateTime SaleDateUtc { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal ChangeAmount { get; set; }
    public SaleStatus Status { get; set; } = SaleStatus.Completed;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Paid;
    public string? Notes { get; set; }
    public DateTime? CancelledAtUtc { get; set; }
    public Guid? CancelledByUserId { get; set; }
    public string? CancellationReason { get; set; }

    public ICollection<SaleDetail> Details { get; set; } = new List<SaleDetail>();
    public ICollection<SalePayment> Payments { get; set; } = new List<SalePayment>();
    public ICollection<ReturnRecord> Returns { get; set; } = new List<ReturnRecord>();
}

public class SaleDetail : BaseEntity
{
    public int SaleId { get; set; }
    public Sale Sale { get; set; } = null!;
    public int MedicineId { get; set; }
    public Medicine Medicine { get; set; } = null!;
    public int BatchId { get; set; }
    public MedicineBatch Batch { get; set; } = null!;
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

public class SalePayment : BaseEntity
{
    public int SaleId { get; set; }
    public Sale Sale { get; set; } = null!;
    public PaymentMethod Method { get; set; }
    public decimal Amount { get; set; }
    public string? Reference { get; set; }
    public DateTime PaidAtUtc { get; set; }
}

public class CustomerPayment : BaseEntity
{
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public int? SaleId { get; set; }
    public Sale? Sale { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDateUtc { get; set; }
    public PaymentMethod Method { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public Guid RecordedByUserId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
