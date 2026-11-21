using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.Common.Interfaces;
using PharmacyManagement.Application.DTOs.Inventory;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Application.Mappings;
using PharmacyManagement.Domain.Entities;
using PharmacyManagement.Domain.Enums;
using PharmacyManagement.Domain.Exceptions;

namespace PharmacyManagement.Application.Services;

public sealed class StockTransferService : IStockTransferService
{
    private readonly IApplicationDbContext _db;
    private readonly IDocumentNumberService _documents;
    private readonly IAuditService _audit;
    private readonly ICurrentUserService _currentUser;

    public StockTransferService(
        IApplicationDbContext db,
        IDocumentNumberService documents,
        IAuditService audit,
        ICurrentUserService currentUser)
    {
        _db = db;
        _documents = documents;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<StockTransferDto>> GetPagedAsync(QueryParameters query, StockTransferStatus? status, CancellationToken cancellationToken = default)
    {
        var transfers = _db.StockTransfers.AsNoTracking();
        if (status.HasValue)
        {
            transfers = transfers.Where(t => t.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            transfers = transfers.Where(t =>
                t.TransferNumber.Contains(term) ||
                t.FromBranch.Name.Contains(term) ||
                t.ToBranch.Name.Contains(term));
        }

        var total = await transfers.CountAsync(cancellationToken);
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 200);
        var items = await transfers
            .OrderByDescending(t => t.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new StockTransferDto
            {
                Id = t.Id,
                TransferNumber = t.TransferNumber,
                FromBranchId = t.FromBranchId,
                FromBranchName = t.FromBranch.Name,
                ToBranchId = t.ToBranchId,
                ToBranchName = t.ToBranch.Name,
                Status = t.Status,
                RequestedByUserId = t.RequestedByUserId,
                ApprovedByUserId = t.ApprovedByUserId,
                TransferDateUtc = t.TransferDateUtc,
                CreatedAtUtc = t.CreatedAtUtc,
                Notes = t.Notes,
                Details = t.Details.Select(d => new StockTransferDetailDto
                {
                    Id = d.Id,
                    MedicineId = d.MedicineId,
                    MedicineName = d.Medicine.Name,
                    BatchId = d.BatchId,
                    BatchNumber = d.Batch.BatchNumber,
                    ExpiryDate = d.Batch.ExpiryDate,
                    Quantity = d.Quantity,
                    ReceivedQuantity = d.ReceivedQuantity
                }).ToList()
            })
            .ToListAsync(cancellationToken);

        return PagedResult<StockTransferDto>.Create(items, page, pageSize, total);
    }

    public async Task<StockTransferDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var transfer = await _db.StockTransfers.AsNoTracking()
            .Include(t => t.FromBranch)
            .Include(t => t.ToBranch)
            .Include(t => t.Details).ThenInclude(d => d.Medicine)
            .Include(t => t.Details).ThenInclude(d => d.Batch)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken)
            ?? throw new NotFoundException("StockTransfer", id);

