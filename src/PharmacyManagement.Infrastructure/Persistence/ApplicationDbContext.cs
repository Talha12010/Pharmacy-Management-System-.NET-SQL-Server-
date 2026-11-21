using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common.Interfaces;
using PharmacyManagement.Domain.Common;
using PharmacyManagement.Domain.Entities;
using PharmacyManagement.Domain.Interfaces;

namespace PharmacyManagement.Infrastructure.Persistence;

public sealed class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>, IApplicationDbContext
{
    private readonly ICurrentUserService? _currentUser;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICurrentUserService? currentUser = null)
        : base(options)
    {
        _currentUser = currentUser;
    }

    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
    public DbSet<DocumentSequence> DocumentSequences => Set<DocumentSequence>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<MedicineCategory> MedicineCategories => Set<MedicineCategory>();
    public DbSet<Manufacturer> Manufacturers => Set<Manufacturer>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<Medicine> Medicines => Set<Medicine>();

    public DbSet<MedicineBatch> MedicineBatches => Set<MedicineBatch>();
    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();
    public DbSet<StockTransfer> StockTransfers => Set<StockTransfer>();
    public DbSet<StockTransferDetail> StockTransferDetails => Set<StockTransferDetail>();

    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderDetail> PurchaseOrderDetails => Set<PurchaseOrderDetail>();
    public DbSet<GoodsReceipt> GoodsReceipts => Set<GoodsReceipt>();
    public DbSet<GoodsReceiptDetail> GoodsReceiptDetails => Set<GoodsReceiptDetail>();
    public DbSet<SupplierPayment> SupplierPayments => Set<SupplierPayment>();

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleDetail> SaleDetails => Set<SaleDetail>();
    public DbSet<SalePayment> SalePayments => Set<SalePayment>();
    public DbSet<CustomerPayment> CustomerPayments => Set<CustomerPayment>();

    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<PrescriptionDetail> PrescriptionDetails => Set<PrescriptionDetail>();

    public DbSet<ReturnRecord> Returns => Set<ReturnRecord>();
    public DbSet<ReturnDetail> ReturnDetails => Set<ReturnDetail>();
    public DbSet<Refund> Refunds => Set<Refund>();

    public DbSet<ExpenseCategory> ExpenseCategories => Set<ExpenseCategory>();
    public DbSet<Expense> Expenses => Set<Expense>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(BuildSoftDeleteFilter(entityType.ClrType));
            }
        }
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<decimal>().HavePrecision(18, 2);
        configurationBuilder.Properties<string>().HaveMaxLength(500);
        base.ConfigureConventions(configurationBuilder);
    }

    public override int SaveChanges()
    {
        ApplyAuditAndSoftDelete();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyAuditAndSoftDelete();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditAndSoftDelete();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ApplyAuditAndSoftDelete();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ApplyAuditAndSoftDelete()
    {
        var now = DateTime.UtcNow;
        var userId = _currentUser?.UserId;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is AuditableEntity auditable)
            {
                if (entry.State == EntityState.Added)
                {
                    if (auditable.CreatedAtUtc == default)
                    {
                        auditable.CreatedAtUtc = now;
                    }

                    auditable.CreatedByUserId ??= userId;
                }
                else if (entry.State == EntityState.Modified)
                {
                    auditable.UpdatedAtUtc = now;
                    auditable.UpdatedByUserId = userId;
                }
            }

            if (entry.Entity is ISoftDeletable softDeletable)
            {
                if (entry.State == EntityState.Deleted)
                {
                    entry.State = EntityState.Modified;
                    softDeletable.IsDeleted = true;
                    softDeletable.DeletedAtUtc = now;
                    softDeletable.DeletedByUserId = userId;
                }
                else if (softDeletable.IsDeleted && softDeletable.DeletedAtUtc is null)
                {
                    softDeletable.DeletedAtUtc = now;
                    softDeletable.DeletedByUserId = userId;
                }
            }
        }
    }

    private static LambdaExpression BuildSoftDeleteFilter(Type entityType)
    {
        var parameter = Expression.Parameter(entityType, "e");
        var property = Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
        var body = Expression.Equal(property, Expression.Constant(false));
        return Expression.Lambda(body, parameter);
    }
}
