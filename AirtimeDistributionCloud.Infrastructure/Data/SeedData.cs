using AirtimeDistributionCloud.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AirtimeDistributionCloud.Infrastructure.Data;

public static class SeedData
{
    public static readonly string[] Roles = ["SystemAdmin", "SuperAdministrator", "Dealer", "Cashier"];

    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        try
        {
            await context.Database.MigrateAsync();

            foreach (var role in Roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                    logger.LogInformation("Created role: {Role}", role);
                }
            }

            const string adminEmail = "admin@airtimecloud.com";
            if (await userManager.FindByEmailAsync(adminEmail) is null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    EmailConfirmed = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System"
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123456");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "SystemAdmin");
                    logger.LogInformation("Created default admin user");
                }
                else
                {
                    logger.LogError("Failed to create admin: {Errors}",
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }

            // Seed system settings (add any missing defaults)
            var defaultSettings = new Dictionary<string, (string Value, string Description)>
            {
                ["ExpenseApprovalThreshold"] = ("50000", "Expenses at or above this amount (SSP) require admin approval"),
                ["CommissionMinPercentage"] = ("0", "Minimum allowed commission rate (%)"),
                ["CommissionMaxPercentage"] = ("200", "Maximum allowed commission rate (%)"),
            };
            var existingKeys = context.SystemSettings.Select(s => s.Key).ToHashSet();
            var settingsAdded = false;
            foreach (var (key, (value, description)) in defaultSettings)
            {
                if (!existingKeys.Contains(key))
                {
                    context.SystemSettings.Add(new SystemSetting { Key = key, Value = value, Description = description });
                    settingsAdded = true;
                }
            }
            if (settingsAdded)
            {
                await context.SaveChangesAsync();
                logger.LogInformation("Seeded default system settings");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }
}
