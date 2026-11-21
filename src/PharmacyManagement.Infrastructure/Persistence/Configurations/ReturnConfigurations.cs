using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacyManagement.Domain.Entities;

namespace PharmacyManagement.Infrastructure.Persistence.Configurations;

public sealed class ReturnRecordConfiguration : IEntityTypeConfiguration<ReturnRecord>
{
    public void Configure(EntityTypeBuilder<ReturnRecord> builder)
    {
        builder.ToTable("Returns");
        builder.Property(r => r.ReturnNumber).HasMaxLength(50).IsRequired();
        builder.Property(r => r.Reason).HasMaxLength(1000).IsRequired();
        builder.Property(r => r.Notes).HasMaxLength(1000);
        builder.Property(r => r.RejectionReason).HasMaxLength(1000);
        builder.HasIndex(r => r.ReturnNumber).IsUnique();
        builder.HasIndex(r => r.RequestedAtUtc);
        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.Type);
        builder.HasOne(r => r.Branch)
            .WithMany()
            .HasForeignKey(r => r.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(r => r.OriginalSale)
            .WithMany(s => s.Returns)
            .HasForeignKey(r => r.OriginalSaleId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(r => r.OriginalPurchaseOrder)
            .WithMany()
            .HasForeignKey(r => r.OriginalPurchaseOrderId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(r => r.Customer)
            .WithMany()
            .HasForeignKey(r => r.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(r => r.Supplier)
            .WithMany()
            .HasForeignKey(r => r.SupplierId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public sealed class ReturnDetailConfiguration : IEntityTypeConfiguration<ReturnDetail>
{
    public void Configure(EntityTypeBuilder<ReturnDetail> builder)
    {
        builder.ToTable("ReturnDetails");
        builder.Property(d => d.Notes).HasMaxLength(500);
        builder.HasIndex(d => d.ReturnId);
        builder.HasOne(d => d.Return)
            .WithMany(r => r.Details)
            .HasForeignKey(d => d.ReturnId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(d => d.Medicine)
            .WithMany()
            .HasForeignKey(d => d.MedicineId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(d => d.Batch)
            .WithMany()
            .HasForeignKey(d => d.BatchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class RefundConfiguration : IEntityTypeConfiguration<Refund>
{
    public void Configure(EntityTypeBuilder<Refund> builder)
    {
        builder.ToTable("Refunds");
        builder.Property(r => r.Notes).HasMaxLength(500);
        builder.HasIndex(r => r.ReturnId);
        builder.HasIndex(r => r.RefundedAtUtc);
        builder.HasOne(r => r.Return)
            .WithMany(rr => rr.Refunds)
            .HasForeignKey(r => r.ReturnId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
