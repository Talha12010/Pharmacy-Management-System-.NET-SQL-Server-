namespace PharmacyManagement.Application.DTOs.Medicines;

public sealed class MedicineDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? GenericName { get; set; }
    public string? BrandName { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int ManufacturerId { get; set; }
    public string ManufacturerName { get; set; } = string.Empty;
    public int UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DosageForm { get; set; } = string.Empty;
    public string Strength { get; set; } = string.Empty;
    public decimal PurchasePrice { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal TaxPercentage { get; set; }
    public int ReorderLevel { get; set; }
    public bool RequiresPrescription { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public int TotalStock { get; set; }
    public int BatchCount { get; set; }
    public DateTime? NearestExpiryDate { get; set; }
}

public sealed class MedicineListItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? GenericName { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string ManufacturerName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public decimal SellingPrice { get; set; }
    public bool RequiresPrescription { get; set; }
    public bool IsActive { get; set; }
    public int TotalStock { get; set; }
    public int ReorderLevel { get; set; }
}

public class CreateMedicineRequest
{
    public string Name { get; set; } = string.Empty;
    public string? GenericName { get; set; }
    public string? BrandName { get; set; }
    public int CategoryId { get; set; }
    public int ManufacturerId { get; set; }
    public int UnitId { get; set; }
    public string? Barcode { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DosageForm { get; set; } = string.Empty;
    public string Strength { get; set; } = string.Empty;
    public decimal PurchasePrice { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal TaxPercentage { get; set; }
    public int ReorderLevel { get; set; }
    public bool RequiresPrescription { get; set; }
}

public sealed class UpdateMedicineRequest : CreateMedicineRequest
{
    public bool IsActive { get; set; } = true;
}

public sealed class MedicineLookupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? GenericName { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal TaxPercentage { get; set; }
    public bool RequiresPrescription { get; set; }
    public int AvailableStock { get; set; }
    public IReadOnlyList<BatchLookupDto> Batches { get; set; } = Array.Empty<BatchLookupDto>();
}

public sealed class BatchLookupDto
{
    public int Id { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public int AvailableQuantity { get; set; }
    public decimal SellingPrice { get; set; }
}
