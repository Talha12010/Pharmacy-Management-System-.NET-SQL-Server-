using FluentValidation;
using PharmacyManagement.Application.DTOs.Purchasing;

namespace PharmacyManagement.Application.Validators;

public sealed class CreatePurchaseOrderRequestValidator : AbstractValidator<CreatePurchaseOrderRequest>
{
    public CreatePurchaseOrderRequestValidator()
    {
        RuleFor(x => x.SupplierId).GreaterThan(0);
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.DiscountAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Items).NotEmpty().WithMessage("At least one item is required.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.MedicineId).GreaterThan(0);
            item.RuleFor(x => x.OrderedQuantity).GreaterThan(0);
            item.RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
            item.RuleFor(x => x.TaxPercentage).InclusiveBetween(0, 100);
        });
    }
}

public sealed class ReceivePurchaseOrderRequestValidator : AbstractValidator<ReceivePurchaseOrderRequest>
{
    public ReceivePurchaseOrderRequestValidator()
    {
        RuleFor(x => x.Items).NotEmpty().WithMessage("At least one received item is required.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.PurchaseOrderDetailId).GreaterThan(0);
            item.RuleFor(x => x.Quantity).GreaterThan(0);
            item.RuleFor(x => x.BatchNumber).NotEmpty().MaximumLength(50);
            item.RuleFor(x => x.ExpiryDate)
                .GreaterThan(DateTime.UtcNow.Date).WithMessage("Expired stock cannot be received.")
                .GreaterThan(x => x.ManufacturingDate).WithMessage("Expiry date must be after the manufacturing date.");
            item.RuleFor(x => x.UnitCost).GreaterThanOrEqualTo(0).When(x => x.UnitCost.HasValue);
        });
    }
}
