namespace AirtimeDistributionCloud.Application.Services;

public interface IPermissionService
{
    static readonly string[] AdminKeys =
    [
        "admin.products", "admin.branches", "admin.deposits",
        "admin.approvals", "admin.transfer-approvals", "admin.expense-approvals",
        "admin.expense-categories", "admin.settings"
    ];

    static readonly string[] ReportsKeys =
    [
        "reports.reports", "reports.analytics"
    ];

    static readonly string[] AssetKeys =
    [
        "assets.registry", "assets.categories"
    ];

    static readonly string[] DealerKeys =
    [
        "dealer.registration", "dealer.transfers", "dealer.commissions"
    ];

    static readonly string[] CashierKeys =
    [
        "cashier.deposits", "cashier.balances", "cashier.expenses"
    ];

    static readonly string[] SystemKeys =
    [
        "system.users", "system.audit"
    ];

    static string[] AllKeys => [.. AdminKeys, .. ReportsKeys, .. AssetKeys, .. DealerKeys, .. CashierKeys, .. SystemKeys];

    // Roles whose permissions are managed; SystemAdmin always has full access in code
    static readonly string[] ManagedRoles = ["SuperAdministrator", "AssetManager", "Dealer", "Cashier"];

    static readonly Dictionary<string, string> PageDisplayNames = new()
    {
        ["admin.products"] = "Products",
        ["admin.branches"] = "Branches",
        ["admin.deposits"] = "Deposits",
        ["admin.approvals"] = "Deposit Approvals",
        ["admin.transfer-approvals"] = "Transfer Approvals",
        ["admin.expense-approvals"] = "Expense Approvals",
        ["admin.expense-categories"] = "Expense Categories",
        ["admin.settings"] = "System Settings",
        ["reports.reports"] = "Reports",
        ["reports.analytics"] = "Sales & Capital",
        ["assets.registry"] = "Asset Registry",
        ["assets.categories"] = "Asset Categories",
        ["dealer.registration"] = "Dealer Registration",
        ["dealer.transfers"] = "Airtime Transfers",
        ["dealer.commissions"] = "Commissions",
        ["cashier.deposits"] = "Cash Deposits",
        ["cashier.balances"] = "Balances",
        ["cashier.expenses"] = "Expenses",
        ["system.users"] = "User Management",
        ["system.audit"] = "Audit Logs",
    };

    Task<bool> IsAllowedAsync(string roleName, string pageKey);
    Task<Dictionary<string, Dictionary<string, bool>>> GetAllPermissionsAsync();
    Task SetPermissionAsync(string roleName, string pageKey, bool isAllowed);
}
