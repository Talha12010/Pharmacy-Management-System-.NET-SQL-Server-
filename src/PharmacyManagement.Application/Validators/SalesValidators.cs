using FluentValidation;
using PharmacyManagement.Application.DTOs.Prescriptions;
using PharmacyManagement.Application.DTOs.Returns;
using PharmacyManagement.Application.DTOs.Sales;

namespace PharmacyManagement.Application.Validators;

public sealed class CreateSaleRequestValidator : AbstractValidator<CreateSaleRequest>
{
    public CreateSaleRequestValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.DiscountAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Items).NotEmpty().WithMessage("The cart is empty.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.MedicineId).GreaterThan(0);
            item.RuleFor(x => x.Quantity).GreaterThan(0);
            item.RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0).When(x => x.UnitPrice.HasValue);
            item.RuleFor(x => x.DiscountAmount).GreaterThanOrEqualTo(0);
        });
        RuleForEach(x => x.Payments).ChildRules(payment =>
        {
            payment.RuleFor(x => x.Amount).GreaterThan(0);
            payment.RuleFor(x => x.Method).IsInEnum();
        });
    }
}

public sealed class CancelSaleRequestValidator : AbstractValidator<CancelSaleRequest>
{
    public CancelSaleRequestValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}

public sealed class RecordSalePaymentRequestValidator : AbstractValidator<RecordSalePaymentRequest>
{
    public RecordSalePaymentRequestValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Method).IsInEnum();
    }
}

public sealed class CreatePrescriptionRequestValidator : AbstractValidator<CreatePrescriptionRequest>
{
    public CreatePrescriptionRequestValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.PatientName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DoctorName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PrescriptionDate).NotEmpty().LessThanOrEqualTo(DateTime.UtcNow.Date.AddDays(1));
        RuleFor(x => x.Details).NotEmpty().WithMessage("At least one prescribed medicine is required.");
        RuleForEach(x => x.Details).ChildRules(item =>
        {
            item.RuleFor(x => x.MedicineId).GreaterThan(0);
            item.RuleFor(x => x.Dosage).NotEmpty().MaximumLength(100);
            item.RuleFor(x => x.Quantity).GreaterThan(0);
            item.RuleFor(x => x.DurationDays).GreaterThan(0).When(x => x.DurationDays.HasValue);
        });
    }
}

public sealed class CreateReturnRequestValidator : AbstractValidator<CreateReturnRequest>
{
    public CreateReturnRequestValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Items).NotEmpty().WithMessage("At least one returned item is required.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.MedicineId).GreaterThan(0);
            item.RuleFor(x => x.BatchId).GreaterThan(0);
            item.RuleFor(x => x.Quantity).GreaterThan(0);
            item.RuleFor(x => x.Condition).IsInEnum();
            item.RuleFor(x => x.RestockDecision).IsInEnum();
        });
        RuleFor(x => x)
            .Must(x => x.Type == Domain.Enums.ReturnType.SaleReturn
                ? x.OriginalSaleId is > 0
                : x.OriginalPurchaseOrderId is > 0)
            .WithMessage("The original sale or purchase order is required for the selected return type.");
    }
}

public sealed class ApproveReturnRequestValidator : AbstractValidator<ApproveReturnRequest>
{
    public ApproveReturnRequestValidator()
    {
        RuleForEach(x => x.Inspections!).ChildRules(item =>
        {
            item.RuleFor(x => x.ReturnDetailId).GreaterThan(0);
            item.RuleFor(x => x.Condition).IsInEnum();
            item.RuleFor(x => x.RestockDecision).IsInEnum();
        });
    }
}
