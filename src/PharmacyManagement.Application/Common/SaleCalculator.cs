using PharmacyManagement.Domain.Exceptions;

namespace PharmacyManagement.Application.Common;

public sealed record SaleLineCalculation(
    decimal Gross,
    decimal Discount,
    decimal Taxable,
    decimal Tax,
    decimal LineTotal);

public sealed record SaleTotals(
    decimal SubTotal,
    decimal DiscountTotal,
    decimal TaxTotal,
    decimal Total);

public static class SaleCalculator
{
    public static SaleLineCalculation CalculateLine(
        int quantity, decimal unitPrice, decimal discountAmount, decimal taxPercentage)
    {
        if (quantity <= 0)
        {
            throw new BusinessRuleException("Sale quantity must be greater than zero.");
        }

        if (unitPrice < 0 || discountAmount < 0)
        {
            throw new BusinessRuleException("Price and discount cannot be negative.");
        }

        if (taxPercentage is < 0 or > 100)
        {
            throw new BusinessRuleException("Tax percentage must be between 0 and 100.");
        }

        var gross = Round(quantity * unitPrice);
        if (discountAmount > gross)
        {
            throw new BusinessRuleException("Line discount cannot exceed the line gross amount.");
        }

        var taxable = Round(gross - discountAmount);
        var tax = Round(taxable * taxPercentage / 100m);
        return new SaleLineCalculation(gross, Round(discountAmount), taxable, tax, Round(taxable + tax));
    }

    public static SaleTotals CalculateTotals(IEnumerable<SaleLineCalculation> lines, decimal invoiceDiscount)
    {
        if (invoiceDiscount < 0)
        {
            throw new BusinessRuleException("Invoice discount cannot be negative.");
        }

        var list = lines.ToList();
        var subTotal = Round(list.Sum(l => l.Gross));
        var lineDiscounts = Round(list.Sum(l => l.Discount));
        var tax = Round(list.Sum(l => l.Tax));
        var netBeforeInvoiceDiscount = Round(subTotal - lineDiscounts);

        if (invoiceDiscount > netBeforeInvoiceDiscount)
        {
            throw new BusinessRuleException("Invoice discount cannot exceed the net subtotal.");
        }

        var discountTotal = Round(lineDiscounts + invoiceDiscount);
        var total = Round(subTotal - discountTotal + tax);
        return new SaleTotals(subTotal, discountTotal, tax, total);
    }

    public static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
