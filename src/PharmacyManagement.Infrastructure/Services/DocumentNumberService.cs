using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common.Interfaces;
using PharmacyManagement.Domain.Enums;
using PharmacyManagement.Domain.Exceptions;

namespace PharmacyManagement.Infrastructure.Services;

public sealed class DocumentNumberService : IDocumentNumberService
{
    private readonly IApplicationDbContext _db;

    public DocumentNumberService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<string> NextAsync(DocumentType type, DateTime utcNow, CancellationToken cancellationToken = default)
    {
        var prefix = type switch
        {
            DocumentType.Sale => "SAL",
            DocumentType.PurchaseOrder => "PO",
            DocumentType.GoodsReceipt => "GRN",
            DocumentType.Return => "RET",
            DocumentType.StockTransfer => "ST",
            DocumentType.Prescription => "RX",
            DocumentType.Expense => "EXP",
            _ => "DOC"
        };

        var typeName = type.ToString();
        var periodKey = utcNow.ToString("yyyyMM");

        for (var attempt = 0; attempt < 5; attempt++)
        {
            var sequence = await _db.DocumentSequences
                .SingleOrDefaultAsync(s => s.DocumentType == typeName && s.PeriodKey == periodKey, cancellationToken);

            if (sequence is null)
            {
                sequence = new Domain.Entities.DocumentSequence
                {
                    DocumentType = typeName,
                    PeriodKey = periodKey,
                    LastNumber = 1
                };
                _db.DocumentSequences.Add(sequence);
            }
            else
            {
                sequence.LastNumber += 1;
            }

            try
            {
                await _db.SaveChangesAsync(cancellationToken);
                return $"{prefix}-{periodKey}-{sequence.LastNumber:D6}";
            }
            catch (DbUpdateConcurrencyException)
            {
                _db.DocumentSequences.Local.Clear();
                await Task.Delay(20 * (attempt + 1), cancellationToken);
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                _db.DocumentSequences.Local.Clear();
                await Task.Delay(20 * (attempt + 1), cancellationToken);
            }
        }

        throw new ConflictException($"Could not allocate a document number for {type}.");
    }

    private static bool IsUniqueViolation(DbUpdateException exception)
        => exception.InnerException?.Message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) == true;
}
