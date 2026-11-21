using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Application.Services;
using PharmacyManagement.Application.Validators;

namespace PharmacyManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateSaleRequestValidator>();

        services.AddScoped<ISettingsService, SettingsService>();
        services.AddScoped<IAuditQueryService, AuditQueryService>();
        services.AddScoped<IBranchService, BranchService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<ICatalogService, CatalogService>();
        services.AddScoped<IMedicineService, MedicineService>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IStockTransferService, StockTransferService>();
        services.AddScoped<IPurchaseService, PurchaseService>();
        services.AddScoped<ISaleService, SaleService>();
        services.AddScoped<IPrescriptionService, PrescriptionService>();
        services.AddScoped<IReturnService, ReturnService>();
        services.AddScoped<IExpenseService, ExpenseService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IReportService, ReportService>();

        return services;
    }
}
