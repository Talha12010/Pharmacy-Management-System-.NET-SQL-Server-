using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.Common.Interfaces;
using PharmacyManagement.Application.DTOs.Medicines;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Application.Mappings;
using PharmacyManagement.Domain.Entities;
using PharmacyManagement.Domain.Exceptions;

namespace PharmacyManagement.Application.Services;

public sealed class MedicineService : IMedicineService
{
    private readonly IApplicationDbContext _db;
    private readonly IAuditService _audit;

    public MedicineService(IApplicationDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<PagedResult<MedicineListItemDto>> GetPagedAsync(
        QueryParameters query, int? categoryId, int? manufacturerId, bool? isActive,
        CancellationToken cancellationToken = default)
    {
        var medicines = _db.Medicines.AsNoTracking();

        if (categoryId.HasValue)
        {
            medicines = medicines.Where(m => m.CategoryId == categoryId);
        }

        if (manufacturerId.HasValue)
        {
            medicines = medicines.Where(m => m.ManufacturerId == manufacturerId);
        }

        if (isActive.HasValue)
        {
            medicines = medicines.Where(m => m.IsActive == isActive);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            medicines = medicines.Where(m =>
                m.Name.Contains(term) ||
                (m.GenericName != null && m.GenericName.Contains(term)) ||
                (m.BrandName != null && m.BrandName.Contains(term)) ||
                m.Sku.Contains(term) ||
                (m.Barcode != null && m.Barcode.Contains(term)));
        }

        var ordered = query.SortBy?.ToLowerInvariant() switch
        {
            "price" => query.SortDesc ? medicines.OrderByDescending(m => m.SellingPrice) : medicines.OrderBy(m => m.SellingPrice),
            "stock" => query.SortDesc ? medicines.OrderByDescending(m => m.Batches.Sum(b => b.CurrentQuantity)) : medicines.OrderBy(m => m.Batches.Sum(b => b.CurrentQuantity)),
            _ => query.SortDesc ? medicines.OrderByDescending(m => m.Name) : medicines.OrderBy(m => m.Name)
        };

        var projected = ordered.Select(CatalogMappings.MedicineListProjection);
        return await projected.ToPagedResultAsync(query.Page, query.PageSize, cancellationToken);
    }

    public async Task<MedicineDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var medicine = await _db.Medicines.AsNoTracking()
            .Include(m => m.Category)
            .Include(m => m.Manufacturer)
            .Include(m => m.Unit)
            .Include(m => m.Batches)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken)
            ?? throw new NotFoundException("Medicine", id);

