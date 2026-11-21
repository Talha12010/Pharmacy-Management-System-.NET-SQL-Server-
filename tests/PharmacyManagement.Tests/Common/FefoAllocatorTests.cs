using PharmacyManagement.Application.Common;
using PharmacyManagement.Domain.Entities;
using PharmacyManagement.Domain.Exceptions;

namespace PharmacyManagement.Tests.Common;

public sealed class FefoAllocatorTests
{
    [Fact]
    public void Allocate_Uses_Earliest_Expiry_First()
    {
        var asOf = new DateTime(2026, 1, 1);
        var later = Batch(1, new DateTime(2026, 6, 1), new DateTime(2025, 12, 1), 10);
        var earlier = Batch(2, new DateTime(2026, 2, 1), new DateTime(2025, 12, 2), 8);

        var allocations = FefoAllocator.Allocate(new[] { later, earlier }, 12, asOf);

        Assert.Equal(2, allocations.Count);
        Assert.Equal(2, allocations[0].Batch.Id);
        Assert.Equal(8, allocations[0].Quantity);
        Assert.Equal(1, allocations[1].Batch.Id);
        Assert.Equal(4, allocations[1].Quantity);
    }

    [Fact]
    public void Allocate_Orders_Same_Expiry_By_Received_Date()
    {
        var asOf = new DateTime(2026, 1, 1);
        var receivedLater = Batch(1, new DateTime(2026, 6, 1), new DateTime(2025, 12, 5), 5);
        var receivedEarlier = Batch(2, new DateTime(2026, 6, 1), new DateTime(2025, 12, 1), 5);

        var allocations = FefoAllocator.Allocate(new[] { receivedLater, receivedEarlier }, 7, asOf);

        Assert.Equal(2, allocations[0].Batch.Id);
        Assert.Equal(5, allocations[0].Quantity);
        Assert.Equal(1, allocations[1].Batch.Id);
        Assert.Equal(2, allocations[1].Quantity);
    }

    [Fact]
    public void Allocate_Skips_Expired_Inactive_And_Empty_Batches()
    {
        var asOf = new DateTime(2026, 1, 1);
        var usable = Batch(1, new DateTime(2026, 6, 1), new DateTime(2025, 12, 1), 5);
        var expired = Batch(2, new DateTime(2025, 12, 31), new DateTime(2025, 1, 1), 100);
        var inactive = Batch(3, new DateTime(2026, 6, 1), new DateTime(2025, 12, 1), 100);
        inactive.IsActive = false;
        var empty = Batch(4, new DateTime(2026, 6, 1), new DateTime(2025, 12, 1), 0);

        var allocations = FefoAllocator.Allocate(new[] { expired, inactive, empty, usable }, 5, asOf);

        Assert.Single(allocations);
        Assert.Equal(1, allocations[0].Batch.Id);
    }

    [Fact]
    public void Allocate_Throws_When_Stock_Is_Insufficient()
    {
        var asOf = new DateTime(2026, 1, 1);
        var batch = Batch(1, new DateTime(2026, 6, 1), new DateTime(2025, 12, 1), 3);

        var exception = Assert.Throws<BusinessRuleException>(
            () => FefoAllocator.Allocate(new[] { batch }, 4, asOf));

        Assert.Contains("Insufficient stock", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Allocate_Throws_When_Quantity_Is_Not_Positive(int quantity)
    {
        var asOf = new DateTime(2026, 1, 1);
        var batch = Batch(1, new DateTime(2026, 6, 1), new DateTime(2025, 12, 1), 10);

        Assert.Throws<BusinessRuleException>(
            () => FefoAllocator.Allocate(new[] { batch }, quantity, asOf));
    }

    private static MedicineBatch Batch(
        int id, DateTime expiryDate, DateTime receivedAtUtc, int currentQuantity) => new()
        {
            Id = id,
            BatchNumber = $"B-{id}",
            ExpiryDate = expiryDate,
            ReceivedAtUtc = receivedAtUtc,
            CurrentQuantity = currentQuantity,
            InitialQuantity = Math.Max(currentQuantity, 1),
            IsActive = true
        };
}
