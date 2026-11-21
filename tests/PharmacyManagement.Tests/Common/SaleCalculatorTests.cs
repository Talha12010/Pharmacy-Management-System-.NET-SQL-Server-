using PharmacyManagement.Application.Common;
using PharmacyManagement.Domain.Exceptions;

namespace PharmacyManagement.Tests.Common;

public sealed class SaleCalculatorTests
{
    [Fact]
    public void CalculateLine_Returns_Expected_Amounts()
    {
        var result = SaleCalculator.CalculateLine(2, 10m, 1m, 5m);

        Assert.Equal(20m, result.Gross);
        Assert.Equal(1m, result.Discount);
        Assert.Equal(19m, result.Taxable);
        Assert.Equal(0.95m, result.Tax);
        Assert.Equal(19.95m, result.LineTotal);
    }

    [Fact]
    public void CalculateTotals_Combines_Lines_And_Invoice_Discount()
    {
        var first = SaleCalculator.CalculateLine(2, 10m, 1m, 5m);
        var second = SaleCalculator.CalculateLine(1, 5m, 0m, 10m);

        var totals = SaleCalculator.CalculateTotals(new[] { first, second }, 2m);

        Assert.Equal(25m, totals.SubTotal);
        Assert.Equal(3m, totals.DiscountTotal);
        Assert.Equal(1.45m, totals.TaxTotal);
        Assert.Equal(23.45m, totals.Total);
    }

    public static TheoryData<int, decimal, decimal, decimal> InvalidLines => new()
    {
        { 0, 10m, 0m, 0m },
        { -1, 10m, 0m, 0m },
        { 1, -1m, 0m, 0m },
        { 1, 10m, -1m, 0m },
        { 1, 10m, 0m, -1m },
        { 1, 10m, 0m, 101m },
        { 1, 10m, 11m, 0m }
    };

    [Theory]
    [MemberData(nameof(InvalidLines))]
    public void CalculateLine_Rejects_Invalid_Values(
        int quantity, decimal unitPrice, decimal discount, decimal taxPercentage)
    {
        Assert.Throws<BusinessRuleException>(
            () => SaleCalculator.CalculateLine(quantity, unitPrice, discount, taxPercentage));
    }

    [Fact]
    public void CalculateTotals_Rejects_Invoice_Discount_Above_Net_Subtotal()
    {
        var line = SaleCalculator.CalculateLine(1, 10m, 0m, 0m);

        Assert.Throws<BusinessRuleException>(
            () => SaleCalculator.CalculateTotals(new[] { line }, 11m));
    }
}
