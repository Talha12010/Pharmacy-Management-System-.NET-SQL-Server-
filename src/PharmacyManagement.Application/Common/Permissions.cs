namespace PharmacyManagement.Application.Common;

public static class Permissions
{
    public const string DashboardView = "Dashboard.View";

    public const string MedicinesView = "Medicines.View";
    public const string MedicinesManage = "Medicines.Manage";

    public const string InventoryView = "Inventory.View";
    public const string InventoryAdjust = "Inventory.Adjust";
    public const string InventoryTransfer = "Inventory.Transfer";

    public const string SalesView = "Sales.View";
    public const string SalesCreate = "Sales.Create";
    public const string SalesCancel = "Sales.Cancel";

    public const string PurchasesView = "Purchases.View";
    public const string PurchasesCreate = "Purchases.Create";
    public const string PurchasesApprove = "Purchases.Approve";
    public const string PurchasesReceive = "Purchases.Receive";

    public const string SuppliersView = "Suppliers.View";
    public const string SuppliersManage = "Suppliers.Manage";

    public const string CustomersView = "Customers.View";
    public const string CustomersManage = "Customers.Manage";

    public const string PrescriptionsView = "Prescriptions.View";
    public const string PrescriptionsCreate = "Prescriptions.Create";
    public const string PrescriptionsVerify = "Prescriptions.Verify";

    public const string ReturnsView = "Returns.View";
    public const string ReturnsCreate = "Returns.Create";
    public const string ReturnsApprove = "Returns.Approve";

    public const string ExpensesView = "Expenses.View";
    public const string ExpensesManage = "Expenses.Manage";

    public const string EmployeesView = "Employees.View";
    public const string EmployeesManage = "Employees.Manage";

    public const string ReportsView = "Reports.View";
    public const string ReportsExport = "Reports.Export";

    public const string UsersView = "Users.View";
    public const string UsersManage = "Users.Manage";

    public const string SettingsView = "Settings.View";
    public const string SettingsManage = "Settings.Manage";

    public const string BranchesView = "Branches.View";
    public const string BranchesManage = "Branches.Manage";

    public const string AuditView = "Audit.View";

    public static readonly IReadOnlyList<PermissionDefinition> All = new List<PermissionDefinition>
    {
        new(DashboardView, "View dashboard", "Dashboard"),
        new(MedicinesView, "View medicines", "Medicines"),
        new(MedicinesManage, "Manage medicines", "Medicines"),
        new(InventoryView, "View inventory", "Inventory"),
        new(InventoryAdjust, "Adjust stock", "Inventory"),
        new(InventoryTransfer, "Transfer stock", "Inventory"),
        new(SalesView, "View sales", "Sales"),
        new(SalesCreate, "Create sales", "Sales"),
        new(SalesCancel, "Cancel sales", "Sales"),
        new(PurchasesView, "View purchases", "Purchases"),
        new(PurchasesCreate, "Create purchase orders", "Purchases"),
        new(PurchasesApprove, "Approve purchase orders", "Purchases"),
        new(PurchasesReceive, "Receive goods", "Purchases"),
        new(SuppliersView, "View suppliers", "Suppliers"),
        new(SuppliersManage, "Manage suppliers", "Suppliers"),
        new(CustomersView, "View customers", "Customers"),
        new(CustomersManage, "Manage customers", "Customers"),
        new(PrescriptionsView, "View prescriptions", "Prescriptions"),
        new(PrescriptionsCreate, "Register prescriptions", "Prescriptions"),
        new(PrescriptionsVerify, "Verify prescriptions", "Prescriptions"),
        new(ReturnsView, "View returns", "Returns"),
        new(ReturnsCreate, "Create returns", "Returns"),
        new(ReturnsApprove, "Approve returns", "Returns"),
        new(ExpensesView, "View expenses", "Expenses"),
        new(ExpensesManage, "Manage expenses", "Expenses"),
        new(EmployeesView, "View employees", "Employees"),
        new(EmployeesManage, "Manage employees", "Employees"),
        new(ReportsView, "View reports", "Reports"),
        new(ReportsExport, "Export reports", "Reports"),
        new(UsersView, "View users", "Users"),
        new(UsersManage, "Manage users", "Users"),
        new(SettingsView, "View settings", "Settings"),
        new(SettingsManage, "Manage settings", "Settings"),
        new(BranchesView, "View branches", "Branches"),
        new(BranchesManage, "Manage branches", "Branches"),
        new(AuditView, "View audit logs", "Audit")
    };
}

