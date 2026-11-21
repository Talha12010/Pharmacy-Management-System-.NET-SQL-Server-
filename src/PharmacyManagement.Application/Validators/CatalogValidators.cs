using FluentValidation;
using PharmacyManagement.Application.DTOs.Catalog;
using PharmacyManagement.Application.DTOs.Medicines;
using PharmacyManagement.Application.DTOs.Organization;

namespace PharmacyManagement.Application.Validators;

public sealed class CreateBranchRequestValidator : AbstractValidator<CreateBranchRequest>
{
    public CreateBranchRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Email).EmailAddress().MaximumLength(256).When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Phone).MaximumLength(30);
    }
}

public sealed class UpdateBranchRequestValidator : AbstractValidator<UpdateBranchRequest>
{
    public UpdateBranchRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Email).EmailAddress().MaximumLength(256).When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Phone).MaximumLength(30);
    }
}

public sealed class CreateEmployeeRequestValidator : AbstractValidator<CreateEmployeeRequest>
{
    public CreateEmployeeRequestValidator()
    {
        RuleFor(x => x.EmployeeCode).NotEmpty().MaximumLength(30);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).EmailAddress().MaximumLength(256).When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Position).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Salary).GreaterThanOrEqualTo(0);
        RuleFor(x => x.BranchId).GreaterThan(0);
    }
}

public sealed class UpdateEmployeeRequestValidator : AbstractValidator<UpdateEmployeeRequest>
{
    public UpdateEmployeeRequestValidator()
    {
        RuleFor(x => x.EmployeeCode).NotEmpty().MaximumLength(30);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).EmailAddress().MaximumLength(256).When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Position).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Salary).GreaterThanOrEqualTo(0);
        RuleFor(x => x.BranchId).GreaterThan(0);
    }
}

public sealed class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public sealed class CreateManufacturerRequestValidator : AbstractValidator<CreateManufacturerRequest>
{
    public CreateManufacturerRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Country).MaximumLength(100);
        RuleFor(x => x.ContactInfo).MaximumLength(300);
    }
}

public sealed class CreateUnitRequestValidator : AbstractValidator<CreateUnitRequest>
{
    public CreateUnitRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Abbreviation).NotEmpty().MaximumLength(20);
    }
}

public sealed class CreateMedicineRequestValidator : AbstractValidator<CreateMedicineRequest>
{
    public CreateMedicineRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.GenericName).MaximumLength(200);
        RuleFor(x => x.BrandName).MaximumLength(200);
        RuleFor(x => x.CategoryId).GreaterThan(0);
        RuleFor(x => x.ManufacturerId).GreaterThan(0);
        RuleFor(x => x.UnitId).GreaterThan(0);
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Barcode).MaximumLength(50);
        RuleFor(x => x.DosageForm).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Strength).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PurchasePrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SellingPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TaxPercentage).InclusiveBetween(0, 100);
        RuleFor(x => x.ReorderLevel).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}

public sealed class UpdateMedicineRequestValidator : AbstractValidator<UpdateMedicineRequest>
{
    public UpdateMedicineRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CategoryId).GreaterThan(0);
        RuleFor(x => x.ManufacturerId).GreaterThan(0);
        RuleFor(x => x.UnitId).GreaterThan(0);
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Barcode).MaximumLength(50);
        RuleFor(x => x.DosageForm).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Strength).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PurchasePrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SellingPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TaxPercentage).InclusiveBetween(0, 100);
        RuleFor(x => x.ReorderLevel).GreaterThanOrEqualTo(0);
    }
}
