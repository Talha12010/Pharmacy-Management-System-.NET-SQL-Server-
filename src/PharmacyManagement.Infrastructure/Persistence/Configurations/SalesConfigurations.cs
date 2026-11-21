using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacyManagement.Domain.Entities;

namespace PharmacyManagement.Infrastructure.Persistence.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.Ignore(c => c.FullName);
        builder.Property(c => c.Code).HasMaxLength(50).IsRequired();
        builder.Property(c => c.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(c => c.LastName).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Phone).HasMaxLength(30);
        builder.Property(c => c.Email).HasMaxLength(256);
        builder.Property(c => c.Address).HasMaxLength(500);
        builder.HasIndex(c => c.Code).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(c => c.Phone);
        builder.HasIndex(c => c.Email);
    }
}

public sealed class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("Sales");
        builder.Property(s => s.InvoiceNumber).HasMaxLength(50).IsRequired();
        builder.Property(s => s.Notes).HasMaxLength(1000);
        builder.Property(s => s.CancellationReason).HasMaxLength(1000);
        builder.HasIndex(s => s.InvoiceNumber).IsUnique();
        builder.HasIndex(s => s.SaleDateUtc);
        builder.HasIndex(s => s.BranchId);
        builder.HasIndex(s => s.CustomerId);
        builder.HasIndex(s => s.PrescriptionId);
        builder.HasIndex(s => s.Status);
        builder.HasOne(s => s.Branch)
            .WithMany(b => b.Sales)
            .HasForeignKey(s => s.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(s => s.Customer)
            .WithMany(c => c.Sales)
            .HasForeignKey(s => s.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(s => s.Prescription)
            .WithMany(p => p.Sales)
            .HasForeignKey(s => s.PrescriptionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public sealed class SaleDetailConfiguration : IEntityTypeConfiguration<SaleDetail>
{
    public void Configure(EntityTypeBuilder<SaleDetail> builder)
    {
        builder.ToTable("SaleDetails");
        builder.Property(d => d.BatchNumber).HasMaxLength(50).IsRequired();
        builder.HasIndex(d => d.SaleId);
        builder.HasIndex(d => d.MedicineId);
        builder.HasIndex(d => d.BatchId);
        builder.HasOne(d => d.Sale)
            .WithMany(s => s.Details)
            .HasForeignKey(d => d.SaleId)
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

public sealed class SalePaymentConfiguration : IEntityTypeConfiguration<SalePayment>
{
    public void Configure(EntityTypeBuilder<SalePayment> builder)
    {
        builder.ToTable("SalePayments");
        builder.Property(p => p.Reference).HasMaxLength(100);
        builder.HasIndex(p => p.SaleId);
        builder.HasIndex(p => p.PaidAtUtc);
        builder.HasOne(p => p.Sale)
            .WithMany(s => s.Payments)
            .HasForeignKey(p => p.SaleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class CustomerPaymentConfiguration : IEntityTypeConfiguration<CustomerPayment>
{
    public void Configure(EntityTypeBuilder<CustomerPayment> builder)
    {
        builder.ToTable("CustomerPayments");
        builder.Property(p => p.Reference).HasMaxLength(100);
        builder.Property(p => p.Notes).HasMaxLength(500);
        builder.HasIndex(p => p.PaymentDateUtc);
        builder.HasIndex(p => p.CustomerId);
        builder.HasOne(p => p.Customer)
            .WithMany(c => c.Payments)
            .HasForeignKey(p => p.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.Sale)
            .WithMany()
            .HasForeignKey(p => p.SaleId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
