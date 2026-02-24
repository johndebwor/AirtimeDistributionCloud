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
                ["AirtimeDepositAutoApprove"] = ("No", "Automatically approve new airtime deposits without admin review (Yes/No)"),
                ["ExpenseApprovalThreshold"] = ("50000", "Expenses at or above this amount (SSP) require admin approval"),
                ["CommissionMinPercentage"] = ("0", "Minimum allowed commission rate (%)"),
                ["CommissionMaxPercentage"] = ("200", "Maximum allowed commission rate (%)"),
                ["AirtimeDepositFormula"] = (
                    "TotalAmount = Amount + (Amount × BonusPercentage ÷ 100)",
                    "Formula used when recording a new airtime deposit. BonusAmount = Amount × BonusPercentage ÷ 100; TotalAmount = Amount + BonusAmount."
                ),
                ["AirtimeTransferFormula"] = (
                    "AirtimeAmount = Amount ÷ (1 + CommissionRate ÷ 100); CommissionAmount = Amount − AirtimeAmount",
                    "Formula used when creating a new airtime transfer. The total entered amount is split into airtime and dealer commission."
                ),
                // Company Profile
                ["CompanyName"] = ("", "Company name displayed on reports and documents"),
                ["CompanyAbbreviation"] = ("", "Short abbreviation shown in the sidebar navigation (e.g. RB)"),
                ["CompanyAddress"] = ("", "Company physical address"),
                ["CompanyTel1"] = ("", "Primary telephone number"),
                ["CompanyTel2"] = ("", "Secondary telephone number"),
                ["CompanyTel3"] = ("", "Third telephone number"),
                ["CompanyLogoPath"] = ("", "Path to company logo image"),
                ["CompanyStampPath"] = ("", "Path to company electronic stamp image"),
                // Email Configuration
                ["SmtpHost"] = ("", "SMTP server hostname (e.g. smtp.gmail.com)"),
                ["SmtpPort"] = ("587", "SMTP server port (587 for TLS, 465 for SSL, 25 for unencrypted)"),
                ["SmtpUsername"] = ("", "SMTP authentication username / email"),
                ["SmtpPassword"] = ("", "SMTP authentication password or app password"),
                ["SmtpSenderEmail"] = ("", "Email address shown as the sender (From)"),
                ["SmtpSenderName"] = ("RasidBase", "Display name shown as the sender"),
                ["SmtpUseSsl"] = ("Yes", "Use SSL/TLS encryption for SMTP connection (Yes/No)"),
                // WhatsApp Business API
                ["WhatsAppApiAccessToken"] = ("", "Meta WhatsApp Business API access token (Bearer token)"),
                ["WhatsAppApiPhoneNumberId"] = ("", "WhatsApp Business phone number ID from Meta dashboard"),
                // Notifications
                ["TransferNotificationEnabled"] = ("No", "Enable dealer notifications on transfer approval (Yes/No)"),
                ["TransferNotificationMethod"] = ("Email", "Notification method: Email, DealerWhatsApp, or WhatsAppGroup"),
                ["TransferNotificationWhatsAppGroupUrl"] = ("", "WhatsApp group invite URL (used when method is WhatsAppGroup)"),
                ["TransferNotificationTemplate"] = (
                    "Dear {{Name}},\nYour request to transfer {{AirtimeAmount}} SSP of airtime has been approved.\n\nDetails of the transfer to dealer Number [{{DealerNumber}}]:\nReference No.: {{AirtimeTransferId}}\nProduct: {{Product}}\nAirtime Amount: {{AirtimeAmount}} SSP\nCommission Amount: {{CommissionAmount}} SSP\nBranch: {{Branch}}\nDate/Time: {{CreatedDate}}\n------------------------------\nTotal balance: {{TotalBalance}} SSP\nTotal commission: {{TotalCommission}} SSP\nCash balance: {{CashBalance}} SSP\n\nBest regards,\n{{CompanyName}} Administration",
                    "Message template for transfer approval notifications. Placeholders: {{Name}}, {{DealerNumber}}, {{AirtimeTransferId}}, {{Product}}, {{AirtimeAmount}}, {{CommissionAmount}}, {{Branch}}, {{CreatedDate}}, {{TotalBalance}}, {{TotalCommission}}, {{CashBalance}}, {{CompanyName}}"
                ),
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

            // Remove deprecated settings no longer in defaults
            var deprecatedKeys = new[] { "CashDepositFormula" };
            var toRemove = context.SystemSettings.Where(s => deprecatedKeys.Contains(s.Key)).ToList();
            if (toRemove.Count > 0)
            {
                context.SystemSettings.RemoveRange(toRemove);
                await context.SaveChangesAsync();
                logger.LogInformation("Removed deprecated settings: {Keys}", string.Join(", ", toRemove.Select(s => s.Key)));
            }

            // Seed default expense categories
            var defaultCategories = new (string Name, string Description)[]
            {
                ("Transport", "Vehicle fuel, transport, and logistics costs"),
                ("Office Supplies", "Stationery, printer cartridges, and other office consumables"),
                ("Utilities", "Electricity, water, internet, and phone bills"),
                ("Maintenance", "Building and equipment maintenance and repairs"),
                ("Other", "Miscellaneous expenses not covered by other categories"),
            };
            var existingCategoryNames = context.ExpenseCategories.Select(c => c.Name).ToHashSet();
            var categoriesAdded = false;
            foreach (var (name, description) in defaultCategories)
            {
                if (!existingCategoryNames.Contains(name))
                {
                    context.ExpenseCategories.Add(new Core.Entities.ExpenseCategory
                    {
                        Name = name,
                        Description = description,
                        IsActive = true
                    });
                    categoriesAdded = true;
                }
            }
            if (categoriesAdded)
            {
                await context.SaveChangesAsync();
                logger.LogInformation("Seeded default expense categories");
            }

            // Seed default asset categories
            var defaultAssetCategories = new (string Name, string Description)[]
            {
                ("IT Equipment", "Computers, laptops, tablets, printers, and peripherals"),
                ("Telecom Equipment", "SIM cards, modems, routers, Starlink devices"),
                ("Vehicles", "Cars, trucks, and other motor vehicles"),
                ("Motorbikes", "Motorcycles and scooters"),
                ("Furniture", "Office desks, chairs, cabinets, and furnishings"),
                ("Office Equipment", "Projectors, whiteboards, safes, and general equipment"),
                ("Other", "Miscellaneous assets not covered by other categories"),
            };
            var existingAssetCatNames = context.AssetCategories.Select(c => c.Name).ToHashSet();
            var assetCatsAdded = false;
            foreach (var (name, description) in defaultAssetCategories)
            {
                if (!existingAssetCatNames.Contains(name))
                {
                    context.AssetCategories.Add(new Core.Entities.AssetCategory
                    {
                        Name = name,
                        Description = description,
                        IsActive = true
                    });
                    assetCatsAdded = true;
                }
            }
            if (assetCatsAdded)
            {
                await context.SaveChangesAsync();
                logger.LogInformation("Seeded default asset categories");
            }

            // Seed default page permissions
            var defaultPermissions = new (string Role, string Page)[]
            {
                // SuperAdministrator
                ("SuperAdministrator", "admin.products"),
                ("SuperAdministrator", "admin.branches"),
                ("SuperAdministrator", "admin.deposits"),
                ("SuperAdministrator", "admin.approvals"),
                ("SuperAdministrator", "admin.transfer-approvals"),
                ("SuperAdministrator", "admin.expense-approvals"),
                ("SuperAdministrator", "admin.expense-categories"),
                ("SuperAdministrator", "admin.settings"),
                ("SuperAdministrator", "admin.analytics"),
                ("SuperAdministrator", "admin.assets"),
                ("SuperAdministrator", "admin.asset-categories"),
                ("SuperAdministrator", "dealer.registration"),
                ("SuperAdministrator", "dealer.transfers"),
                ("SuperAdministrator", "dealer.commissions"),
                ("SuperAdministrator", "system.users"),
                ("SuperAdministrator", "system.audit"),
                // Dealer
                ("Dealer", "dealer.registration"),
                ("Dealer", "dealer.transfers"),
                ("Dealer", "dealer.commissions"),
                // Cashier
                ("Cashier", "cashier.deposits"),
                ("Cashier", "cashier.balances"),
                ("Cashier", "cashier.expenses"),
            };

            var existingPerms = context.PagePermissions
                .Select(p => p.RoleName + "|" + p.PageKey)
                .ToHashSet();

            var permsAdded = false;
            foreach (var (role, page) in defaultPermissions)
            {
                if (!existingPerms.Contains(role + "|" + page))
                {
                    context.PagePermissions.Add(new Core.Entities.PagePermission
                    {
                        RoleName = role,
                        PageKey = page,
                        IsAllowed = true
                    });
                    permsAdded = true;
                }
            }

            if (permsAdded)
            {
                await context.SaveChangesAsync();
                logger.LogInformation("Seeded default page permissions");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }
}
