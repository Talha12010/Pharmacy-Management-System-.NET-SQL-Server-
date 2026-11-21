using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacyManagement.Domain.Entities;

namespace PharmacyManagement.Infrastructure.Persistence.Configurations;

public sealed class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("Suppliers");
        builder.Property(s => s.Name).HasMaxLength(200).IsRequired();
        builder.Property(s => s.CompanyName).HasMaxLength(200);
        builder.Property(s => s.ContactPerson).HasMaxLength(100);
        builder.Property(s => s.Phone).HasMaxLength(30);
        builder.Property(s => s.Email).HasMaxLength(256);
        builder.Property(s => s.Address).HasMaxLength(500);
        builder.Property(s => s.TaxRegistrationNumber).HasMaxLength(100);
        builder.Property(s => s.PaymentTerms).HasMaxLength(200);
        builder.HasIndex(s => s.Name).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(s => s.Phone);
    }
}

public sealed class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("PurchaseOrders");
        builder.Property(o => o.OrderNumber).HasMaxLength(50).IsRequired();
        builder.Property(o => o.Notes).HasMaxLength(1000);
        builder.Property(o => o.RejectionReason).HasMaxLength(1000);
        builder.HasIndex(o => o.OrderNumber).IsUnique();
        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.OrderDateUtc);
        builder.HasIndex(o => o.SupplierId);
        builder.HasIndex(o => o.BranchId);
        builder.HasOne(o => o.Supplier)
            .WithMany(s => s.PurchaseOrders)
            .HasForeignKey(o => o.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(o => o.Branch)
            .WithMany(b => b.PurchaseOrders)
            .HasForeignKey(o => o.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class PurchaseOrderDetailConfiguration : IEntityTypeConfiguration<PurchaseOrderDetail>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderDetail> builder)
    {
        builder.ToTable("PurchaseOrderDetails");
        builder.Property(d => d.BatchNumber).HasMaxLength(50);
        builder.HasOne(d => d.PurchaseOrder)
            .WithMany(o => o.Details)
            .HasForeignKey(d => d.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(d => d.Medicine)
            .WithMany()
            .HasForeignKey(d => d.MedicineId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class GoodsReceiptConfiguration : IEntityTypeConfiguration<GoodsReceipt>
{
    public void Configure(EntityTypeBuilder<GoodsReceipt> builder)
    {
        builder.ToTable("GoodsReceipts");
        builder.Property(r => r.ReceiptNumber).HasMaxLength(50).IsRequired();
        builder.Property(r => r.Notes).HasMaxLength(1000);
        builder.HasIndex(r => r.ReceiptNumber).IsUnique();
        builder.HasIndex(r => r.ReceivedAtUtc);
        builder.HasOne(r => r.PurchaseOrder)
            .WithMany(o => o.GoodsReceipts)
            .HasForeignKey(r => r.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class GoodsReceiptDetailConfiguration : IEntityTypeConfiguration<GoodsReceiptDetail>
{
    public void Configure(EntityTypeBuilder<GoodsReceiptDetail> builder)
    {
        builder.ToTable("GoodsReceiptDetails");
        builder.Property(d => d.BatchNumber).HasMaxLength(50).IsRequired();
        builder.HasOne(d => d.GoodsReceipt)
            .WithMany(r => r.Details)
            .HasForeignKey(d => d.GoodsReceiptId)
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

public sealed class SupplierPaymentConfiguration : IEntityTypeConfiguration<SupplierPayment>
{
    public void Configure(EntityTypeBuilder<SupplierPayment> builder)
    {
        builder.ToTable("SupplierPayments");
        builder.Property(p => p.Reference).HasMaxLength(100);
        builder.Property(p => p.Notes).HasMaxLength(500);
        builder.HasIndex(p => p.PaymentDateUtc);
        builder.HasIndex(p => p.SupplierId);
        builder.HasOne(p => p.Supplier)
            .WithMany(s => s.Payments)
            .HasForeignKey(p => p.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.PurchaseOrder)
            .WithMany()
            .HasForeignKey(p => p.PurchaseOrderId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
