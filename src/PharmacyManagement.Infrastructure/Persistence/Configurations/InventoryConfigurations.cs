using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacyManagement.Domain.Entities;

namespace PharmacyManagement.Infrastructure.Persistence.Configurations;

public sealed class MedicineBatchConfiguration : IEntityTypeConfiguration<MedicineBatch>
{
    public void Configure(EntityTypeBuilder<MedicineBatch> builder)
    {
        builder.ToTable("MedicineBatches");
        builder.Property(b => b.BatchNumber).HasMaxLength(50).IsRequired();
        builder.Property(b => b.RowVersion).IsRowVersion();
        builder.HasIndex(b => new { b.MedicineId, b.BranchId, b.BatchNumber }).IsUnique();
        builder.HasIndex(b => b.ExpiryDate);
        builder.HasIndex(b => b.BranchId);
        builder.HasIndex(b => new { b.BranchId, b.MedicineId, b.ExpiryDate });
        builder.HasOne(b => b.Medicine)
            .WithMany(m => m.Batches)
            .HasForeignKey(b => b.MedicineId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(b => b.Branch)
            .WithMany(br => br.Batches)
            .HasForeignKey(b => b.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(b => b.Supplier)
            .WithMany(s => s.Batches)
            .HasForeignKey(b => b.SupplierId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public sealed class InventoryTransactionConfiguration : IEntityTypeConfiguration<InventoryTransaction>
{
    public void Configure(EntityTypeBuilder<InventoryTransaction> builder)
    {
        builder.ToTable("InventoryTransactions");
        builder.Property(t => t.ReferenceType).HasMaxLength(50);
        builder.Property(t => t.Notes).HasMaxLength(500);
        builder.HasIndex(t => new { t.MedicineId, t.CreatedAtUtc });
        builder.HasIndex(t => t.BranchId);
        builder.HasIndex(t => t.BatchId);
        builder.HasIndex(t => t.CreatedAtUtc);
        builder.HasOne(t => t.Medicine)
            .WithMany(m => m.InventoryTransactions)
            .HasForeignKey(t => t.MedicineId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(t => t.Batch)
            .WithMany(b => b.Transactions)
            .HasForeignKey(t => t.BatchId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(t => t.Branch)
            .WithMany()
            .HasForeignKey(t => t.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class StockTransferConfiguration : IEntityTypeConfiguration<StockTransfer>
{
    public void Configure(EntityTypeBuilder<StockTransfer> builder)
    {
        builder.ToTable("StockTransfers");
        builder.Property(t => t.TransferNumber).HasMaxLength(50).IsRequired();
        builder.Property(t => t.Notes).HasMaxLength(1000);
        builder.HasIndex(t => t.TransferNumber).IsUnique();
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.TransferDateUtc);
        builder.HasOne(t => t.FromBranch)
            .WithMany()
            .HasForeignKey(t => t.FromBranchId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(t => t.ToBranch)
            .WithMany()
            .HasForeignKey(t => t.ToBranchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class StockTransferDetailConfiguration : IEntityTypeConfiguration<StockTransferDetail>
{
    public void Configure(EntityTypeBuilder<StockTransferDetail> builder)
    {
        builder.ToTable("StockTransferDetails");
        builder.HasOne(d => d.StockTransfer)
            .WithMany(t => t.Details)
            .HasForeignKey(d => d.StockTransferId)
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
