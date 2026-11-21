using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacyManagement.Domain.Entities;

namespace PharmacyManagement.Infrastructure.Persistence.Configurations;

public sealed class PrescriptionConfiguration : IEntityTypeConfiguration<Prescription>
{
    public void Configure(EntityTypeBuilder<Prescription> builder)
    {
        builder.ToTable("Prescriptions");
        builder.Property(p => p.PrescriptionNumber).HasMaxLength(50).IsRequired();
        builder.Property(p => p.PatientName).HasMaxLength(200).IsRequired();
        builder.Property(p => p.PatientPhone).HasMaxLength(30);
        builder.Property(p => p.PatientAddress).HasMaxLength(500);
        builder.Property(p => p.DoctorName).HasMaxLength(200).IsRequired();
        builder.Property(p => p.DoctorLicenseNumber).HasMaxLength(100);
        builder.Property(p => p.ImagePath).HasMaxLength(500);
        builder.Property(p => p.Notes).HasMaxLength(1000);
        builder.Property(p => p.RejectionReason).HasMaxLength(1000);
        builder.HasIndex(p => p.PrescriptionNumber).IsUnique();
        builder.HasIndex(p => p.PrescriptionDate);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.CustomerId);
        builder.HasOne(p => p.Branch)
            .WithMany()
            .HasForeignKey(p => p.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.Customer)
            .WithMany(c => c.Prescriptions)
            .HasForeignKey(p => p.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public sealed class PrescriptionDetailConfiguration : IEntityTypeConfiguration<PrescriptionDetail>
{
    public void Configure(EntityTypeBuilder<PrescriptionDetail> builder)
    {
        builder.ToTable("PrescriptionDetails");
        builder.Property(d => d.Dosage).HasMaxLength(200).IsRequired();
        builder.Property(d => d.Frequency).HasMaxLength(100);
        builder.Property(d => d.Instructions).HasMaxLength(1000);
        builder.HasIndex(d => d.PrescriptionId);
        builder.HasOne(d => d.Prescription)
            .WithMany(p => p.Details)
            .HasForeignKey(d => d.PrescriptionId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(d => d.Medicine)
            .WithMany()
            .HasForeignKey(d => d.MedicineId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
