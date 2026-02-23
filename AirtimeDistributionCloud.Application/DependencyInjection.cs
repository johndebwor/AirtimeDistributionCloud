using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace AirtimeDistributionCloud.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddScoped<Services.IProductService, Services.ProductService>();
        services.AddScoped<Services.IBranchService, Services.BranchService>();
        services.AddScoped<Services.IDealerService, Services.DealerService>();
        services.AddScoped<Services.IAirtimeDepositService, Services.AirtimeDepositService>();
        services.AddScoped<Services.IAirtimeTransferService, Services.AirtimeTransferService>();
        services.AddScoped<Services.ICashDepositService, Services.CashDepositService>();
        services.AddScoped<Services.IAuditLogService, Services.AuditLogService>();
        services.AddScoped<Services.IExpenseService, Services.ExpenseService>();
        services.AddScoped<Services.IUserPreferenceService, Services.UserPreferenceService>();
        services.AddScoped<Services.IPermissionService, Services.PermissionService>();
        services.AddScoped<Services.IExpenseCategoryService, Services.ExpenseCategoryService>();
        services.AddScoped<Services.INotificationService, Services.NotificationService>();

        return services;
    }
}