        return transfer.ToDto();
    }

    public async Task<StockTransferDto> CreateAsync(CreateStockTransferRequest request, CancellationToken cancellationToken = default)
    {
        if (request.FromBranchId == request.ToBranchId)
        {
            throw new BusinessRuleException("Source and destination branches must differ.");
        }

        var branchIds = new[] { request.FromBranchId, request.ToBranchId };
        var existingBranches = await _db.Branches.Where(b => branchIds.Contains(b.Id)).CountAsync(cancellationToken);
        if (existingBranches != 2)
        {
            throw new NotFoundException("One or both branches were not found.");
        }

        if (request.Items.Count == 0)
        {
            throw new BusinessRuleException("At least one item is required for a stock transfer.");
        }

        if (request.Items.Select(i => i.BatchId).Distinct().Count() != request.Items.Count)
        {
            throw new BusinessRuleException("The same batch appears more than once in the transfer.");
        }

        var batchIds = request.Items.Select(i => i.BatchId).ToList();
        var batches = await _db.MedicineBatches
            .Include(b => b.Medicine)
            .Where(b => batchIds.Contains(b.Id))
            .ToDictionaryAsync(b => b.Id, cancellationToken);

        foreach (var item in request.Items)
        {
            if (!batches.TryGetValue(item.BatchId, out var batch))
            {
                throw new NotFoundException("MedicineBatch", item.BatchId);
            }

            if (batch.BranchId != request.FromBranchId)
            {
                throw new BusinessRuleException($"Batch '{batch.BatchNumber}' does not belong to the source branch.");
            }

            if (item.Quantity <= 0)
            {
                throw new BusinessRuleException("Transfer quantities must be greater than zero.");
            }

            if (item.Quantity > batch.CurrentQuantity)
            {
                throw new BusinessRuleException($"Only {batch.CurrentQuantity} units are available in batch '{batch.BatchNumber}'.");
            }

            if (batch.ExpiryDate.Date <= DateTime.UtcNow.Date)
            {
                throw new BusinessRuleException($"Batch '{batch.BatchNumber}' is expired and cannot be transferred.");
            }
        }

        var now = DateTime.UtcNow;
        var transfer = new StockTransfer
        {
            TransferNumber = await _documents.NextAsync(DocumentType.StockTransfer, now, cancellationToken),
            FromBranchId = request.FromBranchId,
            ToBranchId = request.ToBranchId,
            Status = StockTransferStatus.PendingApproval,
            RequestedByUserId = _currentUser.UserId,
            TransferDateUtc = now,
            CreatedAtUtc = now,
            Notes = request.Notes?.Trim()
        };

        foreach (var item in request.Items)
        {
            var batch = batches[item.BatchId];
            transfer.Details.Add(new StockTransferDetail
            {
                MedicineId = batch.MedicineId,
                BatchId = batch.Id,
                Quantity = item.Quantity,
                ReceivedQuantity = 0
            });
        }

        _db.StockTransfers.Add(transfer);
        await _db.SaveChangesAsync(cancellationToken);
        var dto = await GetByIdAsync(transfer.Id, cancellationToken);
        await _audit.LogAsync("StockTransferCreated", nameof(StockTransfer), transfer.Id.ToString(), null, dto, cancellationToken);
        return dto;
    }

    public async Task<StockTransferDto> ApproveAsync(int id, CancellationToken cancellationToken = default)
    {
        var transfer = await _db.StockTransfers.FirstOrDefaultAsync(t => t.Id == id, cancellationToken)
            ?? throw new NotFoundException("StockTransfer", id);

        if (transfer.Status is not (StockTransferStatus.Draft or StockTransferStatus.PendingApproval))
        {
            throw new BusinessRuleException($"Only draft or pending transfers can be approved. Current status: {transfer.Status}.");
        }

        transfer.Status = StockTransferStatus.Approved;
        transfer.ApprovedByUserId = _currentUser.UserId;
        await _db.SaveChangesAsync(cancellationToken);
        var dto = await GetByIdAsync(id, cancellationToken);
        await _audit.LogAsync("StockTransferApproved", nameof(StockTransfer), id.ToString(), null, new { Status = transfer.Status.ToString() }, cancellationToken);
        return dto;
    }

    public async Task<StockTransferDto> CompleteAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var transfer = await _db.StockTransfers
                .Include(t => t.Details)
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken)
                ?? throw new NotFoundException("StockTransfer", id);

            if (transfer.Status is not (StockTransferStatus.Approved or StockTransferStatus.InTransit))
            {
                throw new BusinessRuleException($"Only approved transfers can be completed. Current status: {transfer.Status}.");
            }

            var now = DateTime.UtcNow;
            foreach (var detail in transfer.Details)
            {
                var source = await _db.MedicineBatches.FirstOrDefaultAsync(b => b.Id == detail.BatchId, cancellationToken)
                    ?? throw new NotFoundException("MedicineBatch", detail.BatchId);

                if (source.BranchId != transfer.FromBranchId)
                {
                    throw new BusinessRuleException($"Batch '{source.BatchNumber}' is no longer in the source branch.");
                }

                if (source.CurrentQuantity < detail.Quantity)
                {
                    throw new BusinessRuleException($"Insufficient stock in batch '{source.BatchNumber}' for transfer {transfer.TransferNumber}.");
                }

                var destination = await _db.MedicineBatches.FirstOrDefaultAsync(b =>
                    b.BranchId == transfer.ToBranchId &&
                    b.MedicineId == source.MedicineId &&
                    b.BatchNumber == source.BatchNumber &&
                    b.ExpiryDate == source.ExpiryDate, cancellationToken);

                var beforeOut = source.CurrentQuantity;
                source.CurrentQuantity -= detail.Quantity;
                _db.InventoryTransactions.Add(new InventoryTransaction
                {
                    MedicineId = source.MedicineId,
                    Batch = source,
                    BatchId = source.Id,
                    BranchId = transfer.FromBranchId,
                    Type = InventoryTransactionType.TransferOut,
                    QuantityChange = -detail.Quantity,
                    QuantityBefore = beforeOut,
                    QuantityAfter = source.CurrentQuantity,
                    UnitCost = source.PurchasePrice,
                    ReferenceType = nameof(StockTransfer),
                    ReferenceId = transfer.Id,
                    Notes = $"Transfer {transfer.TransferNumber} to branch {transfer.ToBranchId}",
                    PerformedByUserId = _currentUser.UserId,
                    CreatedAtUtc = now
                });

                MedicineBatch destinationBatch;
                var beforeIn = 0;
                if (destination is not null)
                {
                    beforeIn = destination.CurrentQuantity;
                    destination.CurrentQuantity += detail.Quantity;
                    destination.IsActive = true;
                    destinationBatch = destination;
                }
                else
                {
                    destinationBatch = new MedicineBatch
                    {
                        MedicineId = source.MedicineId,
                        BranchId = transfer.ToBranchId,
                        SupplierId = source.SupplierId,
                        BatchNumber = source.BatchNumber,
                        ManufacturingDate = source.ManufacturingDate,
                        ExpiryDate = source.ExpiryDate,
                        PurchasePrice = source.PurchasePrice,
                        SellingPrice = source.SellingPrice,
                        InitialQuantity = detail.Quantity,
                        CurrentQuantity = detail.Quantity,
                        ReceivedAtUtc = now,
                        IsActive = true
                    };
                    _db.MedicineBatches.Add(destinationBatch);
                }

                _db.InventoryTransactions.Add(new InventoryTransaction
                {
                    MedicineId = source.MedicineId,
                    Batch = destinationBatch,
                    BatchId = destinationBatch.Id == 0 ? null : destinationBatch.Id,
                    BranchId = transfer.ToBranchId,
                    Type = InventoryTransactionType.TransferIn,
                    QuantityChange = detail.Quantity,
                    QuantityBefore = beforeIn,
                    QuantityAfter = beforeIn + detail.Quantity,
                    UnitCost = source.PurchasePrice,
                    ReferenceType = nameof(StockTransfer),
                    ReferenceId = transfer.Id,
                    Notes = $"Transfer {transfer.TransferNumber} from branch {transfer.FromBranchId}",
                    PerformedByUserId = _currentUser.UserId,
                    CreatedAtUtc = now
                });

                detail.ReceivedQuantity = detail.Quantity;
            }

            transfer.Status = StockTransferStatus.Completed;
            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            var dto = await GetByIdAsync(id, cancellationToken);
            await _audit.LogAsync("StockTransferCompleted", nameof(StockTransfer), id.ToString(), null, new { Status = transfer.Status.ToString() }, cancellationToken);
            return dto;
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new ConflictException("The stock changed while completing the transfer. Refresh and try again.");
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<StockTransferDto> CancelAsync(int id, CancellationToken cancellationToken = default)
    {
        var transfer = await _db.StockTransfers.FirstOrDefaultAsync(t => t.Id == id, cancellationToken)
            ?? throw new NotFoundException("StockTransfer", id);

        if (transfer.Status is not (StockTransferStatus.Draft or StockTransferStatus.PendingApproval or StockTransferStatus.Approved))
        {
            throw new BusinessRuleException($"Transfer with status {transfer.Status} cannot be cancelled.");
        }

        transfer.Status = StockTransferStatus.Cancelled;
        await _db.SaveChangesAsync(cancellationToken);
        var dto = await GetByIdAsync(id, cancellationToken);
        await _audit.LogAsync("StockTransferCancelled", nameof(StockTransfer), id.ToString(), null, new { Status = transfer.Status.ToString() }, cancellationToken);
        return dto;
    }
}
