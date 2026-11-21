# Database Design - SQL Server

## 1. Conventions

- Database: `PharmacyManagement`
- Collation: default SQL Server collation
- Primary keys: `int IDENTITY` for business entities, `uniqueidentifier` for Identity users/roles, `bigint IDENTITY` for high-volume logs/movements
- Monetary values: `decimal(18,2)`; percentages `decimal(5,2)`
- Timestamps: `datetime2` in UTC (`*AtUtc` suffix)
- Soft delete: `IsDeleted`, `DeletedAtUtc`, `DeletedByUserId` on master/operational entities
- Financial/inventory records are never hard-deleted
- Optimistic concurrency: `rowversion` on `MedicineBatches`
- Indexes on all foreign keys, unique business keys, and frequent search/filter columns
- Cascade behavior: `Restrict` for financial references, `Cascade` only for owned child rows (details, payments)

## 2. Entity Relationship Diagram

```mermaid
erDiagram
    ApplicationUser ||--o{ UserRoles : has
    ApplicationRole ||--o{ UserRoles : assigned
    ApplicationRole ||--o{ RolePermissions : grants
    Permission ||--o{ RolePermissions : included

    Branch ||--o{ Employees : employs
    Branch ||--o{ MedicineBatches : stocks
    Branch ||--o{ Sales : records
    Branch ||--o{ PurchaseOrders : orders
    Branch ||--o{ Expenses : incurs
    ApplicationUser ||--o{ Employees : account
    ApplicationUser ||--o{ RefreshTokens : owns

    MedicineCategory ||--o{ Medicines : classifies
    Manufacturer ||--o{ Medicines : produces
    Unit ||--o{ Medicines : measures
    Medicine ||--o{ MedicineBatches : has
    Supplier ||--o{ MedicineBatches : supplies
    Medicine ||--o{ InventoryTransactions : moves
    MedicineBatch ||--o{ InventoryTransactions : tracks
    Branch ||--o{ InventoryTransactions : scopes

    Supplier ||--o{ PurchaseOrders : fulfills
    PurchaseOrder ||--o{ PurchaseOrderDetails : contains
    Medicine ||--o{ PurchaseOrderDetails : ordered
    PurchaseOrder ||--o{ GoodsReceipts : receives
    GoodsReceipt ||--o{ GoodsReceiptDetails : contains
    MedicineBatch ||--o{ GoodsReceiptDetails : created
    Supplier ||--o{ SupplierPayments : paid

    Customer ||--o{ Sales : buys
    Customer ||--o{ CustomerPayments : pays
    Sale ||--o{ SaleDetails : contains
    MedicineBatch ||--o{ SaleDetails : consumed
    Sale ||--o{ SalePayments : paid
    Prescription ||--o{ Sale : authorizes
    Prescription ||--o{ PrescriptionDetails : contains
    Medicine ||--o{ PrescriptionDetails : prescribed

    Sale ||--o{ Returns : refunded
    PurchaseOrder ||--o{ Returns : returned
    Return ||--o{ ReturnDetails : contains
    MedicineBatch ||--o{ ReturnDetails : inspected
    Return ||--o{ Refunds : refunds

    StockTransfer ||--o{ StockTransferDetails : contains
    MedicineBatch ||--o{ StockTransferDetails : transferred
    Branch ||--o{ StockTransfer : from
    Branch ||--o{ StockTransfer : to

    ExpenseCategory ||--o{ Expenses : classifies
    ApplicationUser ||--o{ AuditLogs : acts
    ApplicationUser ||--o{ Sales : cashier
```

## 3. Tables

### Identity & Security

| Table | Key columns | Constraints / indexes |
|---|---|---|
| `ApplicationUsers` | `Id (guid)`, `Email`, `FirstName`, `LastName`, `IsActive`, `BranchId` | unique `NormalizedEmail`, unique `NormalizedUserName`, FK `BranchId` |
| `ApplicationRoles` | `Id (guid)`, `Name`, `Description`, `IsSystemRole` | unique `NormalizedName` |
| `UserRoles` | `UserId`, `RoleId` | composite PK, FKs |
| `Permissions` | `Id`, `Code`, `Name`, `Module` | unique `Code` |
| `RolePermissions` | `RoleId`, `PermissionId` | composite PK, FKs |
| `RefreshTokens` | `Id (guid)`, `UserId`, `TokenHash`, `ExpiresAtUtc`, `RevokedAtUtc` | unique `TokenHash`, FK user |
| `AuditLogs` | `Id (bigint)`, `UserId`, `Action`, `EntityName`, `EntityId`, `OldValues`, `NewValues`, `IpAddress`, `CreatedAtUtc` | index `(CreatedAtUtc)`, `(UserId)`, `(EntityName, EntityId)` |

### Organization

