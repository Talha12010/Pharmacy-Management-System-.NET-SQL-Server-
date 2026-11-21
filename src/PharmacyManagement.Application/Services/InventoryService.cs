using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.Common.Interfaces;
using PharmacyManagement.Application.DTOs.Dashboard;
using PharmacyManagement.Application.DTOs.Inventory;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Application.Mappings;
using PharmacyManagement.Domain.Entities;
using PharmacyManagement.Domain.Enums;
using PharmacyManagement.Domain.Exceptions;

namespace PharmacyManagement.Application.Services;

public sealed class InventoryService : IInventoryService
{
    private readonly IApplicationDbContext _db;
    private readonly IAuditService _audit;
    private readonly ICurrentUserService _currentUser;

    public InventoryService(IApplicationDbContext db, IAuditService audit, ICurrentUserService currentUser)
    {
        _db = db;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<BatchDto>> GetBatchesAsync(
        QueryParameters query, int? medicineId, int? branchId, bool expiringOnly, bool expiredOnly, int days,
        CancellationToken cancellationToken = default)
    {
        var batches = _db.MedicineBatches.AsNoTracking()
            .Include(b => b.Medicine)
            .Include(b => b.Branch)
            .Include(b => b.Supplier)
            .AsQueryable();

        var today = DateTime.UtcNow.Date;

        if (medicineId.HasValue)
        {
            batches = batches.Where(b => b.MedicineId == medicineId);
        }

        if (branchId.HasValue)
        {
            batches = batches.Where(b => b.BranchId == branchId);
        }

        if (expiredOnly)
        {
            batches = batches.Where(b => b.ExpiryDate <= today);
        }
        else if (expiringOnly)
        {
            var cutoff = today.AddDays(Math.Clamp(days, 1, 3650));
            batches = batches.Where(b => b.ExpiryDate > today && b.ExpiryDate <= cutoff);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            batches = batches.Where(b => b.Medicine.Name.Contains(term) || b.BatchNumber.Contains(term));
        }

        var total = await batches.CountAsync(cancellationToken);
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 200);
        var items = await batches
            .OrderBy(b => b.ExpiryDate)
            .ThenBy(b => b.Medicine.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<BatchDto>.Create(items.Select(b => b.ToDto()).ToList(), page, pageSize, total);
    }

    public async Task<PagedResult<StockSummaryDto>> GetStockAsync(
        QueryParameters query, int? branchId, bool lowStockOnly, CancellationToken cancellationToken = default)
    {
        var grouped = await _db.MedicineBatches.AsNoTracking()
            .Where(b => b.CurrentQuantity > 0 && (!branchId.HasValue || b.BranchId == branchId))
            .GroupBy(b => new { b.MedicineId, b.BranchId })
            .Select(g => new
            {
                g.Key.MedicineId,
                g.Key.BranchId,
                TotalQuantity = g.Sum(x => x.CurrentQuantity),
                NearestExpiryDate = g.Min(x => (DateTime?)x.ExpiryDate)
            })
            .ToListAsync(cancellationToken);

        var medicineIds = grouped.Select(g => g.MedicineId).Distinct().ToList();
        var medicines = await _db.Medicines.AsNoTracking()
            .Include(m => m.Category)
            .Include(m => m.Unit)
            .Where(m => medicineIds.Contains(m.Id))
            .ToDictionaryAsync(m => m.Id, cancellationToken);

        var branchIds = grouped.Select(g => g.BranchId).Distinct().ToList();
        var branches = await _db.Branches.AsNoTracking()
            .Where(b => branchIds.Contains(b.Id))
            .ToDictionaryAsync(b => b.Id, b => b.Name, cancellationToken);

        IEnumerable<StockSummaryDto> rows = grouped
            .Where(g => medicines.ContainsKey(g.MedicineId))
            .Select(g =>
            {
                var medicine = medicines[g.MedicineId];
                return new StockSummaryDto
                {
                    MedicineId = g.MedicineId,
                    MedicineName = medicine.Name,
                    CategoryName = medicine.Category?.Name ?? string.Empty,
                    BranchId = g.BranchId,
                    BranchName = branches.GetValueOrDefault(g.BranchId, string.Empty),
                    UnitAbbreviation = medicine.Unit?.Abbreviation ?? string.Empty,
                    TotalQuantity = g.TotalQuantity,
                    ReorderLevel = medicine.ReorderLevel,
                    IsLowStock = medicine.ReorderLevel > 0 && g.TotalQuantity <= medicine.ReorderLevel,
                    NearestExpiryDate = g.NearestExpiryDate
                };
            });

        if (lowStockOnly)
        {
            rows = rows.Where(r => r.IsLowStock);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            rows = rows.Where(r => r.MedicineName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                                   r.CategoryName.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        rows = query.SortBy?.ToLowerInvariant() switch
        {
            "quantity" => query.SortDesc ? rows.OrderByDescending(r => r.TotalQuantity) : rows.OrderBy(r => r.TotalQuantity),
            "expiry" => query.SortDesc ? rows.OrderByDescending(r => r.NearestExpiryDate) : rows.OrderBy(r => r.NearestExpiryDate),
            _ => query.SortDesc ? rows.OrderByDescending(r => r.MedicineName) : rows.OrderBy(r => r.MedicineName)
        };

        var materialized = rows.ToList();
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 200);
        var items = materialized.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return PagedResult<StockSummaryDto>.Create(items, page, pageSize, materialized.Count);
    }

    public async Task<PagedResult<InventoryTransactionDto>> GetMovementsAsync(
        QueryParameters query, int? medicineId, int? branchId, InventoryTransactionType? type,
        DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var movements = _db.InventoryTransactions.AsNoTracking()
            .Include(t => t.Medicine)
            .Include(t => t.Batch)
            .Include(t => t.Branch)
            .AsQueryable();

        if (medicineId.HasValue)
        {
            movements = movements.Where(t => t.MedicineId == medicineId);
        }

        if (branchId.HasValue)
        {
            movements = movements.Where(t => t.BranchId == branchId);
        }

        if (type.HasValue)
        {
            movements = movements.Where(t => t.Type == type);
        }

        if (from.HasValue)
        {
            var fromUtc = from.Value.Date;
            movements = movements.Where(t => t.CreatedAtUtc >= fromUtc);
        }

        if (to.HasValue)
        {
            var toUtc = to.Value.Date.AddDays(1);
            movements = movements.Where(t => t.CreatedAtUtc < toUtc);
        }

        var total = await movements.CountAsync(cancellationToken);
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 200);
        var items = await movements
            .OrderByDescending(t => t.CreatedAtUtc)
            .ThenByDescending(t => t.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<InventoryTransactionDto>.Create(items.Select(t => t.ToDto()).ToList(), page, pageSize, total);
    }

    public async Task<BatchDto> StockInAsync(StockInRequest request, CancellationToken cancellationToken = default)
    {
        var medicine = await _db.Medicines.FirstOrDefaultAsync(m => m.Id == request.MedicineId, cancellationToken)
            ?? throw new NotFoundException("Medicine", request.MedicineId);

        if (!await _db.Branches.AnyAsync(b => b.Id == request.BranchId, cancellationToken))
        {
            throw new NotFoundException("Branch", request.BranchId);
        }

        if (request.SupplierId.HasValue && !await _db.Suppliers.AnyAsync(s => s.Id == request.SupplierId, cancellationToken))
        {
            throw new NotFoundException("Supplier", request.SupplierId);
        }

        if (request.Quantity <= 0)
        {
            throw new BusinessRuleException("Quantity must be greater than zero.");
        }

        if (request.ExpiryDate.Date <= DateTime.UtcNow.Date)
        {
            throw new BusinessRuleException("Expired stock cannot be received.");
        }

        var batchNumber = request.BatchNumber.Trim();
        var existing = await _db.MedicineBatches
            .FirstOrDefaultAsync(b => b.MedicineId == request.MedicineId &&
                                      b.BranchId == request.BranchId &&
                                      b.BatchNumber == batchNumber, cancellationToken);

        if (existing is not null)
        {
            if (existing.ExpiryDate.Date != request.ExpiryDate.Date)
            {
                throw new ConflictException($"Batch '{batchNumber}' already exists with a different expiry date.");
            }

            var before = existing.CurrentQuantity;
            existing.CurrentQuantity += request.Quantity;
            existing.IsActive = true;
            if (request.PurchasePrice > 0)
            {
                existing.PurchasePrice = request.PurchasePrice;
            }

            if (request.SellingPrice > 0)
            {
                existing.SellingPrice = request.SellingPrice;
            }

            AddTransaction(existing, InventoryTransactionType.StockIn, request.Quantity, before, existing.CurrentQuantity,
                existing.PurchasePrice, "Manual", null, request.Notes);
            await _db.SaveChangesAsync(cancellationToken);
            var dto = await LoadBatchDtoAsync(existing.Id, cancellationToken);
            await _audit.LogAsync("StockIn", nameof(MedicineBatch), existing.Id.ToString(), null,
                new { medicine.Name, Quantity = request.Quantity, existing.CurrentQuantity }, cancellationToken);
            return dto;
        }

        var batch = new MedicineBatch
        {
            MedicineId = request.MedicineId,
            BranchId = request.BranchId,
            SupplierId = request.SupplierId,
            BatchNumber = batchNumber,
            ManufacturingDate = request.ManufacturingDate.Date,
            ExpiryDate = request.ExpiryDate.Date,
            PurchasePrice = request.PurchasePrice,
            SellingPrice = request.SellingPrice,
            InitialQuantity = request.Quantity,
            CurrentQuantity = request.Quantity,
            ReceivedAtUtc = DateTime.UtcNow,
            IsActive = true
        };

        _db.MedicineBatches.Add(batch);
        AddTransaction(batch, InventoryTransactionType.StockIn, request.Quantity, 0, request.Quantity,
            request.PurchasePrice, "Manual", null, request.Notes);
        await _db.SaveChangesAsync(cancellationToken);
        var created = await LoadBatchDtoAsync(batch.Id, cancellationToken);
        await _audit.LogAsync("StockIn", nameof(MedicineBatch), batch.Id.ToString(), null,
            new { medicine.Name, batch.BatchNumber, Quantity = request.Quantity }, cancellationToken);
        return created;
    }

    public async Task<BatchDto> StockOutAsync(StockOutRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Type is not (InventoryTransactionType.StockOut or InventoryTransactionType.Damaged or InventoryTransactionType.Expired))
        {
            throw new BusinessRuleException("Invalid stock-out type. Use StockOut, Damaged or Expired.");
        }

        var batch = await _db.MedicineBatches.FirstOrDefaultAsync(b => b.Id == request.BatchId, cancellationToken)
            ?? throw new NotFoundException("MedicineBatch", request.BatchId);

        if (request.Quantity <= 0)
        {
            throw new BusinessRuleException("Quantity must be greater than zero.");
        }

        if (request.Quantity > batch.CurrentQuantity)
        {
            throw new BusinessRuleException($"Only {batch.CurrentQuantity} units are available in batch {batch.BatchNumber}.");
        }

        if (request.Type == InventoryTransactionType.StockOut && batch.ExpiryDate.Date <= DateTime.UtcNow.Date)
        {
            throw new BusinessRuleException("Expired stock cannot be issued. Use the Expired stock-out type to dispose of it.");
        }

        var before = batch.CurrentQuantity;
        batch.CurrentQuantity -= request.Quantity;
        AddTransaction(batch, request.Type, -request.Quantity, before, batch.CurrentQuantity,
            batch.PurchasePrice, "Manual", null, request.Reason);
        await _db.SaveChangesAsync(cancellationToken);
        var dto = await LoadBatchDtoAsync(batch.Id, cancellationToken);
        await _audit.LogAsync("StockOut", nameof(MedicineBatch), batch.Id.ToString(), null,
            new { batch.BatchNumber, Quantity = request.Quantity, Reason = request.Reason, Type = request.Type.ToString() }, cancellationToken);
        return dto;
    }

    public async Task<BatchDto> AdjustAsync(StockAdjustmentRequest request, CancellationToken cancellationToken = default)
    {
        var batch = await _db.MedicineBatches.FirstOrDefaultAsync(b => b.Id == request.BatchId, cancellationToken)
            ?? throw new NotFoundException("MedicineBatch", request.BatchId);

        if (request.NewQuantity < 0)
        {
            throw new BusinessRuleException("New quantity cannot be negative.");
        }

        var delta = request.NewQuantity - batch.CurrentQuantity;
        if (delta == 0)
        {
            throw new BusinessRuleException("The new quantity matches the current quantity; no adjustment is needed.");
        }

        var before = batch.CurrentQuantity;
        batch.CurrentQuantity = request.NewQuantity;
        AddTransaction(batch,
            delta > 0 ? InventoryTransactionType.AdjustmentIncrease : InventoryTransactionType.AdjustmentDecrease,
            delta, before, batch.CurrentQuantity, batch.PurchasePrice, "Adjustment", null, request.Reason);
        await _db.SaveChangesAsync(cancellationToken);
        var dto = await LoadBatchDtoAsync(batch.Id, cancellationToken);
        await _audit.LogAsync("StockAdjusted", nameof(MedicineBatch), batch.Id.ToString(),
            new { Quantity = before }, new { Quantity = request.NewQuantity, Reason = request.Reason }, cancellationToken);
        return dto;
    }

    public async Task<IReadOnlyList<LowStockItemDto>> GetLowStockAsync(int? branchId, int limit = 50, CancellationToken cancellationToken = default)
    {
        var stock = await GetStockAsync(
            new QueryParameters { Page = 1, PageSize = Math.Clamp(limit, 1, 200), SortBy = "quantity", SortDesc = false },
            branchId, true, cancellationToken);

        return stock.Items.Select(s => new LowStockItemDto
        {
            MedicineId = s.MedicineId,
            MedicineName = s.MedicineName,
            BranchId = s.BranchId,
            BranchName = s.BranchName,
            Quantity = s.TotalQuantity,
            ReorderLevel = s.ReorderLevel
        }).ToList();
    }

    public async Task<IReadOnlyList<ExpiringItemDto>> GetExpiringAsync(int days, int? branchId, int limit = 50, CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var cutoff = today.AddDays(Math.Clamp(days, 1, 3650));
        var batches = await _db.MedicineBatches.AsNoTracking()
            .Include(b => b.Medicine)
            .Include(b => b.Branch)
            .Where(b => b.CurrentQuantity > 0 && b.ExpiryDate > today && b.ExpiryDate <= cutoff &&
                        (!branchId.HasValue || b.BranchId == branchId))
            .OrderBy(b => b.ExpiryDate)
            .Take(Math.Clamp(limit, 1, 500))
            .ToListAsync(cancellationToken);

        return batches.Select(ToExpiringItem).ToList();
    }

    public async Task<IReadOnlyList<ExpiringItemDto>> GetExpiredAsync(int? branchId, int limit = 50, CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var batches = await _db.MedicineBatches.AsNoTracking()
            .Include(b => b.Medicine)
            .Include(b => b.Branch)
            .Where(b => b.CurrentQuantity > 0 && b.ExpiryDate <= today &&
                        (!branchId.HasValue || b.BranchId == branchId))
            .OrderByDescending(b => b.ExpiryDate)
            .Take(Math.Clamp(limit, 1, 500))
            .ToListAsync(cancellationToken);

        return batches.Select(ToExpiringItem).ToList();
    }

    private static ExpiringItemDto ToExpiringItem(MedicineBatch batch) => new()
    {
        BatchId = batch.Id,
        MedicineName = batch.Medicine?.Name ?? string.Empty,
        BatchNumber = batch.BatchNumber,
        BranchId = batch.BranchId,
        BranchName = batch.Branch?.Name ?? string.Empty,
        ExpiryDate = batch.ExpiryDate,
        Quantity = batch.CurrentQuantity,
        DaysToExpiry = (batch.ExpiryDate.Date - DateTime.UtcNow.Date).Days
    };

    private void AddTransaction(MedicineBatch batch, InventoryTransactionType type, int quantityChange,
        int before, int after, decimal unitCost, string referenceType, int? referenceId, string? notes)
    {
        _db.InventoryTransactions.Add(new InventoryTransaction
        {
            MedicineId = batch.MedicineId,
            Batch = batch,
            BatchId = batch.Id == 0 ? null : batch.Id,
            BranchId = batch.BranchId,
            Type = type,
            QuantityChange = quantityChange,
            QuantityBefore = before,
            QuantityAfter = after,
            UnitCost = unitCost,
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            Notes = notes,
            PerformedByUserId = _currentUser.UserId,
            CreatedAtUtc = DateTime.UtcNow
        });
    }

    private async Task<BatchDto> LoadBatchDtoAsync(int id, CancellationToken cancellationToken)
    {
        var batch = await _db.MedicineBatches.AsNoTracking()
            .Include(b => b.Medicine)
            .Include(b => b.Branch)
            .Include(b => b.Supplier)
            .FirstAsync(b => b.Id == id, cancellationToken);
        return batch.ToDto();
    }
}
