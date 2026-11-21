using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common.Interfaces;
using PharmacyManagement.Application.DTOs.Catalog;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Application.Mappings;
using PharmacyManagement.Domain.Entities;
using PharmacyManagement.Domain.Exceptions;

namespace PharmacyManagement.Application.Services;

public sealed class CatalogService : ICatalogService
{
    private readonly IApplicationDbContext _db;
    private readonly IAuditService _audit;

    public CatalogService(IApplicationDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = _db.MedicineCategories.AsNoTracking();
        if (!includeInactive)
        {
            query = query.Where(c => c.IsActive);
        }

        return await query
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive,
                MedicineCount = c.Medicines.Count()
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();
        if (await _db.MedicineCategories.AnyAsync(c => c.Name == name, cancellationToken))
        {
            throw new ConflictException($"Category '{name}' already exists.");
        }

        var category = new MedicineCategory
        {
            Name = name,
            Description = request.Description?.Trim(),
            IsActive = request.IsActive
        };

        _db.MedicineCategories.Add(category);
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("CategoryCreated", nameof(MedicineCategory), category.Id.ToString(), null, category.ToDto(), cancellationToken);
        return category.ToDto();
    }

    public async Task<CategoryDto> UpdateCategoryAsync(int id, UpdateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = await _db.MedicineCategories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new NotFoundException("MedicineCategory", id);

        var name = request.Name.Trim();
        if (await _db.MedicineCategories.AnyAsync(c => c.Id != id && c.Name == name, cancellationToken))
        {
            throw new ConflictException($"Category '{name}' already exists.");
        }

        var before = category.ToDto();
        category.Name = name;
        category.Description = request.Description?.Trim();
        category.IsActive = request.IsActive;
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("CategoryUpdated", nameof(MedicineCategory), id.ToString(), before, category.ToDto(), cancellationToken);
        return category.ToDto();
    }

    public async Task DeleteCategoryAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _db.MedicineCategories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new NotFoundException("MedicineCategory", id);

        if (await _db.Medicines.IgnoreQueryFilters().AnyAsync(m => m.CategoryId == id, cancellationToken))
        {
            throw new BusinessRuleException("This category is used by medicines and cannot be deleted. Deactivate it instead.");
        }

