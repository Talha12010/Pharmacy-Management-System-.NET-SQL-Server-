using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.Common.Interfaces;
using PharmacyManagement.Application.DTOs.Prescriptions;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Application.Mappings;
using PharmacyManagement.Domain.Entities;
using PharmacyManagement.Domain.Enums;
using PharmacyManagement.Domain.Exceptions;

namespace PharmacyManagement.Application.Services;

public sealed class PrescriptionService : IPrescriptionService
{
    private readonly IApplicationDbContext _db;
    private readonly IDocumentNumberService _documents;
    private readonly IFileStorageService _storage;
    private readonly IAuditService _audit;
    private readonly ICurrentUserService _currentUser;

    public PrescriptionService(
        IApplicationDbContext db,
        IDocumentNumberService documents,
        IFileStorageService storage,
        IAuditService audit,
        ICurrentUserService currentUser)
    {
        _db = db;
        _documents = documents;
        _storage = storage;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<PrescriptionDto>> GetPagedAsync(
        QueryParameters query, PrescriptionStatus? status, int? branchId, DateTime? from, DateTime? to,
        CancellationToken cancellationToken = default)
    {
        var prescriptions = _db.Prescriptions.AsNoTracking()
            .Include(p => p.Branch)
            .Include(p => p.Customer)
            .Include(p => p.Details).ThenInclude(d => d.Medicine)
            .AsQueryable();

        if (status.HasValue)
        {
            prescriptions = prescriptions.Where(p => p.Status == status);
        }

        if (branchId.HasValue)
        {
            prescriptions = prescriptions.Where(p => p.BranchId == branchId);
        }

        if (from.HasValue)
        {
            var fromUtc = from.Value.Date;
            prescriptions = prescriptions.Where(p => p.PrescriptionDate >= fromUtc);
        }

        if (to.HasValue)
        {
            var toUtc = to.Value.Date.AddDays(1);
            prescriptions = prescriptions.Where(p => p.PrescriptionDate < toUtc);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            prescriptions = prescriptions.Where(p =>
                p.PrescriptionNumber.Contains(term) ||
                p.PatientName.Contains(term) ||
                p.DoctorName.Contains(term) ||
                (p.Customer != null && (p.Customer.FirstName.Contains(term) || p.Customer.LastName.Contains(term))));
        }

        var total = await prescriptions.CountAsync(cancellationToken);
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 200);
        var items = await prescriptions
            .OrderByDescending(p => p.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<PrescriptionDto>.Create(items.Select(p => p.ToDto()).ToList(), page, pageSize, total);
    }

    public async Task<PrescriptionDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var prescription = await _db.Prescriptions.AsNoTracking()
            .Include(p => p.Branch)
            .Include(p => p.Customer)
            .Include(p => p.Details).ThenInclude(d => d.Medicine)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new NotFoundException("Prescription", id);

        return prescription.ToDto();
    }

    public async Task<PrescriptionDto> CreateAsync(CreatePrescriptionRequest request, CancellationToken cancellationToken = default)
    {
        var medicines = await ValidateRequestAsync(request, cancellationToken);
        var now = DateTime.UtcNow;

        var prescription = new Prescription
        {
            PrescriptionNumber = await _documents.NextAsync(DocumentType.Prescription, now, cancellationToken),
            BranchId = request.BranchId,
            CustomerId = request.CustomerId,
            PatientName = request.PatientName.Trim(),
            PatientPhone = request.PatientPhone?.Trim(),
            PatientAddress = request.PatientAddress?.Trim(),
            DoctorName = request.DoctorName.Trim(),
            DoctorLicenseNumber = request.DoctorLicenseNumber?.Trim(),
            PrescriptionDate = request.PrescriptionDate.Date,
            Status = PrescriptionStatus.Pending,
            Notes = request.Notes?.Trim(),
            CreatedByUserId = _currentUser.UserId ?? Guid.Empty,
            CreatedAtUtc = now
        };

        foreach (var item in request.Details)
        {
            prescription.Details.Add(new PrescriptionDetail
            {
                MedicineId = item.MedicineId,
                Dosage = item.Dosage.Trim(),
                Frequency = item.Frequency?.Trim(),
                DurationDays = item.DurationDays,
                Quantity = item.Quantity,
                Instructions = item.Instructions?.Trim()
            });
        }

        _db.Prescriptions.Add(prescription);
        await _db.SaveChangesAsync(cancellationToken);
        var dto = await GetByIdAsync(prescription.Id, cancellationToken);
        await _audit.LogAsync("PrescriptionCreated", nameof(Prescription), prescription.Id.ToString(), null, dto, cancellationToken);
        return dto;
    }

