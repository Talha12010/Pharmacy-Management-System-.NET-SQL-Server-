IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] uniqueidentifier NOT NULL,
        [Description] nvarchar(500) NULL,
        [IsSystemRole] bit NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(500) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE TABLE [AuditLogs] (
        [Id] bigint NOT NULL IDENTITY,
        [UserId] uniqueidentifier NULL,
        [UserName] nvarchar(256) NULL,
        [Action] nvarchar(100) NOT NULL,
        [EntityName] nvarchar(100) NOT NULL,
        [EntityId] nvarchar(100) NULL,
        [OldValues] nvarchar(500) NULL,
        [NewValues] nvarchar(500) NULL,
        [IpAddress] nvarchar(64) NULL,
        [UserAgent] nvarchar(500) NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE TABLE [Branches] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [Code] nvarchar(50) NOT NULL,
        [Address] nvarchar(500) NULL,
        [Phone] nvarchar(30) NULL,
        [Email] nvarchar(256) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [CreatedByUserId] uniqueidentifier NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [UpdatedByUserId] uniqueidentifier NULL,
        CONSTRAINT [PK_Branches] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE TABLE [Customers] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(50) NOT NULL,
        [FirstName] nvarchar(100) NOT NULL,
        [LastName] nvarchar(100) NOT NULL,
        [Phone] nvarchar(30) NULL,
        [Email] nvarchar(256) NULL,
        [Address] nvarchar(500) NULL,
        [DateOfBirth] datetime2 NULL,
        [Gender] int NULL,
        [LoyaltyPoints] int NOT NULL,
        [CreditLimit] decimal(18,2) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [CreatedByUserId] uniqueidentifier NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [UpdatedByUserId] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAtUtc] datetime2 NULL,
        [DeletedByUserId] uniqueidentifier NULL,
        CONSTRAINT [PK_Customers] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE TABLE [DocumentSequences] (
        [Id] int NOT NULL IDENTITY,
        [DocumentType] nvarchar(50) NOT NULL,
        [PeriodKey] nvarchar(20) NOT NULL,
        [LastNumber] int NOT NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_DocumentSequences] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE TABLE [ExpenseCategories] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(150) NOT NULL,
        [Description] nvarchar(500) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_ExpenseCategories] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE TABLE [Manufacturers] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [Country] nvarchar(100) NULL,
        [ContactInfo] nvarchar(500) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Manufacturers] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE TABLE [MedicineCategories] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(150) NOT NULL,
        [Description] nvarchar(500) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_MedicineCategories] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE TABLE [Permissions] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(100) NOT NULL,
        [Name] nvarchar(150) NOT NULL,
        [Module] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NULL,
        CONSTRAINT [PK_Permissions] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE TABLE [Suppliers] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [CompanyName] nvarchar(200) NULL,
        [ContactPerson] nvarchar(100) NULL,
        [Phone] nvarchar(30) NULL,
        [Email] nvarchar(256) NULL,
        [Address] nvarchar(500) NULL,
        [TaxRegistrationNumber] nvarchar(100) NULL,
        [OpeningBalance] decimal(18,2) NOT NULL,
        [PaymentTerms] nvarchar(200) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [CreatedByUserId] uniqueidentifier NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [UpdatedByUserId] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAtUtc] datetime2 NULL,
        [DeletedByUserId] uniqueidentifier NULL,
        CONSTRAINT [PK_Suppliers] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE TABLE [SystemSettings] (
        [Id] int NOT NULL IDENTITY,
        [Key] nvarchar(100) NOT NULL,
        [Value] nvarchar(2000) NOT NULL,
        [Description] nvarchar(500) NULL,
        [Group] nvarchar(100) NOT NULL,
        [IsEditable] bit NOT NULL,
        [UpdatedAtUtc] datetime2 NOT NULL,
        [UpdatedByUserId] uniqueidentifier NULL,
        CONSTRAINT [PK_SystemSettings] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE TABLE [Units] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [Abbreviation] nvarchar(20) NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Units] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] uniqueidentifier NOT NULL,
        [ClaimType] nvarchar(500) NULL,
        [ClaimValue] nvarchar(500) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] uniqueidentifier NOT NULL,
        [FirstName] nvarchar(100) NOT NULL,
        [LastName] nvarchar(100) NOT NULL,
        [IsActive] bit NOT NULL,
        [BranchId] int NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [LastLoginAtUtc] datetime2 NULL,
        [ProfileImagePath] nvarchar(500) NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(500) NULL,
        [SecurityStamp] nvarchar(500) NULL,
        [ConcurrencyStamp] nvarchar(500) NULL,
        [PhoneNumber] nvarchar(500) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUsers_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE SET NULL
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE TABLE [StockTransfers] (
        [Id] int NOT NULL IDENTITY,
        [TransferNumber] nvarchar(50) NOT NULL,
        [FromBranchId] int NOT NULL,
        [ToBranchId] int NOT NULL,
        [Status] int NOT NULL,
        [RequestedByUserId] uniqueidentifier NULL,
        [ApprovedByUserId] uniqueidentifier NULL,
        [TransferDateUtc] datetime2 NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [Notes] nvarchar(1000) NULL,
        CONSTRAINT [PK_StockTransfers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_StockTransfers_Branches_FromBranchId] FOREIGN KEY ([FromBranchId]) REFERENCES [Branches] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_StockTransfers_Branches_ToBranchId] FOREIGN KEY ([ToBranchId]) REFERENCES [Branches] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE TABLE [Prescriptions] (
        [Id] int NOT NULL IDENTITY,
        [PrescriptionNumber] nvarchar(50) NOT NULL,
        [BranchId] int NOT NULL,
        [CustomerId] int NULL,
        [PatientName] nvarchar(200) NOT NULL,
        [PatientPhone] nvarchar(30) NULL,
        [PatientAddress] nvarchar(500) NULL,
        [DoctorName] nvarchar(200) NOT NULL,
        [DoctorLicenseNumber] nvarchar(100) NULL,
        [PrescriptionDate] datetime2 NOT NULL,
        [Status] int NOT NULL,
        [ImagePath] nvarchar(500) NULL,
        [Notes] nvarchar(1000) NULL,
        [RejectionReason] nvarchar(1000) NULL,
        [CreatedByUserId] uniqueidentifier NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [VerifiedByUserId] uniqueidentifier NULL,
        [VerifiedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_Prescriptions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Prescriptions_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Prescriptions_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE SET NULL
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE TABLE [Expenses] (
        [Id] int NOT NULL IDENTITY,
        [ExpenseNumber] nvarchar(50) NOT NULL,
        [ExpenseCategoryId] int NOT NULL,
        [BranchId] int NOT NULL,
        [ExpenseDateUtc] datetime2 NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [PaymentMethod] int NOT NULL,
        [Description] nvarchar(1000) NOT NULL,
        [AttachmentPath] nvarchar(500) NULL,
        [RecordedByUserId] uniqueidentifier NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [CreatedByUserId] uniqueidentifier NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [UpdatedByUserId] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAtUtc] datetime2 NULL,
        [DeletedByUserId] uniqueidentifier NULL,
        CONSTRAINT [PK_Expenses] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Expenses_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Expenses_ExpenseCategories_ExpenseCategoryId] FOREIGN KEY ([ExpenseCategoryId]) REFERENCES [ExpenseCategories] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE TABLE [RolePermissions] (
        [RoleId] uniqueidentifier NOT NULL,
        [PermissionId] int NOT NULL,
        CONSTRAINT [PK_RolePermissions] PRIMARY KEY ([RoleId], [PermissionId]),
        CONSTRAINT [FK_RolePermissions_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_RolePermissions_Permissions_PermissionId] FOREIGN KEY ([PermissionId]) REFERENCES [Permissions] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE TABLE [PurchaseOrders] (
        [Id] int NOT NULL IDENTITY,
        [OrderNumber] nvarchar(50) NOT NULL,
        [SupplierId] int NOT NULL,
        [BranchId] int NOT NULL,
        [OrderDateUtc] datetime2 NOT NULL,
        [ExpectedDeliveryDate] datetime2 NULL,
        [Status] int NOT NULL,
        [SubTotal] decimal(18,2) NOT NULL,
        [TaxAmount] decimal(18,2) NOT NULL,
        [DiscountAmount] decimal(18,2) NOT NULL,
        [TotalAmount] decimal(18,2) NOT NULL,
        [Notes] nvarchar(1000) NULL,
        [CreatedByUserId] uniqueidentifier NOT NULL,
        [ApprovedByUserId] uniqueidentifier NULL,
        [ApprovedAtUtc] datetime2 NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [RejectionReason] nvarchar(1000) NULL,
        CONSTRAINT [PK_PurchaseOrders] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PurchaseOrders_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PurchaseOrders_Suppliers_SupplierId] FOREIGN KEY ([SupplierId]) REFERENCES [Suppliers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE TABLE [Medicines] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [GenericName] nvarchar(200) NULL,
        [BrandName] nvarchar(200) NULL,
        [CategoryId] int NOT NULL,
        [ManufacturerId] int NOT NULL,
        [UnitId] int NOT NULL,
        [Barcode] nvarchar(100) NULL,
        [Sku] nvarchar(50) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [DosageForm] nvarchar(100) NOT NULL,
        [Strength] nvarchar(100) NOT NULL,
        [PurchasePrice] decimal(18,2) NOT NULL,
        [SellingPrice] decimal(18,2) NOT NULL,
        [TaxPercentage] decimal(18,2) NOT NULL,
        [ReorderLevel] int NOT NULL,
        [RequiresPrescription] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [CreatedByUserId] uniqueidentifier NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [UpdatedByUserId] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAtUtc] datetime2 NULL,
        [DeletedByUserId] uniqueidentifier NULL,
        CONSTRAINT [PK_Medicines] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Medicines_Manufacturers_ManufacturerId] FOREIGN KEY ([ManufacturerId]) REFERENCES [Manufacturers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Medicines_MedicineCategories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [MedicineCategories] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Medicines_Units_UnitId] FOREIGN KEY ([UnitId]) REFERENCES [Units] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] uniqueidentifier NOT NULL,
        [ClaimType] nvarchar(500) NULL,
        [ClaimValue] nvarchar(500) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(500) NOT NULL,
        [ProviderKey] nvarchar(500) NOT NULL,
        [ProviderDisplayName] nvarchar(500) NULL,
        [UserId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] uniqueidentifier NOT NULL,
        [RoleId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] uniqueidentifier NOT NULL,
        [LoginProvider] nvarchar(500) NOT NULL,
        [Name] nvarchar(500) NOT NULL,
        [Value] nvarchar(500) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE TABLE [Employees] (
        [Id] int NOT NULL IDENTITY,
        [EmployeeCode] nvarchar(50) NOT NULL,
        [FirstName] nvarchar(100) NOT NULL,
        [LastName] nvarchar(100) NOT NULL,
        [Phone] nvarchar(30) NULL,
        [Email] nvarchar(256) NULL,
        [Address] nvarchar(500) NULL,
        [Position] nvarchar(100) NOT NULL,
        [HireDate] datetime2 NOT NULL,
        [Salary] decimal(18,2) NOT NULL,
        [Status] int NOT NULL,
        [BranchId] int NOT NULL,
        [UserId] uniqueidentifier NULL,
        [Notes] nvarchar(1000) NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [CreatedByUserId] uniqueidentifier NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [UpdatedByUserId] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAtUtc] datetime2 NULL,
        [DeletedByUserId] uniqueidentifier NULL,
        CONSTRAINT [PK_Employees] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Employees_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_Employees_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE TABLE [RefreshTokens] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [TokenHash] nvarchar(256) NOT NULL,
        [ExpiresAtUtc] datetime2 NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [CreatedByIp] nvarchar(64) NULL,
        [RevokedAtUtc] datetime2 NULL,
        [RevokedByIp] nvarchar(64) NULL,
        [ReplacedByTokenHash] nvarchar(256) NULL,
        CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RefreshTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE TABLE [Sales] (
        [Id] int NOT NULL IDENTITY,
        [InvoiceNumber] nvarchar(50) NOT NULL,
        [BranchId] int NOT NULL,
        [CustomerId] int NULL,
        [PrescriptionId] int NULL,
        [CashierUserId] uniqueidentifier NOT NULL,
        [SaleDateUtc] datetime2 NOT NULL,
        [SubTotal] decimal(18,2) NOT NULL,
        [DiscountAmount] decimal(18,2) NOT NULL,
        [TaxAmount] decimal(18,2) NOT NULL,
        [TotalAmount] decimal(18,2) NOT NULL,
        [PaidAmount] decimal(18,2) NOT NULL,
        [ChangeAmount] decimal(18,2) NOT NULL,
        [Status] int NOT NULL,
        [PaymentStatus] int NOT NULL,
        [Notes] nvarchar(1000) NULL,
        [CancelledAtUtc] datetime2 NULL,
        [CancelledByUserId] uniqueidentifier NULL,
        [CancellationReason] nvarchar(1000) NULL,
        CONSTRAINT [PK_Sales] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Sales_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Sales_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_Sales_Prescriptions_PrescriptionId] FOREIGN KEY ([PrescriptionId]) REFERENCES [Prescriptions] ([Id]) ON DELETE SET NULL
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE TABLE [GoodsReceipts] (
        [Id] int NOT NULL IDENTITY,
        [ReceiptNumber] nvarchar(50) NOT NULL,
        [PurchaseOrderId] int NOT NULL,
        [ReceivedAtUtc] datetime2 NOT NULL,
        [ReceivedByUserId] uniqueidentifier NOT NULL,
        [TotalAmount] decimal(18,2) NOT NULL,
        [Notes] nvarchar(1000) NULL,
        CONSTRAINT [PK_GoodsReceipts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_GoodsReceipts_PurchaseOrders_PurchaseOrderId] FOREIGN KEY ([PurchaseOrderId]) REFERENCES [PurchaseOrders] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE TABLE [SupplierPayments] (
        [Id] int NOT NULL IDENTITY,
        [SupplierId] int NOT NULL,
        [PurchaseOrderId] int NULL,
        [Amount] decimal(18,2) NOT NULL,
        [PaymentDateUtc] datetime2 NOT NULL,
        [Method] int NOT NULL,
        [Reference] nvarchar(100) NULL,
        [Notes] nvarchar(500) NULL,
        [RecordedByUserId] uniqueidentifier NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_SupplierPayments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SupplierPayments_PurchaseOrders_PurchaseOrderId] FOREIGN KEY ([PurchaseOrderId]) REFERENCES [PurchaseOrders] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_SupplierPayments_Suppliers_SupplierId] FOREIGN KEY ([SupplierId]) REFERENCES [Suppliers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE TABLE [MedicineBatches] (
        [Id] int NOT NULL IDENTITY,
        [MedicineId] int NOT NULL,
        [BranchId] int NOT NULL,
        [SupplierId] int NULL,
        [BatchNumber] nvarchar(50) NOT NULL,
        [ManufacturingDate] datetime2 NOT NULL,
        [ExpiryDate] datetime2 NOT NULL,
        [PurchasePrice] decimal(18,2) NOT NULL,
        [SellingPrice] decimal(18,2) NOT NULL,
        [InitialQuantity] int NOT NULL,
        [CurrentQuantity] int NOT NULL,
        [ReceivedAtUtc] datetime2 NOT NULL,
        [IsActive] bit NOT NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_MedicineBatches] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MedicineBatches_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_MedicineBatches_Medicines_MedicineId] FOREIGN KEY ([MedicineId]) REFERENCES [Medicines] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_MedicineBatches_Suppliers_SupplierId] FOREIGN KEY ([SupplierId]) REFERENCES [Suppliers] ([Id]) ON DELETE SET NULL
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE TABLE [PrescriptionDetails] (
        [Id] int NOT NULL IDENTITY,
        [PrescriptionId] int NOT NULL,
        [MedicineId] int NOT NULL,
        [Dosage] nvarchar(200) NOT NULL,
        [Frequency] nvarchar(100) NULL,
        [DurationDays] int NULL,
        [Quantity] int NOT NULL,
        [Instructions] nvarchar(1000) NULL,
        CONSTRAINT [PK_PrescriptionDetails] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PrescriptionDetails_Medicines_MedicineId] FOREIGN KEY ([MedicineId]) REFERENCES [Medicines] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PrescriptionDetails_Prescriptions_PrescriptionId] FOREIGN KEY ([PrescriptionId]) REFERENCES [Prescriptions] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE TABLE [PurchaseOrderDetails] (
        [Id] int NOT NULL IDENTITY,
        [PurchaseOrderId] int NOT NULL,
        [MedicineId] int NOT NULL,
        [OrderedQuantity] int NOT NULL,
        [ReceivedQuantity] int NOT NULL,
        [UnitPrice] decimal(18,2) NOT NULL,
        [TaxPercentage] decimal(18,2) NOT NULL,
        [LineTotal] decimal(18,2) NOT NULL,
        [BatchNumber] nvarchar(50) NULL,
        [ManufacturingDate] datetime2 NULL,
        [ExpiryDate] datetime2 NULL,
        CONSTRAINT [PK_PurchaseOrderDetails] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PurchaseOrderDetails_Medicines_MedicineId] FOREIGN KEY ([MedicineId]) REFERENCES [Medicines] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PurchaseOrderDetails_PurchaseOrders_PurchaseOrderId] FOREIGN KEY ([PurchaseOrderId]) REFERENCES [PurchaseOrders] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE TABLE [CustomerPayments] (
        [Id] int NOT NULL IDENTITY,
        [CustomerId] int NOT NULL,
        [SaleId] int NULL,
        [Amount] decimal(18,2) NOT NULL,
        [PaymentDateUtc] datetime2 NOT NULL,
        [Method] int NOT NULL,
        [Reference] nvarchar(100) NULL,
        [Notes] nvarchar(500) NULL,
        [RecordedByUserId] uniqueidentifier NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_CustomerPayments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CustomerPayments_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CustomerPayments_Sales_SaleId] FOREIGN KEY ([SaleId]) REFERENCES [Sales] ([Id]) ON DELETE SET NULL
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE TABLE [Returns] (
        [Id] int NOT NULL IDENTITY,
        [ReturnNumber] nvarchar(50) NOT NULL,
        [Type] int NOT NULL,
        [BranchId] int NOT NULL,
        [OriginalSaleId] int NULL,
        [OriginalPurchaseOrderId] int NULL,
        [CustomerId] int NULL,
        [SupplierId] int NULL,
        [Reason] nvarchar(1000) NOT NULL,
        [Status] int NOT NULL,
        [TotalAmount] decimal(18,2) NOT NULL,
        [RefundMethod] int NULL,
        [Notes] nvarchar(1000) NULL,
        [RequestedByUserId] uniqueidentifier NOT NULL,
        [RequestedAtUtc] datetime2 NOT NULL,
        [ApprovedByUserId] uniqueidentifier NULL,
        [ApprovedAtUtc] datetime2 NULL,
        [RejectionReason] nvarchar(1000) NULL,
        CONSTRAINT [PK_Returns] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Returns_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Returns_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_Returns_PurchaseOrders_OriginalPurchaseOrderId] FOREIGN KEY ([OriginalPurchaseOrderId]) REFERENCES [PurchaseOrders] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Returns_Sales_OriginalSaleId] FOREIGN KEY ([OriginalSaleId]) REFERENCES [Sales] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Returns_Suppliers_SupplierId] FOREIGN KEY ([SupplierId]) REFERENCES [Suppliers] ([Id]) ON DELETE SET NULL
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE TABLE [SalePayments] (
        [Id] int NOT NULL IDENTITY,
        [SaleId] int NOT NULL,
        [Method] int NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Reference] nvarchar(100) NULL,
        [PaidAtUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_SalePayments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SalePayments_Sales_SaleId] FOREIGN KEY ([SaleId]) REFERENCES [Sales] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE TABLE [GoodsReceiptDetails] (
        [Id] int NOT NULL IDENTITY,
        [GoodsReceiptId] int NOT NULL,
        [MedicineId] int NOT NULL,
        [BatchId] int NOT NULL,
        [BatchNumber] nvarchar(50) NOT NULL,
        [ManufacturingDate] datetime2 NOT NULL,
        [ExpiryDate] datetime2 NOT NULL,
        [Quantity] int NOT NULL,
        [UnitCost] decimal(18,2) NOT NULL,
        [LineTotal] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_GoodsReceiptDetails] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_GoodsReceiptDetails_GoodsReceipts_GoodsReceiptId] FOREIGN KEY ([GoodsReceiptId]) REFERENCES [GoodsReceipts] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_GoodsReceiptDetails_MedicineBatches_BatchId] FOREIGN KEY ([BatchId]) REFERENCES [MedicineBatches] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_GoodsReceiptDetails_Medicines_MedicineId] FOREIGN KEY ([MedicineId]) REFERENCES [Medicines] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE TABLE [InventoryTransactions] (
        [Id] bigint NOT NULL IDENTITY,
        [MedicineId] int NOT NULL,
        [BatchId] int NULL,
        [BranchId] int NOT NULL,
        [Type] int NOT NULL,
        [QuantityChange] int NOT NULL,
        [QuantityBefore] int NOT NULL,
        [QuantityAfter] int NOT NULL,
        [UnitCost] decimal(18,2) NULL,
        [ReferenceType] nvarchar(50) NULL,
        [ReferenceId] int NULL,
        [Notes] nvarchar(500) NULL,
        [PerformedByUserId] uniqueidentifier NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_InventoryTransactions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_InventoryTransactions_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_InventoryTransactions_MedicineBatches_BatchId] FOREIGN KEY ([BatchId]) REFERENCES [MedicineBatches] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_InventoryTransactions_Medicines_MedicineId] FOREIGN KEY ([MedicineId]) REFERENCES [Medicines] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE TABLE [SaleDetails] (
        [Id] int NOT NULL IDENTITY,
        [SaleId] int NOT NULL,
        [MedicineId] int NOT NULL,
        [BatchId] int NOT NULL,
        [BatchNumber] nvarchar(50) NOT NULL,
        [Quantity] int NOT NULL,
        [ReturnedQuantity] int NOT NULL,
        [UnitPrice] decimal(18,2) NOT NULL,
        [UnitCost] decimal(18,2) NOT NULL,
        [DiscountAmount] decimal(18,2) NOT NULL,
        [TaxPercentage] decimal(18,2) NOT NULL,
        [TaxAmount] decimal(18,2) NOT NULL,
        [LineTotal] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_SaleDetails] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SaleDetails_MedicineBatches_BatchId] FOREIGN KEY ([BatchId]) REFERENCES [MedicineBatches] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SaleDetails_Medicines_MedicineId] FOREIGN KEY ([MedicineId]) REFERENCES [Medicines] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SaleDetails_Sales_SaleId] FOREIGN KEY ([SaleId]) REFERENCES [Sales] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE TABLE [StockTransferDetails] (
        [Id] int NOT NULL IDENTITY,
        [StockTransferId] int NOT NULL,
        [MedicineId] int NOT NULL,
        [BatchId] int NOT NULL,
        [Quantity] int NOT NULL,
        [ReceivedQuantity] int NOT NULL,
        CONSTRAINT [PK_StockTransferDetails] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_StockTransferDetails_MedicineBatches_BatchId] FOREIGN KEY ([BatchId]) REFERENCES [MedicineBatches] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_StockTransferDetails_Medicines_MedicineId] FOREIGN KEY ([MedicineId]) REFERENCES [Medicines] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_StockTransferDetails_StockTransfers_StockTransferId] FOREIGN KEY ([StockTransferId]) REFERENCES [StockTransfers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE TABLE [Refunds] (
        [Id] int NOT NULL IDENTITY,
        [ReturnId] int NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Method] int NOT NULL,
        [RefundedAtUtc] datetime2 NOT NULL,
        [ProcessedByUserId] uniqueidentifier NOT NULL,
        [Notes] nvarchar(500) NULL,
        CONSTRAINT [PK_Refunds] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Refunds_Returns_ReturnId] FOREIGN KEY ([ReturnId]) REFERENCES [Returns] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE TABLE [ReturnDetails] (
        [Id] int NOT NULL IDENTITY,
        [ReturnId] int NOT NULL,
        [MedicineId] int NOT NULL,
        [BatchId] int NOT NULL,
        [Quantity] int NOT NULL,
        [UnitPrice] decimal(18,2) NOT NULL,
        [LineTotal] decimal(18,2) NOT NULL,
        [Condition] int NOT NULL,
        [RestockDecision] int NOT NULL,
        [InspectedByUserId] uniqueidentifier NULL,
        [InspectedAtUtc] datetime2 NULL,
        [Notes] nvarchar(500) NULL,
        CONSTRAINT [PK_ReturnDetails] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ReturnDetails_MedicineBatches_BatchId] FOREIGN KEY ([BatchId]) REFERENCES [MedicineBatches] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ReturnDetails_Medicines_MedicineId] FOREIGN KEY ([MedicineId]) REFERENCES [Medicines] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ReturnDetails_Returns_ReturnId] FOREIGN KEY ([ReturnId]) REFERENCES [Returns] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUsers_BranchId] ON [AspNetUsers] ([BranchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_CreatedAtUtc] ON [AuditLogs] ([CreatedAtUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_EntityName_EntityId] ON [AuditLogs] ([EntityName], [EntityId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_UserId] ON [AuditLogs] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Branches_Code] ON [Branches] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CustomerPayments_CustomerId] ON [CustomerPayments] ([CustomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CustomerPayments_PaymentDateUtc] ON [CustomerPayments] ([PaymentDateUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CustomerPayments_SaleId] ON [CustomerPayments] ([SaleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Customers_Code] ON [Customers] ([Code]) WHERE [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Customers_Email] ON [Customers] ([Email]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Customers_Phone] ON [Customers] ([Phone]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_DocumentSequences_DocumentType_PeriodKey] ON [DocumentSequences] ([DocumentType], [PeriodKey]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Employees_BranchId] ON [Employees] ([BranchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Employees_EmployeeCode] ON [Employees] ([EmployeeCode]) WHERE [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Employees_UserId] ON [Employees] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ExpenseCategories_Name] ON [ExpenseCategories] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Expenses_BranchId] ON [Expenses] ([BranchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Expenses_ExpenseCategoryId] ON [Expenses] ([ExpenseCategoryId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Expenses_ExpenseDateUtc] ON [Expenses] ([ExpenseDateUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Expenses_ExpenseNumber] ON [Expenses] ([ExpenseNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_GoodsReceiptDetails_BatchId] ON [GoodsReceiptDetails] ([BatchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_GoodsReceiptDetails_GoodsReceiptId] ON [GoodsReceiptDetails] ([GoodsReceiptId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_GoodsReceiptDetails_MedicineId] ON [GoodsReceiptDetails] ([MedicineId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_GoodsReceipts_PurchaseOrderId] ON [GoodsReceipts] ([PurchaseOrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_GoodsReceipts_ReceiptNumber] ON [GoodsReceipts] ([ReceiptNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_GoodsReceipts_ReceivedAtUtc] ON [GoodsReceipts] ([ReceivedAtUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_InventoryTransactions_BatchId] ON [InventoryTransactions] ([BatchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_InventoryTransactions_BranchId] ON [InventoryTransactions] ([BranchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_InventoryTransactions_CreatedAtUtc] ON [InventoryTransactions] ([CreatedAtUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_InventoryTransactions_MedicineId_CreatedAtUtc] ON [InventoryTransactions] ([MedicineId], [CreatedAtUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Manufacturers_Name] ON [Manufacturers] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_MedicineBatches_BranchId] ON [MedicineBatches] ([BranchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_MedicineBatches_BranchId_MedicineId_ExpiryDate] ON [MedicineBatches] ([BranchId], [MedicineId], [ExpiryDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_MedicineBatches_ExpiryDate] ON [MedicineBatches] ([ExpiryDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_MedicineBatches_MedicineId_BranchId_BatchNumber] ON [MedicineBatches] ([MedicineId], [BranchId], [BatchNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_MedicineBatches_SupplierId] ON [MedicineBatches] ([SupplierId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_MedicineCategories_Name] ON [MedicineCategories] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Medicines_Barcode] ON [Medicines] ([Barcode]) WHERE [Barcode] IS NOT NULL AND [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Medicines_CategoryId] ON [Medicines] ([CategoryId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Medicines_GenericName] ON [Medicines] ([GenericName]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Medicines_IsActive] ON [Medicines] ([IsActive]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Medicines_ManufacturerId] ON [Medicines] ([ManufacturerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Medicines_Name] ON [Medicines] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Medicines_Sku] ON [Medicines] ([Sku]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Medicines_UnitId] ON [Medicines] ([UnitId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Permissions_Code] ON [Permissions] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Permissions_Module] ON [Permissions] ([Module]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PrescriptionDetails_MedicineId] ON [PrescriptionDetails] ([MedicineId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PrescriptionDetails_PrescriptionId] ON [PrescriptionDetails] ([PrescriptionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Prescriptions_BranchId] ON [Prescriptions] ([BranchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Prescriptions_CustomerId] ON [Prescriptions] ([CustomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Prescriptions_PrescriptionDate] ON [Prescriptions] ([PrescriptionDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Prescriptions_PrescriptionNumber] ON [Prescriptions] ([PrescriptionNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Prescriptions_Status] ON [Prescriptions] ([Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PurchaseOrderDetails_MedicineId] ON [PurchaseOrderDetails] ([MedicineId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PurchaseOrderDetails_PurchaseOrderId] ON [PurchaseOrderDetails] ([PurchaseOrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PurchaseOrders_BranchId] ON [PurchaseOrders] ([BranchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PurchaseOrders_OrderDateUtc] ON [PurchaseOrders] ([OrderDateUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PurchaseOrders_OrderNumber] ON [PurchaseOrders] ([OrderNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PurchaseOrders_Status] ON [PurchaseOrders] ([Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PurchaseOrders_SupplierId] ON [PurchaseOrders] ([SupplierId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RefreshTokens_ExpiresAtUtc] ON [RefreshTokens] ([ExpiresAtUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RefreshTokens_TokenHash] ON [RefreshTokens] ([TokenHash]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RefreshTokens_UserId] ON [RefreshTokens] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Refunds_RefundedAtUtc] ON [Refunds] ([RefundedAtUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Refunds_ReturnId] ON [Refunds] ([ReturnId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ReturnDetails_BatchId] ON [ReturnDetails] ([BatchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ReturnDetails_MedicineId] ON [ReturnDetails] ([MedicineId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ReturnDetails_ReturnId] ON [ReturnDetails] ([ReturnId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Returns_BranchId] ON [Returns] ([BranchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Returns_CustomerId] ON [Returns] ([CustomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Returns_OriginalPurchaseOrderId] ON [Returns] ([OriginalPurchaseOrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Returns_OriginalSaleId] ON [Returns] ([OriginalSaleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Returns_RequestedAtUtc] ON [Returns] ([RequestedAtUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Returns_ReturnNumber] ON [Returns] ([ReturnNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Returns_Status] ON [Returns] ([Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Returns_SupplierId] ON [Returns] ([SupplierId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Returns_Type] ON [Returns] ([Type]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RolePermissions_PermissionId] ON [RolePermissions] ([PermissionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SaleDetails_BatchId] ON [SaleDetails] ([BatchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SaleDetails_MedicineId] ON [SaleDetails] ([MedicineId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SaleDetails_SaleId] ON [SaleDetails] ([SaleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SalePayments_PaidAtUtc] ON [SalePayments] ([PaidAtUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SalePayments_SaleId] ON [SalePayments] ([SaleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Sales_BranchId] ON [Sales] ([BranchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Sales_CustomerId] ON [Sales] ([CustomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Sales_InvoiceNumber] ON [Sales] ([InvoiceNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Sales_PrescriptionId] ON [Sales] ([PrescriptionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Sales_SaleDateUtc] ON [Sales] ([SaleDateUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Sales_Status] ON [Sales] ([Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_StockTransferDetails_BatchId] ON [StockTransferDetails] ([BatchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_StockTransferDetails_MedicineId] ON [StockTransferDetails] ([MedicineId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_StockTransferDetails_StockTransferId] ON [StockTransferDetails] ([StockTransferId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_StockTransfers_FromBranchId] ON [StockTransfers] ([FromBranchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_StockTransfers_Status] ON [StockTransfers] ([Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_StockTransfers_ToBranchId] ON [StockTransfers] ([ToBranchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_StockTransfers_TransferDateUtc] ON [StockTransfers] ([TransferDateUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_StockTransfers_TransferNumber] ON [StockTransfers] ([TransferNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SupplierPayments_PaymentDateUtc] ON [SupplierPayments] ([PaymentDateUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SupplierPayments_PurchaseOrderId] ON [SupplierPayments] ([PurchaseOrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SupplierPayments_SupplierId] ON [SupplierPayments] ([SupplierId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Suppliers_Name] ON [Suppliers] ([Name]) WHERE [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Suppliers_Phone] ON [Suppliers] ([Phone]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SystemSettings_Key] ON [SystemSettings] ([Key]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Units_Abbreviation] ON [Units] ([Abbreviation]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Units_Name] ON [Units] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121110513_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261121110513_InitialCreate', N'10.0.15');
END;

COMMIT;
GO

