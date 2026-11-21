using PharmacyManagement.Domain.Enums;

namespace PharmacyManagement.Application.DTOs.Prescriptions;

public sealed class PrescriptionDto
{
    public int Id { get; set; }
    public string PrescriptionNumber { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string? PatientPhone { get; set; }
    public string? PatientAddress { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string? DoctorLicenseNumber { get; set; }
    public DateTime PrescriptionDate { get; set; }
    public PrescriptionStatus Status { get; set; }
    public string? ImagePath { get; set; }
    public string? Notes { get; set; }
    public string? RejectionReason { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Guid? VerifiedByUserId { get; set; }
    public DateTime? VerifiedAtUtc { get; set; }
    public IReadOnlyList<PrescriptionDetailDto> Details { get; set; } = Array.Empty<PrescriptionDetailDto>();
}

public sealed class PrescriptionDetailDto
{
    public int Id { get; set; }
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string? Frequency { get; set; }
    public int? DurationDays { get; set; }
    public int Quantity { get; set; }
    public string? Instructions { get; set; }
}

public sealed class CreatePrescriptionRequest
{
    public int BranchId { get; set; }
    public int? CustomerId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string? PatientPhone { get; set; }
    public string? PatientAddress { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string? DoctorLicenseNumber { get; set; }
    public DateTime PrescriptionDate { get; set; }
    public string? Notes { get; set; }
    public List<PrescriptionItemRequest> Details { get; set; } = new();
}

public sealed class PrescriptionItemRequest
{
    public int MedicineId { get; set; }
    public string Dosage { get; set; } = string.Empty;
    public string? Frequency { get; set; }
    public int? DurationDays { get; set; }
    public int Quantity { get; set; }
    public string? Instructions { get; set; }
}

public sealed class RejectPrescriptionRequest
{
    public string Reason { get; set; } = string.Empty;
}
