using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Domain.Entities;
using PharmacyManagement.Domain.Enums;
using PharmacyManagement.Infrastructure.Persistence;

namespace PharmacyManagement.Infrastructure.Persistence;

public sealed class DbInitializer
{
    private readonly ApplicationDbContext _db;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;

    public DbInitializer(
        ApplicationDbContext db,
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        _db = db;
        _roleManager = roleManager;
        _userManager = userManager;
        _configuration = configuration;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await SeedRolesAndPermissionsAsync(cancellationToken);

        var branch = await SeedBranchAsync(cancellationToken);
        await SeedReferenceDataAsync(cancellationToken);
        await SeedAdminAsync(branch.Id, cancellationToken);
        await SeedSampleInventoryAsync(branch.Id, cancellationToken);
    }

    private async Task SeedRolesAndPermissionsAsync(CancellationToken cancellationToken)
    {
        foreach (var permission in Permissions.All)
        {
            var existing = await _db.Permissions.SingleOrDefaultAsync(p => p.Code == permission.Code, cancellationToken);
            if (existing is null)
            {
                _db.Permissions.Add(new Permission
                {
                    Code = permission.Code,
                    Name = permission.Name,
                    Module = permission.Module,
                    Description = permission.Description
                });
            }
            else
            {
                existing.Name = permission.Name;
                existing.Module = permission.Module;
            }
        }

        await _db.SaveChangesAsync(cancellationToken);

        foreach (var roleName in Roles.All)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role is null)
            {
                role = new ApplicationRole { Name = roleName, IsSystemRole = true, NormalizedName = roleName.ToUpperInvariant() };
                await _roleManager.CreateAsync(role);
            }
            else
            {
                role.IsSystemRole = true;
                await _roleManager.UpdateAsync(role);
            }

            var permissionCodes = Roles.PermissionsFor(roleName);
            var permissionIds = await _db.Permissions
                .Where(p => permissionCodes.Contains(p.Code))
                .Select(p => p.Id)
                .ToListAsync(cancellationToken);

            var existingLinks = await _db.RolePermissions
                .Where(rp => rp.RoleId == role.Id)
                .ToListAsync(cancellationToken);
            _db.RolePermissions.RemoveRange(existingLinks);

