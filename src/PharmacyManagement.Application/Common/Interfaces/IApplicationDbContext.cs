using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using PharmacyManagement.Domain.Entities;

namespace PharmacyManagement.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<ApplicationUser> Users { get; }
    DbSet<ApplicationRole> Roles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<RefreshToken> RefreshTokens { get; }

    DbSet<Branch> Branches { get; }
    DbSet<Employee> Employees { get; }
    DbSet<SystemSetting> SystemSettings { get; }
    DbSet<DocumentSequence> DocumentSequences { get; }
    DbSet<AuditLog> AuditLogs { get; }

    DbSet<MedicineCategory> MedicineCategories { get; }
    DbSet<Manufacturer> Manufacturers { get; }
    DbSet<Unit> Units { get; }
    DbSet<Medicine> Medicines { get; }

    DbSet<MedicineBatch> MedicineBatches { get; }
    DbSet<InventoryTransaction> InventoryTransactions { get; }
    DbSet<StockTransfer> StockTransfers { get; }
    DbSet<StockTransferDetail> StockTransferDetails { get; }

    DbSet<Supplier> Suppliers { get; }
    DbSet<PurchaseOrder> PurchaseOrders { get; }
    DbSet<PurchaseOrderDetail> PurchaseOrderDetails { get; }
    DbSet<GoodsReceipt> GoodsReceipts { get; }
    DbSet<GoodsReceiptDetail> GoodsReceiptDetails { get; }
    DbSet<SupplierPayment> SupplierPayments { get; }

    DbSet<Customer> Customers { get; }
    DbSet<Sale> Sales { get; }
    DbSet<SaleDetail> SaleDetails { get; }
    DbSet<SalePayment> SalePayments { get; }
    DbSet<CustomerPayment> CustomerPayments { get; }

    DbSet<Prescription> Prescriptions { get; }
    DbSet<PrescriptionDetail> PrescriptionDetails { get; }

    DbSet<ReturnRecord> Returns { get; }
    DbSet<ReturnDetail> ReturnDetails { get; }
    DbSet<Refund> Refunds { get; }

    DbSet<ExpenseCategory> ExpenseCategories { get; }
    DbSet<Expense> Expenses { get; }

    DatabaseFacade Database { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