| Table | Key columns | Constraints / indexes |
|---|---|---|
| `Branches` | `Id`, `Name`, `Code`, `Address`, `Phone`, `IsActive` | unique `Code` |
| `Employees` | `Id`, `EmployeeCode`, `FirstName`, `LastName`, `Position`, `Salary`, `Status`, `BranchId`, `UserId` | unique `EmployeeCode`, FKs, soft delete |
| `SystemSettings` | `Id`, `Key`, `Value`, `Group`, `IsEditable` | unique `Key` |
| `DocumentSequences` | `Id`, `DocumentType`, `PeriodKey`, `LastNumber` | unique `(DocumentType, PeriodKey)` |

### Catalog

| Table | Key columns | Constraints / indexes |
|---|---|---|
| `MedicineCategories` | `Id`, `Name`, `IsActive` | unique `Name` |
| `Manufacturers` | `Id`, `Name`, `Country`, `IsActive` | unique `Name` |
| `Units` | `Id`, `Name`, `Abbreviation`, `IsActive` | unique `Name`, unique `Abbreviation` |
| `Medicines` | `Id`, `Name`, `GenericName`, `BrandName`, `CategoryId`, `ManufacturerId`, `UnitId`, `Barcode`, `Sku`, `DosageForm`, `Strength`, `PurchasePrice`, `SellingPrice`, `TaxPercentage`, `ReorderLevel`, `RequiresPrescription`, `IsActive` | unique `Sku`, filtered unique `Barcode`, FKs, indexes on `Name`, `CategoryId`, `ManufacturerId`, soft delete |

### Inventory

| Table | Key columns | Constraints / indexes |
|---|---|---|
| `MedicineBatches` | `Id`, `MedicineId`, `BranchId`, `SupplierId`, `BatchNumber`, `ManufacturingDate`, `ExpiryDate`, `PurchasePrice`, `SellingPrice`, `InitialQuantity`, `CurrentQuantity`, `RowVersion` | unique `(MedicineId, BranchId, BatchNumber)`, index `(BranchId, MedicineId, ExpiryDate)`, check `CurrentQuantity >= 0` |
| `InventoryTransactions` | `Id (bigint)`, `MedicineId`, `BatchId`, `BranchId`, `Type`, `QuantityChange`, `QuantityBefore`, `QuantityAfter`, `UnitCost`, `ReferenceType`, `ReferenceId`, `PerformedByUserId`, `CreatedAtUtc` | indexes `(CreatedAtUtc)`, `(MedicineId)`, `(BatchId)`, `(BranchId)` |
| `StockTransfers` | `Id`, `TransferNumber`, `FromBranchId`, `ToBranchId`, `Status`, `RequestedByUserId`, `ApprovedByUserId`, `TransferDateUtc` | unique `TransferNumber`, FKs |
| `StockTransferDetails` | `Id`, `StockTransferId`, `MedicineId`, `BatchId`, `Quantity`, `ReceivedQuantity` | FK cascade from transfer |

### Purchasing

| Table | Key columns | Constraints / indexes |
|---|---|---|
| `Suppliers` | `Id`, `Name`, `CompanyName`, `ContactPerson`, `Phone`, `Email`, `TaxRegistrationNumber`, `OpeningBalance`, `PaymentTerms`, `IsActive` | unique `Name`, soft delete |
| `PurchaseOrders` | `Id`, `OrderNumber`, `SupplierId`, `BranchId`, `OrderDateUtc`, `Status`, `SubTotal`, `TaxAmount`, `DiscountAmount`, `TotalAmount`, `CreatedByUserId`, `ApprovedByUserId` | unique `OrderNumber`, indexes `(Status)`, `(SupplierId)`, `(OrderDateUtc)` |
| `PurchaseOrderDetails` | `Id`, `PurchaseOrderId`, `MedicineId`, `OrderedQuantity`, `ReceivedQuantity`, `UnitPrice`, `TaxPercentage`, `LineTotal` | FKs, check `ReceivedQuantity <= OrderedQuantity` |
| `GoodsReceipts` | `Id`, `ReceiptNumber`, `PurchaseOrderId`, `ReceivedAtUtc`, `ReceivedByUserId`, `TotalAmount` | unique `ReceiptNumber` |
| `GoodsReceiptDetails` | `Id`, `GoodsReceiptId`, `MedicineId`, `BatchId`, `BatchNumber`, `ManufacturingDate`, `ExpiryDate`, `Quantity`, `UnitCost`, `LineTotal` | FKs |
| `SupplierPayments` | `Id`, `SupplierId`, `PurchaseOrderId`, `Amount`, `PaymentDateUtc`, `Method`, `Reference`, `RecordedByUserId` | indexes `(SupplierId)`, `(PaymentDateUtc)` |

### Sales

