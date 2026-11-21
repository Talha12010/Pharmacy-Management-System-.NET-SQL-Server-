using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacyManagement.Domain.Entities;

namespace PharmacyManagement.Infrastructure.Persistence.Configurations;

public sealed class MedicineCategoryConfiguration : IEntityTypeConfiguration<MedicineCategory>
{
    public void Configure(EntityTypeBuilder<MedicineCategory> builder)
    {
        builder.ToTable("MedicineCategories");
        builder.Property(c => c.Name).HasMaxLength(150).IsRequired();
        builder.Property(c => c.Description).HasMaxLength(500);
        builder.HasIndex(c => c.Name).IsUnique();
    }
}

public sealed class ManufacturerConfiguration : IEntityTypeConfiguration<Manufacturer>
{
    public void Configure(EntityTypeBuilder<Manufacturer> builder)
    {
        builder.ToTable("Manufacturers");
        builder.Property(m => m.Name).HasMaxLength(200).IsRequired();
        builder.Property(m => m.Country).HasMaxLength(100);
        builder.Property(m => m.ContactInfo).HasMaxLength(500);
        builder.HasIndex(m => m.Name).IsUnique();
    }
}

public sealed class UnitConfiguration : IEntityTypeConfiguration<Unit>
{
    public void Configure(EntityTypeBuilder<Unit> builder)
    {
        builder.ToTable("Units");
        builder.Property(u => u.Name).HasMaxLength(100).IsRequired();
        builder.Property(u => u.Abbreviation).HasMaxLength(20).IsRequired();
        builder.HasIndex(u => u.Name).IsUnique();
        builder.HasIndex(u => u.Abbreviation).IsUnique();
    }
}

public sealed class MedicineConfiguration : IEntityTypeConfiguration<Medicine>
{
    public void Configure(EntityTypeBuilder<Medicine> builder)
    {
        builder.ToTable("Medicines");
        builder.Property(m => m.Name).HasMaxLength(200).IsRequired();
        builder.Property(m => m.GenericName).HasMaxLength(200);
        builder.Property(m => m.BrandName).HasMaxLength(200);
        builder.Property(m => m.Barcode).HasMaxLength(100);
        builder.Property(m => m.Sku).HasMaxLength(50).IsRequired();
        builder.Property(m => m.Description).HasMaxLength(1000);
        builder.Property(m => m.DosageForm).HasMaxLength(100).IsRequired();
        builder.Property(m => m.Strength).HasMaxLength(100).IsRequired();
        builder.HasIndex(m => m.Sku).IsUnique();
        builder.HasIndex(m => m.Barcode).IsUnique().HasFilter("[Barcode] IS NOT NULL AND [IsDeleted] = 0");
        builder.HasIndex(m => m.Name);
        builder.HasIndex(m => m.GenericName);
        builder.HasIndex(m => m.CategoryId);
        builder.HasIndex(m => m.ManufacturerId);
        builder.HasIndex(m => m.IsActive);
        builder.HasOne(m => m.Category)
            .WithMany(c => c.Medicines)
            .HasForeignKey(m => m.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.Manufacturer)
            .WithMany(mf => mf.Medicines)
            .HasForeignKey(m => m.ManufacturerId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.Unit)
            .WithMany(u => u.Medicines)
            .HasForeignKey(m => m.UnitId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
