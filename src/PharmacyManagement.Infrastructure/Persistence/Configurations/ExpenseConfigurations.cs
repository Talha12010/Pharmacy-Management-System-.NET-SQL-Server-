using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacyManagement.Domain.Entities;

namespace PharmacyManagement.Infrastructure.Persistence.Configurations;

public sealed class ExpenseCategoryConfiguration : IEntityTypeConfiguration<ExpenseCategory>
{
    public void Configure(EntityTypeBuilder<ExpenseCategory> builder)
    {
        builder.ToTable("ExpenseCategories");
        builder.Property(c => c.Name).HasMaxLength(150).IsRequired();
        builder.Property(c => c.Description).HasMaxLength(500);
        builder.HasIndex(c => c.Name).IsUnique();
    }
}

public sealed class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ToTable("Expenses");
        builder.Property(e => e.ExpenseNumber).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(1000).IsRequired();
        builder.Property(e => e.AttachmentPath).HasMaxLength(500);
        builder.HasIndex(e => e.ExpenseNumber).IsUnique();
        builder.HasIndex(e => e.ExpenseDateUtc);
        builder.HasIndex(e => e.BranchId);
        builder.HasIndex(e => e.ExpenseCategoryId);
        builder.HasOne(e => e.ExpenseCategory)
            .WithMany(c => c.Expenses)
            .HasForeignKey(e => e.ExpenseCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Branch)
            .WithMany(b => b.Expenses)
            .HasForeignKey(e => e.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