| Table | Key columns | Constraints / indexes |
|---|---|---|
| `Customers` | `Id`, `Code`, `FirstName`, `LastName`, `Phone`, `Email`, `CreditLimit`, `LoyaltyPoints`, `IsActive` | unique `Code`, soft delete |
| `Sales` | `Id`, `InvoiceNumber`, `BranchId`, `CustomerId`, `PrescriptionId`, `CashierUserId`, `SaleDateUtc`, `SubTotal`, `DiscountAmount`, `TaxAmount`, `TotalAmount`, `PaidAmount`, `ChangeAmount`, `Status`, `PaymentStatus` | unique `InvoiceNumber`, indexes `(SaleDateUtc)`, `(CustomerId)`, `(BranchId)`, `(Status)` |
| `SaleDetails` | `Id`, `SaleId`, `MedicineId`, `BatchId`, `BatchNumber`, `Quantity`, `UnitPrice`, `UnitCost`, `DiscountAmount`, `TaxPercentage`, `TaxAmount`, `LineTotal` | FK cascade from sale, indexes `(MedicineId)`, `(BatchId)` |
| `SalePayments` | `Id`, `SaleId`, `Method`, `Amount`, `Reference`, `PaidAtUtc` | FK cascade from sale |
| `Prescriptions` | `Id`, `PrescriptionNumber`, `BranchId`, `CustomerId`, `PatientName`, `DoctorName`, `PrescriptionDate`, `Status`, `ImagePath`, `VerifiedByUserId` | unique `PrescriptionNumber`, index `(Status)` |
| `PrescriptionDetails` | `Id`, `PrescriptionId`, `MedicineId`, `Dosage`, `Frequency`, `DurationDays`, `Quantity` | FK cascade |
| `CustomerPayments` | `Id`, `CustomerId`, `SaleId`, `Amount`, `PaymentDateUtc`, `Method`, `Reference`, `RecordedByUserId` | indexes `(CustomerId)`, `(PaymentDateUtc)` |

### Returns & Expenses

| Table | Key columns | Constraints / indexes |
|---|---|---|
| `Returns` | `Id`, `ReturnNumber`, `Type`, `BranchId`, `OriginalSaleId`, `OriginalPurchaseOrderId`, `CustomerId`, `SupplierId`, `Reason`, `Status`, `TotalAmount`, `RefundMethod`, `RequestedByUserId`, `ApprovedByUserId` | unique `ReturnNumber`, indexes `(Status)`, `(Type)` |
| `ReturnDetails` | `Id`, `ReturnId`, `MedicineId`, `BatchId`, `Quantity`, `UnitPrice`, `LineTotal`, `Condition`, `RestockDecision`, `InspectedByUserId` | FKs |
| `Refunds` | `Id`, `ReturnId`, `Amount`, `Method`, `RefundedAtUtc`, `ProcessedByUserId`, `Notes` | index `(ReturnId)` |
| `ExpenseCategories` | `Id`, `Name`, `IsActive` | unique `Name` |
| `Expenses` | `Id`, `ExpenseNumber`, `CategoryId`, `BranchId`, `ExpenseDateUtc`, `Amount`, `PaymentMethod`, `Description`, `AttachmentPath`, `RecordedByUserId` | unique `ExpenseNumber`, indexes `(ExpenseDateUtc)`, `(CategoryId)`, soft delete |

## 4. Views

| View | Purpose |
|---|---|
| `vw_MedicineStock` | Current sellable stock per medicine/branch (sum of non-expired batch quantities) |
| `vw_ExpiringBatches` | Batches expiring within N days with remaining quantity |
| `vw_LowStockMedicines` | Stock at or below reorder level per medicine/branch |
| `vw_StockValuation` | Inventory value at purchase cost and retail price per medicine/branch |

## 5. Stored Procedures

| Procedure | Purpose |
|---|---|
| `usp_NextDocumentNumber` | Atomically increments and returns a document sequence number |
| `usp_GetTopSellingMedicines` | Top-selling medicines for a date range and branch |
| `usp_GetSalesSummary` | Aggregated sales totals grouped by day for dashboard charts |

## 6. Referential Integrity & Deletion Policy

- Master data referenced by transactions cannot be hard-deleted; services deactivate or soft-delete.
- Sales, sale details, payments, returns, refunds, purchase orders, goods receipts, inventory transactions and audit logs are append-only.
- Sale cancellation keeps the sale row, marks it `Cancelled`, restores eligible stock and writes movements.
- Deleting a medicine sets `IsDeleted`; historical sale details keep their snapshot (`BatchNumber`, `UnitPrice`, `UnitCost`).
- All FK delete behaviors are explicitly configured (`Restrict` for financial/history tables).

## 7. Seed Data

Seeded idempotently on startup (Development/Testing):

- Permissions and the five system roles with role-permission mappings
- Initial SuperAdmin account (password from `Seed:AdminPassword`)
- One main branch, units, categories, manufacturers
- Sample medicines, suppliers, customers, batches, purchase orders, sales and expenses so the dashboard and reports show real database records

Seed data is real transactional data written through EF Core; no statistics are hardcoded in the UI.
