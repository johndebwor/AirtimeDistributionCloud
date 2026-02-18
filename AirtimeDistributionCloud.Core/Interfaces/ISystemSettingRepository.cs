using AirtimeDistributionCloud.Core.Entities;

namespace AirtimeDistributionCloud.Core.Interfaces;

public interface ISystemSettingRepository
{
    Task<SystemSetting?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SystemSetting>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(SystemSetting setting, CancellationToken cancellationToken = default);
    Task UpdateAsync(SystemSetting setting, CancellationToken cancellationToken = default);
}
