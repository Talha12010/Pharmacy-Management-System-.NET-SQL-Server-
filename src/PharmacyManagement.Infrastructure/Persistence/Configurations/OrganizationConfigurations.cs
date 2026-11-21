using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacyManagement.Domain.Entities;

namespace PharmacyManagement.Infrastructure.Persistence.Configurations;

public sealed class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("Branches");
        builder.Property(b => b.Name).HasMaxLength(200).IsRequired();
        builder.Property(b => b.Code).HasMaxLength(50).IsRequired();
        builder.Property(b => b.Address).HasMaxLength(500);
        builder.Property(b => b.Phone).HasMaxLength(30);
        builder.Property(b => b.Email).HasMaxLength(256);
        builder.HasIndex(b => b.Code).IsUnique();
    }
}

public sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");
        builder.Ignore(e => e.FullName);
        builder.Property(e => e.EmployeeCode).HasMaxLength(50).IsRequired();
        builder.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.LastName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Phone).HasMaxLength(30);
        builder.Property(e => e.Email).HasMaxLength(256);
        builder.Property(e => e.Address).HasMaxLength(500);
        builder.Property(e => e.Position).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Notes).HasMaxLength(1000);
        builder.HasIndex(e => e.EmployeeCode).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(e => e.BranchId);
        builder.HasIndex(e => e.UserId);
        builder.HasOne(e => e.Branch)
            .WithMany(b => b.Employees)
            .HasForeignKey(e => e.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public sealed class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
{
    public void Configure(EntityTypeBuilder<SystemSetting> builder)
    {
        builder.ToTable("SystemSettings");
        builder.Property(s => s.Key).HasMaxLength(100).IsRequired();
        builder.Property(s => s.Value).HasMaxLength(2000).IsRequired();
        builder.Property(s => s.Description).HasMaxLength(500);
        builder.Property(s => s.Group).HasMaxLength(100).IsRequired();
        builder.HasIndex(s => s.Key).IsUnique();
    }
}

public sealed class DocumentSequenceConfiguration : IEntityTypeConfiguration<DocumentSequence>
{
    public void Configure(EntityTypeBuilder<DocumentSequence> builder)
    {
        builder.ToTable("DocumentSequences");
        builder.Property(s => s.DocumentType).HasMaxLength(50).IsRequired();
        builder.Property(s => s.PeriodKey).HasMaxLength(20).IsRequired();
        builder.Property(s => s.RowVersion).IsRowVersion();
        builder.HasIndex(s => new { s.DocumentType, s.PeriodKey }).IsUnique();
    }
}

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.Property(l => l.UserName).HasMaxLength(256);
        builder.Property(l => l.Action).HasMaxLength(100).IsRequired();
        builder.Property(l => l.EntityName).HasMaxLength(100).IsRequired();
        builder.Property(l => l.EntityId).HasMaxLength(100);
        builder.Property(l => l.IpAddress).HasMaxLength(64);
        builder.Property(l => l.UserAgent).HasMaxLength(500);
        builder.HasIndex(l => l.CreatedAtUtc);
        builder.HasIndex(l => l.UserId);
        builder.HasIndex(l => new { l.EntityName, l.EntityId });
    }
}