public sealed record PermissionDefinition(string Code, string Name, string Module, string? Description = null);

public static class Roles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string PharmacyAdmin = "PharmacyAdmin";
    public const string Pharmacist = "Pharmacist";
    public const string Cashier = "Cashier";
    public const string InventoryManager = "InventoryManager";

    public static readonly IReadOnlyList<string> All = new[]
    {
        SuperAdmin, PharmacyAdmin, Pharmacist, Cashier, InventoryManager
    };

    public static IReadOnlyList<string> PermissionsFor(string role) => role switch
    {
        SuperAdmin => Permissions.All.Select(p => p.Code).ToList(),
        PharmacyAdmin => new[]
        {
            Permissions.DashboardView,
            Permissions.MedicinesView, Permissions.MedicinesManage,
            Permissions.InventoryView, Permissions.InventoryAdjust, Permissions.InventoryTransfer,
            Permissions.SalesView, Permissions.SalesCreate, Permissions.SalesCancel,
            Permissions.PurchasesView, Permissions.PurchasesCreate, Permissions.PurchasesApprove, Permissions.PurchasesReceive,
            Permissions.SuppliersView, Permissions.SuppliersManage,
            Permissions.CustomersView, Permissions.CustomersManage,
            Permissions.PrescriptionsView, Permissions.PrescriptionsCreate, Permissions.PrescriptionsVerify,
            Permissions.ReturnsView, Permissions.ReturnsCreate, Permissions.ReturnsApprove,
            Permissions.ExpensesView, Permissions.ExpensesManage,
            Permissions.EmployeesView, Permissions.EmployeesManage,
            Permissions.ReportsView, Permissions.ReportsExport,
            Permissions.UsersView, Permissions.UsersManage,
            Permissions.SettingsView, Permissions.BranchesView, Permissions.AuditView
        },
        Pharmacist => new[]
        {
            Permissions.DashboardView,
            Permissions.MedicinesView,
            Permissions.InventoryView, Permissions.InventoryAdjust,
            Permissions.SalesView,
            Permissions.CustomersView,
            Permissions.PrescriptionsView, Permissions.PrescriptionsCreate, Permissions.PrescriptionsVerify,
            Permissions.ReturnsView, Permissions.ReturnsCreate,
            Permissions.ReportsView
        },
        Cashier => new[]
        {
            Permissions.DashboardView,
            Permissions.MedicinesView,
            Permissions.InventoryView,
            Permissions.SalesView, Permissions.SalesCreate, Permissions.SalesCancel,
            Permissions.CustomersView, Permissions.CustomersManage,
            Permissions.PrescriptionsView,
            Permissions.ReturnsView, Permissions.ReturnsCreate,
            Permissions.ReportsView
        },
        InventoryManager => new[]
        {
            Permissions.DashboardView,
            Permissions.MedicinesView, Permissions.MedicinesManage,
            Permissions.InventoryView, Permissions.InventoryAdjust, Permissions.InventoryTransfer,
            Permissions.PurchasesView, Permissions.PurchasesCreate, Permissions.PurchasesApprove, Permissions.PurchasesReceive,
            Permissions.SuppliersView, Permissions.SuppliersManage,
            Permissions.ReportsView, Permissions.ReportsExport
        },
        _ => Array.Empty<string>()
    };
}

public static class CustomClaimTypes
{
    public const string Permission = "permission";
    public const string BranchId = "branch_id";
    public const string FullName = "full_name";
}
