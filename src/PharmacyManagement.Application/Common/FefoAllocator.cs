using PharmacyManagement.Domain.Entities;
using PharmacyManagement.Domain.Exceptions;

namespace PharmacyManagement.Application.Common;

public sealed record BatchAllocation(MedicineBatch Batch, int Quantity);

public static class FefoAllocator
{
    public static IReadOnlyList<BatchAllocation> Allocate(
        IEnumerable<MedicineBatch> batches, int requestedQuantity, DateTime asOfUtc)
    {
        if (requestedQuantity <= 0)
        {
            throw new BusinessRuleException("Quantity must be greater than zero.");
        }

        var eligible = batches
            .Where(b => b.IsActive
                        && b.CurrentQuantity > 0
                        && b.ExpiryDate.Date > asOfUtc.Date)
            .OrderBy(b => b.ExpiryDate)
            .ThenBy(b => b.ReceivedAtUtc)
            .ThenBy(b => b.Id)
            .ToList();

        var available = eligible.Sum(b => b.CurrentQuantity);
        if (available < requestedQuantity)
        {
            throw new BusinessRuleException(
                $"Insufficient stock. Available quantity is {available}, requested quantity is {requestedQuantity}.");
        }

        var result = new List<BatchAllocation>();
        var remaining = requestedQuantity;

        foreach (var batch in eligible)
        {
            var take = Math.Min(batch.CurrentQuantity, remaining);
            if (take <= 0)
            {
                continue;
            }

            result.Add(new BatchAllocation(batch, take));
            remaining -= take;

            if (remaining == 0)
            {
                break;
            }
        }

        return result;
    }
}
