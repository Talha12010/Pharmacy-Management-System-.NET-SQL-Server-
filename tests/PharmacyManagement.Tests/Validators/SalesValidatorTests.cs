using PharmacyManagement.Application.DTOs.Sales;
using PharmacyManagement.Application.Validators;
using PharmacyManagement.Domain.Enums;

namespace PharmacyManagement.Tests.Validators;

public sealed class SalesValidatorTests
{
    [Fact]
    public void CreateSaleRequestValidator_Accepts_Valid_Request()
    {
        var request = new CreateSaleRequest
        {
            BranchId = 1,
            Items =
            {
                new CreateSaleItemRequest { MedicineId = 1, Quantity = 2 }
            },
            Payments =
            {
                new CreateSalePaymentRequest { Method = PaymentMethod.Cash, Amount = 20m }
            }
        };

        var result = new CreateSaleRequestValidator().Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateSaleRequestValidator_Rejects_Empty_Cart()
    {
        var request = new CreateSaleRequest
        {
            BranchId = 1,
            Payments =
            {
                new CreateSalePaymentRequest { Method = PaymentMethod.Cash, Amount = 20m }
            }
        };

        var result = new CreateSaleRequestValidator().Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.ErrorMessage == "The cart is empty.");
    }

    [Fact]
    public void CreateSaleRequestValidator_Rejects_Invalid_Item_And_Payment()
    {
        var request = new CreateSaleRequest
        {
            BranchId = 0,
            DiscountAmount = -1m,
            Items =
            {
                new CreateSaleItemRequest { MedicineId = 0, Quantity = 0, DiscountAmount = -1m }
            },
            Payments =
            {
                new CreateSalePaymentRequest { Method = PaymentMethod.Cash, Amount = 0m }
            }
        };

        var result = new CreateSaleRequestValidator().Validate(request);

        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count >= 5);
    }
}
