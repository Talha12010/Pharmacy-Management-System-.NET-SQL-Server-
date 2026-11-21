using PharmacyManagement.Domain.Enums;

namespace PharmacyManagement.Domain.Entities;

public class Prescription
{
    public int Id { get; set; }
    public string PrescriptionNumber { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public Branch Branch { get; set; } = null!;
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string? PatientPhone { get; set; }
    public string? PatientAddress { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string? DoctorLicenseNumber { get; set; }
    public DateTime PrescriptionDate { get; set; }
    public PrescriptionStatus Status { get; set; } = PrescriptionStatus.Pending;
    public string? ImagePath { get; set; }
    public string? Notes { get; set; }
    public string? RejectionReason { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Guid? VerifiedByUserId { get; set; }
    public DateTime? VerifiedAtUtc { get; set; }

    public ICollection<PrescriptionDetail> Details { get; set; } = new List<PrescriptionDetail>();
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
}

public class PrescriptionDetail
{
    public int Id { get; set; }
    public int PrescriptionId { get; set; }
    public Prescription Prescription { get; set; } = null!;
    public int MedicineId { get; set; }
    public Medicine Medicine { get; set; } = null!;
    public string Dosage { get; set; } = string.Empty;
    public string? Frequency { get; set; }
    public int? DurationDays { get; set; }
    public int Quantity { get; set; }
    public string? Instructions { get; set; }
}