    public async Task<PrescriptionDto> UpdateAsync(int id, CreatePrescriptionRequest request, CancellationToken cancellationToken = default)
    {
        var prescription = await _db.Prescriptions
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new NotFoundException("Prescription", id);

        if (prescription.Status != PrescriptionStatus.Pending)
        {
            throw new BusinessRuleException($"Only pending prescriptions can be edited. Current status: {prescription.Status}.");
        }

        await ValidateRequestAsync(request, cancellationToken);
        var before = await GetByIdAsync(id, cancellationToken);

        prescription.BranchId = request.BranchId;
        prescription.CustomerId = request.CustomerId;
        prescription.PatientName = request.PatientName.Trim();
        prescription.PatientPhone = request.PatientPhone?.Trim();
        prescription.PatientAddress = request.PatientAddress?.Trim();
        prescription.DoctorName = request.DoctorName.Trim();
        prescription.DoctorLicenseNumber = request.DoctorLicenseNumber?.Trim();
        prescription.PrescriptionDate = request.PrescriptionDate.Date;
        prescription.Notes = request.Notes?.Trim();

        _db.PrescriptionDetails.RemoveRange(prescription.Details);
        prescription.Details.Clear();

        foreach (var item in request.Details)
        {
            prescription.Details.Add(new PrescriptionDetail
            {
                PrescriptionId = prescription.Id,
                MedicineId = item.MedicineId,
                Dosage = item.Dosage.Trim(),
                Frequency = item.Frequency?.Trim(),
                DurationDays = item.DurationDays,
                Quantity = item.Quantity,
                Instructions = item.Instructions?.Trim()
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
        var after = await GetByIdAsync(id, cancellationToken);
        await _audit.LogAsync("PrescriptionUpdated", nameof(Prescription), id.ToString(), before, after, cancellationToken);
        return after;
    }

    public async Task<PrescriptionDto> VerifyAsync(int id, CancellationToken cancellationToken = default)
    {
        var prescription = await _db.Prescriptions.FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new NotFoundException("Prescription", id);

        if (prescription.Status != PrescriptionStatus.Pending)
        {
            throw new BusinessRuleException($"Only pending prescriptions can be verified. Current status: {prescription.Status}.");
        }

        prescription.Status = PrescriptionStatus.Verified;
        prescription.VerifiedByUserId = _currentUser.UserId;
        prescription.VerifiedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("PrescriptionVerified", nameof(Prescription), id.ToString(), null, new { Status = prescription.Status.ToString() }, cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<PrescriptionDto> RejectAsync(int id, string reason, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new BusinessRuleException("A rejection reason is required.");
        }

        var prescription = await _db.Prescriptions.FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new NotFoundException("Prescription", id);

        if (prescription.Status != PrescriptionStatus.Pending)
        {
            throw new BusinessRuleException($"Only pending prescriptions can be rejected. Current status: {prescription.Status}.");
        }

        prescription.Status = PrescriptionStatus.Rejected;
        prescription.RejectionReason = reason.Trim();
        prescription.VerifiedByUserId = _currentUser.UserId;
        prescription.VerifiedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("PrescriptionRejected", nameof(Prescription), id.ToString(), null, new { Reason = prescription.RejectionReason }, cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<string> UploadImageAsync(int id, Stream content, string fileName, CancellationToken cancellationToken = default)
    {
        var prescription = await _db.Prescriptions.FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new NotFoundException("Prescription", id);

        if (prescription.Status == PrescriptionStatus.Cancelled)
        {
            throw new BusinessRuleException("Images cannot be uploaded for a cancelled prescription.");
        }

        var path = await _storage.SaveAsync(content, fileName, "prescriptions", cancellationToken);
        var previous = prescription.ImagePath;
        prescription.ImagePath = path;
        await _db.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(previous))
        {
            await _storage.DeleteAsync(previous, cancellationToken);
        }

        await _audit.LogAsync("PrescriptionImageUploaded", nameof(Prescription), id.ToString(), null, new { ImagePath = path }, cancellationToken);
        return path;
    }

    private async Task<Dictionary<int, Medicine>> ValidateRequestAsync(CreatePrescriptionRequest request, CancellationToken cancellationToken)
    {
        if (!await _db.Branches.AnyAsync(b => b.Id == request.BranchId, cancellationToken))
        {
            throw new NotFoundException("Branch", request.BranchId);
        }

        if (request.CustomerId.HasValue && !await _db.Customers.AnyAsync(c => c.Id == request.CustomerId, cancellationToken))
        {
            throw new NotFoundException("Customer", request.CustomerId);
        }

        if (request.Details.Count == 0)
        {
            throw new BusinessRuleException("At least one prescribed medicine is required.");
        }

        if (request.Details.Select(d => d.MedicineId).Distinct().Count() != request.Details.Count)
        {
            throw new BusinessRuleException("The same medicine appears more than once in the prescription.");
        }

        if (request.PrescriptionDate.Date > DateTime.UtcNow.Date.AddDays(1))
        {
            throw new BusinessRuleException("The prescription date cannot be in the future.");
        }

        var medicineIds = request.Details.Select(d => d.MedicineId).ToList();
        var medicines = await _db.Medicines.Where(m => medicineIds.Contains(m.Id)).ToDictionaryAsync(m => m.Id, cancellationToken);

        foreach (var item in request.Details)
        {
            if (!medicines.ContainsKey(item.MedicineId))
            {
                throw new NotFoundException("Medicine", item.MedicineId);
            }

            if (string.IsNullOrWhiteSpace(item.Dosage))
            {
                throw new BusinessRuleException("Dosage is required for every prescribed medicine.");
            }

            if (item.Quantity <= 0)
            {
                throw new BusinessRuleException("Prescribed quantity must be greater than zero.");
            }

            if (item.DurationDays is <= 0)
            {
                throw new BusinessRuleException("Duration must be greater than zero when provided.");
            }
        }

        return medicines;
    }
}
