namespace AirtimeDistributionCloud.Application.Services;

public interface IUserPreferenceService
{
    public const string LastBranchId = "LastBranchId";
    public const string LastProductId = "LastProductId";
    public const string ThemeMode = "ThemeMode";

    Task<int?> GetIntAsync(string userId, string key, CancellationToken cancellationToken = default);
    Task<string?> GetStringAsync(string userId, string key, CancellationToken cancellationToken = default);
    Task SetAsync(string userId, string key, string value, CancellationToken cancellationToken = default);
}
