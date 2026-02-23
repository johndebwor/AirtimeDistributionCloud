using AirtimeDistributionCloud.Core.Entities;
using AirtimeDistributionCloud.Core.Interfaces;

namespace AirtimeDistributionCloud.Application.Services;

public class UserPreferenceService : IUserPreferenceService
{
    private readonly IUnitOfWork _unitOfWork;

    public UserPreferenceService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<string?> GetStringAsync(string userId, string key, CancellationToken cancellationToken = default)
    {
        var prefs = await _unitOfWork.Repository<UserPreference>()
            .FindAsync(p => p.UserId == userId && p.Key == key, cancellationToken);
        return prefs.FirstOrDefault()?.Value;
    }

    public async Task<int?> GetIntAsync(string userId, string key, CancellationToken cancellationToken = default)
    {
        var prefs = await _unitOfWork.Repository<UserPreference>()
            .FindAsync(p => p.UserId == userId && p.Key == key, cancellationToken);
        var pref = prefs.FirstOrDefault();
        return pref is not null && int.TryParse(pref.Value, out var val) ? val : null;
    }

    public async Task SetAsync(string userId, string key, string value, CancellationToken cancellationToken = default)
    {
        var prefs = await _unitOfWork.Repository<UserPreference>()
            .FindAsync(p => p.UserId == userId && p.Key == key, cancellationToken);
        var existing = prefs.FirstOrDefault();

        if (existing is not null)
        {
            var tracked = await _unitOfWork.Repository<UserPreference>().GetByIdAsync(existing.Id, cancellationToken);
            if (tracked is not null)
                tracked.Value = value;
        }
        else
        {
            await _unitOfWork.Repository<UserPreference>().AddAsync(
                new UserPreference { UserId = userId, Key = key, Value = value }, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
