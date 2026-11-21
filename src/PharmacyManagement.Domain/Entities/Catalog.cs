using PharmacyManagement.Domain.Common;

namespace PharmacyManagement.Domain.Entities;

public class MedicineCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<Medicine> Medicines { get; set; } = new List<Medicine>();
}

public class Manufacturer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Country { get; set; }
    public string? ContactInfo { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<Medicine> Medicines { get; set; } = new List<Medicine>();
}

public class Unit
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Abbreviation { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public ICollection<Medicine> Medicines { get; set; } = new List<Medicine>();
}

public class Medicine : SoftDeletableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? GenericName { get; set; }
    public string? BrandName { get; set; }
    public int CategoryId { get; set; }
    public MedicineCategory Category { get; set; } = null!;
    public int ManufacturerId { get; set; }
    public Manufacturer Manufacturer { get; set; } = null!;
    public int UnitId { get; set; }
    public Unit Unit { get; set; } = null!;
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
    public bool IsActive { get; set; } = true;

    public ICollection<MedicineBatch> Batches { get; set; } = new List<MedicineBatch>();
    public ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();
}
