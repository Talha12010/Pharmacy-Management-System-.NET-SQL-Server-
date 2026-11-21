using System.Linq.Expressions;
using PharmacyManagement.Application.DTOs.Catalog;
using PharmacyManagement.Application.DTOs.Medicines;
using PharmacyManagement.Domain.Entities;

namespace PharmacyManagement.Application.Mappings;

public static class CatalogMappings
{
    public static CategoryDto ToDto(this MedicineCategory category, int medicineCount = 0) => new()
    {
        Id = category.Id,
        Name = category.Name,
        Description = category.Description,
        IsActive = category.IsActive,
        MedicineCount = medicineCount
    };

    public static ManufacturerDto ToDto(this Manufacturer manufacturer, int medicineCount = 0) => new()
    {
        Id = manufacturer.Id,
        Name = manufacturer.Name,
        Country = manufacturer.Country,
        ContactInfo = manufacturer.ContactInfo,
        IsActive = manufacturer.IsActive,
        MedicineCount = medicineCount
    };

    public static UnitDto ToDto(this Unit unit, int medicineCount = 0) => new()
    {
        Id = unit.Id,
        Name = unit.Name,
        Abbreviation = unit.Abbreviation,
        IsActive = unit.IsActive,
        MedicineCount = medicineCount
    };

    public static readonly Expression<Func<Medicine, MedicineListItemDto>> MedicineListProjection = m => new MedicineListItemDto
    {
        Id = m.Id,
        Name = m.Name,
        GenericName = m.GenericName,
        CategoryName = m.Category.Name,
        ManufacturerName = m.Manufacturer.Name,
        Sku = m.Sku,
        Barcode = m.Barcode,
        SellingPrice = m.SellingPrice,
        RequiresPrescription = m.RequiresPrescription,
        IsActive = m.IsActive,
        TotalStock = m.Batches
            .Where(b => b.IsActive && b.CurrentQuantity > 0 && b.ExpiryDate > DateTime.UtcNow)
            .Sum(b => (int?)b.CurrentQuantity) ?? 0,
        ReorderLevel = m.ReorderLevel
    };

    public static MedicineDto ToDto(this Medicine medicine) => new()
    {
        Id = medicine.Id,
        Name = medicine.Name,
        GenericName = medicine.GenericName,
        BrandName = medicine.BrandName,
        CategoryId = medicine.CategoryId,
        CategoryName = medicine.Category?.Name ?? string.Empty,
        ManufacturerId = medicine.ManufacturerId,
        ManufacturerName = medicine.Manufacturer?.Name ?? string.Empty,
        UnitId = medicine.UnitId,
        UnitName = medicine.Unit?.Name ?? string.Empty,
        Barcode = medicine.Barcode,
        Sku = medicine.Sku,
        Description = medicine.Description,
        DosageForm = medicine.DosageForm,
        Strength = medicine.Strength,
        PurchasePrice = medicine.PurchasePrice,
        SellingPrice = medicine.SellingPrice,
        TaxPercentage = medicine.TaxPercentage,
        ReorderLevel = medicine.ReorderLevel,
        RequiresPrescription = medicine.RequiresPrescription,
        IsActive = medicine.IsActive,
        CreatedAtUtc = medicine.CreatedAtUtc,
        TotalStock = medicine.Batches
            .Where(b => b.IsActive && b.CurrentQuantity > 0 && b.ExpiryDate > DateTime.UtcNow)
            .Sum(b => b.CurrentQuantity),
        BatchCount = medicine.Batches.Count(b => b.IsActive && b.CurrentQuantity > 0),
        NearestExpiryDate = medicine.Batches
            .Where(b => b.IsActive && b.CurrentQuantity > 0 && b.ExpiryDate > DateTime.UtcNow)
            .OrderBy(b => b.ExpiryDate)
            .Select(b => (DateTime?)b.ExpiryDate)
            .FirstOrDefault()
    };
}