            foreach (var permissionId in permissionIds)
            {
                _db.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionId = permissionId });
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<Branch> SeedBranchAsync(CancellationToken cancellationToken)
    {
        var branch = await _db.Branches.FirstOrDefaultAsync(b => b.Code == "MAIN", cancellationToken);
        if (branch is not null)
        {
            return branch;
        }

        branch = new Branch
        {
            Name = "Main Branch",
            Code = "MAIN",
            Address = "Main pharmacy location",
            Phone = "+10000000000",
            Email = "info@pharmacy.local",
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
        _db.Branches.Add(branch);
        await _db.SaveChangesAsync(cancellationToken);
        return branch;
    }

    private async Task SeedReferenceDataAsync(CancellationToken cancellationToken)
    {
        foreach (var name in new[] { "Analgesics", "Antibiotics", "Antihistamines", "Vitamins", "Cardiovascular", "Diabetes", "Dermatology", "Other" })
        {
            if (!await _db.MedicineCategories.AnyAsync(c => c.Name == name, cancellationToken))
            {
                _db.MedicineCategories.Add(new MedicineCategory { Name = name });
            }
        }

        foreach (var name in new[] { "Generic Pharma", "MedLife Ltd.", "HealthCare Pharmaceuticals", "Nova Drugs" })
        {
            if (!await _db.Manufacturers.AnyAsync(m => m.Name == name, cancellationToken))
            {
                _db.Manufacturers.Add(new Manufacturer { Name = name, Country = "Local" });
            }
        }

        var units = new[] { ("Tablet", "TAB"), ("Capsule", "CAP"), ("Bottle", "BTL"), ("Box", "BOX"), ("Tube", "TUBE"), ("Strip", "STR"), ("Piece", "PC") };
        foreach (var (name, abbreviation) in units)
        {
            if (!await _db.Units.AnyAsync(u => u.Name == name, cancellationToken))
            {
                _db.Units.Add(new Unit { Name = name, Abbreviation = abbreviation });
            }
        }

        foreach (var name in new[] { "Rent", "Utilities", "Salaries", "Transportation", "Maintenance", "Supplies", "Other" })
        {
            if (!await _db.ExpenseCategories.AnyAsync(c => c.Name == name, cancellationToken))
            {
                _db.ExpenseCategories.Add(new ExpenseCategory { Name = name });
            }
        }

        var settings = new (string Key, string Value, string Description, string Group)[]
        {
            ("Pharmacy.Name", "Enterprise Pharmacy", "Pharmacy name printed on invoices and reports", "General"),
            ("Pharmacy.Currency", "USD", "Currency symbol/code used for financial displays", "General"),
            ("Pharmacy.Address", "Main pharmacy address", "Address printed on invoices", "General"),
            ("Pharmacy.Phone", "+10000000000", "Contact number printed on invoices", "General"),
            ("Invoice.Footer", "Thank you for your business.", "Footer text printed on sales invoices", "Invoice"),
            ("Inventory.ExpiryAlertDays", "90", "Number of days used for expiring medicine alerts", "Inventory"),
            ("Inventory.AllowNegativeStock", "false", "Whether stock may go negative (must remain false)", "Inventory"),
            ("Sales.DefaultTaxPercentage", "0", "Default tax percentage for new medicines", "Sales"),
            ("Reports.DefaultPeriodDays", "30", "Default reporting period in days", "Reports")
        };

        foreach (var (key, value, description, group) in settings)
        {
            if (!await _db.SystemSettings.AnyAsync(s => s.Key == key, cancellationToken))
            {
                _db.SystemSettings.Add(new SystemSetting
                {
                    Key = key,
                    Value = value,
                    Description = description,
                    Group = group,
                    UpdatedAtUtc = DateTime.UtcNow
                });
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedAdminAsync(int branchId, CancellationToken cancellationToken)
    {
        var email = _configuration["Seed:AdminEmail"] ?? "admin@pharmacy.local";
        var password = _configuration["Seed:AdminPassword"] ?? "Admin@12345";
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                NormalizedUserName = email.ToUpperInvariant(),
                Email = email,
                NormalizedEmail = email.ToUpperInvariant(),
                EmailConfirmed = true,
                FirstName = "System",
                LastName = "Administrator",
                BranchId = branchId,
                IsActive = true,
                LockoutEnabled = true,
                CreatedAtUtc = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    "Unable to create the initial administrator: " + string.Join("; ", result.Errors.Select(e => e.Description)));
            }
        }

        if (!await _userManager.IsInRoleAsync(user, Roles.SuperAdmin))
        {
            await _userManager.AddToRoleAsync(user, Roles.SuperAdmin);
        }
    }

    private async Task SeedSampleInventoryAsync(int branchId, CancellationToken cancellationToken)
    {
        if (await _db.Medicines.IgnoreQueryFilters().AnyAsync(cancellationToken))
        {
            return;
        }

        var category = await _db.MedicineCategories.FirstAsync(cancellationToken);
        var manufacturer = await _db.Manufacturers.FirstAsync(cancellationToken);
        var unit = await _db.Units.FirstAsync(cancellationToken);

        var medicine = new Medicine
        {
            Name = "Paracetamol 500mg",
            GenericName = "Paracetamol",
            BrandName = "Generic",
            CategoryId = category.Id,
            ManufacturerId = manufacturer.Id,
            UnitId = unit.Id,
            Sku = "MED-0001",
            Barcode = "8900000000017",
            Description = "Analgesic and antipyretic tablets",
            DosageForm = "Tablet",
            Strength = "500 mg",
            PurchasePrice = 1.20m,
            SellingPrice = 2.00m,
            TaxPercentage = 0,
            ReorderLevel = 20,
            RequiresPrescription = false,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.Medicines.Add(medicine);
        await _db.SaveChangesAsync(cancellationToken);

        _db.MedicineBatches.Add(new MedicineBatch
        {
            MedicineId = medicine.Id,
            BranchId = branchId,
            BatchNumber = "SEED-001",
            ManufacturingDate = DateTime.UtcNow.AddMonths(-6),
            ExpiryDate = DateTime.UtcNow.AddYears(2),
            PurchasePrice = 1.20m,
            SellingPrice = 2.00m,
            InitialQuantity = 100,
            CurrentQuantity = 100,
            ReceivedAtUtc = DateTime.UtcNow,
            IsActive = true
        });

        await _db.SaveChangesAsync(cancellationToken);
    }
}