        return medicine.ToDto();
    }

    public async Task<MedicineDto> GetByBarcodeAsync(string barcode, CancellationToken cancellationToken = default)
    {
        var value = barcode.Trim();
        var medicine = await _db.Medicines.AsNoTracking()
            .Include(m => m.Category)
            .Include(m => m.Manufacturer)
            .Include(m => m.Unit)
            .Include(m => m.Batches)
            .FirstOrDefaultAsync(m => m.Barcode == value && m.IsActive, cancellationToken)
            ?? throw new NotFoundException($"No active medicine was found for barcode '{value}'.");

        return medicine.ToDto();
    }

    public async Task<IReadOnlyList<MedicineLookupDto>> LookupAsync(
        string? search, int? branchId, int limit = 20, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var query = _db.Medicines.AsNoTracking()
            .Where(m => m.IsActive)
            .Include(m => m.Batches.Where(b =>
                b.IsActive && b.CurrentQuantity > 0 && b.ExpiryDate > now &&
                (!branchId.HasValue || b.BranchId == branchId.Value)))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(m =>
                m.Name.Contains(term) ||
                (m.GenericName != null && m.GenericName.Contains(term)) ||
                m.Sku.Contains(term) ||
                (m.Barcode != null && m.Barcode.Contains(term)));
        }

        var medicines = await query
            .OrderBy(m => m.Name)
            .Take(Math.Clamp(limit, 1, 50))
            .ToListAsync(cancellationToken);

        return medicines.Select(m => new MedicineLookupDto
        {
            Id = m.Id,
            Name = m.Name,
            GenericName = m.GenericName,
            Sku = m.Sku,
            Barcode = m.Barcode,
            SellingPrice = m.SellingPrice,
            TaxPercentage = m.TaxPercentage,
            RequiresPrescription = m.RequiresPrescription,
            AvailableStock = m.Batches.Sum(b => b.CurrentQuantity),
            Batches = m.Batches
                .Where(b => b.IsActive && b.CurrentQuantity > 0 && b.ExpiryDate > now)
                .OrderBy(b => b.ExpiryDate)
                .ThenBy(b => b.ReceivedAtUtc)
                .Select(b => new BatchLookupDto
                {
                    Id = b.Id,
                    BatchNumber = b.BatchNumber,
                    ExpiryDate = b.ExpiryDate,
                    AvailableQuantity = b.CurrentQuantity,
                    SellingPrice = b.SellingPrice
                })
                .ToList()
        }).ToList();
    }

    public async Task<MedicineDto> CreateAsync(CreateMedicineRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateCatalogReferencesAsync(request.CategoryId, request.ManufacturerId, request.UnitId, cancellationToken);

        var sku = request.Sku.Trim();
        if (await _db.Medicines.IgnoreQueryFilters().AnyAsync(m => m.Sku == sku, cancellationToken))
        {
            throw new ConflictException($"A medicine with SKU '{sku}' already exists.");
        }

        var barcode = string.IsNullOrWhiteSpace(request.Barcode) ? null : request.Barcode.Trim();
        if (barcode is not null && await _db.Medicines.AnyAsync(m => m.Barcode == barcode, cancellationToken))
        {
            throw new ConflictException($"A medicine with barcode '{barcode}' already exists.");
        }

        var medicine = new Medicine
        {
            Name = request.Name.Trim(),
            GenericName = request.GenericName?.Trim(),
            BrandName = request.BrandName?.Trim(),
            CategoryId = request.CategoryId,
            ManufacturerId = request.ManufacturerId,
            UnitId = request.UnitId,
            Barcode = barcode,
            Sku = sku,
            Description = request.Description?.Trim(),
            DosageForm = request.DosageForm.Trim(),
            Strength = request.Strength.Trim(),
            PurchasePrice = request.PurchasePrice,
            SellingPrice = request.SellingPrice,
            TaxPercentage = request.TaxPercentage,
            ReorderLevel = request.ReorderLevel,
            RequiresPrescription = request.RequiresPrescription,
            IsActive = true
        };

        _db.Medicines.Add(medicine);
        await _db.SaveChangesAsync(cancellationToken);
        var dto = await GetByIdAsync(medicine.Id, cancellationToken);
        await _audit.LogAsync("MedicineCreated", nameof(Medicine), medicine.Id.ToString(), null, dto, cancellationToken);
        return dto;
    }

    public async Task<MedicineDto> UpdateAsync(int id, UpdateMedicineRequest request, CancellationToken cancellationToken = default)
    {
        var medicine = await _db.Medicines.FirstOrDefaultAsync(m => m.Id == id, cancellationToken)
            ?? throw new NotFoundException("Medicine", id);

        await ValidateCatalogReferencesAsync(request.CategoryId, request.ManufacturerId, request.UnitId, cancellationToken);

        var sku = request.Sku.Trim();
        if (await _db.Medicines.IgnoreQueryFilters().AnyAsync(m => m.Id != id && m.Sku == sku, cancellationToken))
        {
            throw new ConflictException($"A medicine with SKU '{sku}' already exists.");
        }

        var barcode = string.IsNullOrWhiteSpace(request.Barcode) ? null : request.Barcode.Trim();
        if (barcode is not null && await _db.Medicines.AnyAsync(m => m.Id != id && m.Barcode == barcode, cancellationToken))
        {
            throw new ConflictException($"A medicine with barcode '{barcode}' already exists.");
        }

        var before = await GetByIdAsync(id, cancellationToken);
        medicine.Name = request.Name.Trim();
        medicine.GenericName = request.GenericName?.Trim();
        medicine.BrandName = request.BrandName?.Trim();
        medicine.CategoryId = request.CategoryId;
        medicine.ManufacturerId = request.ManufacturerId;
        medicine.UnitId = request.UnitId;
        medicine.Barcode = barcode;
        medicine.Sku = sku;
        medicine.Description = request.Description?.Trim();
        medicine.DosageForm = request.DosageForm.Trim();
        medicine.Strength = request.Strength.Trim();
        medicine.PurchasePrice = request.PurchasePrice;
        medicine.SellingPrice = request.SellingPrice;
        medicine.TaxPercentage = request.TaxPercentage;
        medicine.ReorderLevel = request.ReorderLevel;
        medicine.RequiresPrescription = request.RequiresPrescription;
        medicine.IsActive = request.IsActive;

        await _db.SaveChangesAsync(cancellationToken);
        var after = await GetByIdAsync(id, cancellationToken);
        await _audit.LogAsync("MedicineUpdated", nameof(Medicine), id.ToString(), before, after, cancellationToken);
        return after;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var medicine = await _db.Medicines.FirstOrDefaultAsync(m => m.Id == id, cancellationToken)
            ?? throw new NotFoundException("Medicine", id);

        medicine.IsDeleted = true;
        medicine.DeletedAtUtc = DateTime.UtcNow;
        medicine.IsActive = false;
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("MedicineDeleted", nameof(Medicine), id.ToString(), null, new { medicine.Name, medicine.Sku }, cancellationToken);
    }

    private async Task ValidateCatalogReferencesAsync(int categoryId, int manufacturerId, int unitId, CancellationToken cancellationToken)
    {
        if (!await _db.MedicineCategories.AnyAsync(c => c.Id == categoryId, cancellationToken))
        {
            throw new NotFoundException("MedicineCategory", categoryId);
        }

        if (!await _db.Manufacturers.AnyAsync(m => m.Id == manufacturerId, cancellationToken))
        {
            throw new NotFoundException("Manufacturer", manufacturerId);
        }

        if (!await _db.Units.AnyAsync(u => u.Id == unitId, cancellationToken))
        {
            throw new NotFoundException("Unit", unitId);
        }
    }
}