        _db.MedicineCategories.Remove(category);
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("CategoryDeleted", nameof(MedicineCategory), id.ToString(), category.ToDto(), null, cancellationToken);
    }

    public async Task<IReadOnlyList<ManufacturerDto>> GetManufacturersAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = _db.Manufacturers.AsNoTracking();
        if (!includeInactive)
        {
            query = query.Where(m => m.IsActive);
        }

        return await query
            .OrderBy(m => m.Name)
            .Select(m => new ManufacturerDto
            {
                Id = m.Id,
                Name = m.Name,
                Country = m.Country,
                ContactInfo = m.ContactInfo,
                IsActive = m.IsActive,
                MedicineCount = m.Medicines.Count()
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ManufacturerDto> CreateManufacturerAsync(CreateManufacturerRequest request, CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();
        if (await _db.Manufacturers.AnyAsync(m => m.Name == name, cancellationToken))
        {
            throw new ConflictException($"Manufacturer '{name}' already exists.");
        }

        var manufacturer = new Manufacturer
        {
            Name = name,
            Country = request.Country?.Trim(),
            ContactInfo = request.ContactInfo?.Trim(),
            IsActive = request.IsActive
        };

        _db.Manufacturers.Add(manufacturer);
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("ManufacturerCreated", nameof(Manufacturer), manufacturer.Id.ToString(), null, manufacturer.ToDto(), cancellationToken);
        return manufacturer.ToDto();
    }

    public async Task<ManufacturerDto> UpdateManufacturerAsync(int id, UpdateManufacturerRequest request, CancellationToken cancellationToken = default)
    {
        var manufacturer = await _db.Manufacturers.FirstOrDefaultAsync(m => m.Id == id, cancellationToken)
            ?? throw new NotFoundException("Manufacturer", id);

        var name = request.Name.Trim();
        if (await _db.Manufacturers.AnyAsync(m => m.Id != id && m.Name == name, cancellationToken))
        {
            throw new ConflictException($"Manufacturer '{name}' already exists.");
        }

        var before = manufacturer.ToDto();
        manufacturer.Name = name;
        manufacturer.Country = request.Country?.Trim();
        manufacturer.ContactInfo = request.ContactInfo?.Trim();
        manufacturer.IsActive = request.IsActive;
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("ManufacturerUpdated", nameof(Manufacturer), id.ToString(), before, manufacturer.ToDto(), cancellationToken);
        return manufacturer.ToDto();
    }

    public async Task DeleteManufacturerAsync(int id, CancellationToken cancellationToken = default)
    {
        var manufacturer = await _db.Manufacturers.FirstOrDefaultAsync(m => m.Id == id, cancellationToken)
            ?? throw new NotFoundException("Manufacturer", id);

        if (await _db.Medicines.IgnoreQueryFilters().AnyAsync(m => m.ManufacturerId == id, cancellationToken))
        {
            throw new BusinessRuleException("This manufacturer is used by medicines and cannot be deleted. Deactivate it instead.");
        }

        _db.Manufacturers.Remove(manufacturer);
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("ManufacturerDeleted", nameof(Manufacturer), id.ToString(), manufacturer.ToDto(), null, cancellationToken);
    }

    public async Task<IReadOnlyList<UnitDto>> GetUnitsAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = _db.Units.AsNoTracking();
        if (!includeInactive)
        {
            query = query.Where(u => u.IsActive);
        }

        return await query
            .OrderBy(u => u.Name)
            .Select(u => new UnitDto
            {
                Id = u.Id,
                Name = u.Name,
                Abbreviation = u.Abbreviation,
                IsActive = u.IsActive,
                MedicineCount = u.Medicines.Count()
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<UnitDto> CreateUnitAsync(CreateUnitRequest request, CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();
        var abbreviation = request.Abbreviation.Trim();
        if (await _db.Units.AnyAsync(u => u.Name == name || u.Abbreviation == abbreviation, cancellationToken))
        {
            throw new ConflictException($"A unit with the same name or abbreviation already exists.");
        }

        var unit = new Unit { Name = name, Abbreviation = abbreviation, IsActive = request.IsActive };
        _db.Units.Add(unit);
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("UnitCreated", nameof(Unit), unit.Id.ToString(), null, unit.ToDto(), cancellationToken);
        return unit.ToDto();
    }

    public async Task<UnitDto> UpdateUnitAsync(int id, UpdateUnitRequest request, CancellationToken cancellationToken = default)
    {
        var unit = await _db.Units.FirstOrDefaultAsync(u => u.Id == id, cancellationToken)
            ?? throw new NotFoundException("Unit", id);

        var name = request.Name.Trim();
        var abbreviation = request.Abbreviation.Trim();
        if (await _db.Units.AnyAsync(u => u.Id != id && (u.Name == name || u.Abbreviation == abbreviation), cancellationToken))
        {
            throw new ConflictException("A unit with the same name or abbreviation already exists.");
        }

        var before = unit.ToDto();
        unit.Name = name;
        unit.Abbreviation = abbreviation;
        unit.IsActive = request.IsActive;
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("UnitUpdated", nameof(Unit), id.ToString(), before, unit.ToDto(), cancellationToken);
        return unit.ToDto();
    }

    public async Task DeleteUnitAsync(int id, CancellationToken cancellationToken = default)
    {
        var unit = await _db.Units.FirstOrDefaultAsync(u => u.Id == id, cancellationToken)
            ?? throw new NotFoundException("Unit", id);

        if (await _db.Medicines.IgnoreQueryFilters().AnyAsync(m => m.UnitId == id, cancellationToken))
        {
            throw new BusinessRuleException("This unit is used by medicines and cannot be deleted. Deactivate it instead.");
        }

        _db.Units.Remove(unit);
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("UnitDeleted", nameof(Unit), id.ToString(), unit.ToDto(), null, cancellationToken);
    }
}
