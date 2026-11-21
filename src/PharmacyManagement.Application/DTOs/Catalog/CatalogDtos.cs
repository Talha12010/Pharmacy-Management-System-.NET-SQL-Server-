namespace PharmacyManagement.Application.DTOs.Catalog;

public sealed class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int MedicineCount { get; set; }
}

public class CreateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class UpdateCategoryRequest : CreateCategoryRequest
{
}

public sealed class ManufacturerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Country { get; set; }
    public string? ContactInfo { get; set; }
    public bool IsActive { get; set; }
    public int MedicineCount { get; set; }
}

public class CreateManufacturerRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Country { get; set; }
    public string? ContactInfo { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class UpdateManufacturerRequest : CreateManufacturerRequest
{
}

public sealed class UnitDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Abbreviation { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int MedicineCount { get; set; }
}

public class CreateUnitRequest
{
    public string Name { get; set; } = string.Empty;
    public string Abbreviation { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public sealed class UpdateUnitRequest : CreateUnitRequest
{
}
