using FluentValidation;
using PharmacyManagement.Application.DTOs.Inventory;

namespace PharmacyManagement.Application.Validators;

public sealed class StockInRequestValidator : AbstractValidator<StockInRequest>
{
    public StockInRequestValidator()
    {
        RuleFor(x => x.MedicineId).GreaterThan(0);
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.BatchNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.PurchasePrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SellingPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ManufacturingDate).NotEmpty();
        RuleFor(x => x.ExpiryDate)
            .NotEmpty()
            .GreaterThan(x => x.ManufacturingDate).WithMessage("Expiry date must be after the manufacturing date.")
            .GreaterThan(DateTime.UtcNow.Date).WithMessage("Expired stock cannot be added.");
    }
}

public sealed class StockOutRequestValidator : AbstractValidator<StockOutRequest>
{
    public StockOutRequestValidator()
    {
        RuleFor(x => x.BatchId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Type).IsInEnum();
    }
}

public sealed class StockAdjustmentRequestValidator : AbstractValidator<StockAdjustmentRequest>
{
    public StockAdjustmentRequestValidator()
    {
        RuleFor(x => x.BatchId).GreaterThan(0);
        RuleFor(x => x.NewQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}

public sealed class CreateStockTransferRequestValidator : AbstractValidator<CreateStockTransferRequest>
{
    public CreateStockTransferRequestValidator()
    {
        RuleFor(x => x.FromBranchId).GreaterThan(0);
        RuleFor(x => x.ToBranchId).GreaterThan(0)
            .NotEqual(x => x.FromBranchId).WithMessage("Source and destination branches must differ.");
        RuleFor(x => x.Items).NotEmpty().WithMessage("At least one item is required.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.BatchId).GreaterThan(0);
            item.RuleFor(x => x.Quantity).GreaterThan(0);
        });
    }
}
