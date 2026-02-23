using AirtimeDistributionCloud.Core.Entities;
using AirtimeDistributionCloud.Core.Interfaces;
using AirtimeDistributionCloud.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AirtimeDistributionCloud.Infrastructure.Repositories;

public class SystemSettingRepository : ISystemSettingRepository
{
    private readonly AppDbContext _context;

    public SystemSettingRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SystemSetting?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _context.SystemSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Key == key, cancellationToken);
    }

    public async Task<IReadOnlyList<SystemSetting>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SystemSettings
            .AsNoTracking()
            .OrderBy(s => s.Key)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(SystemSetting setting, CancellationToken cancellationToken = default)
    {
        await _context.SystemSettings.AddAsync(setting, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(SystemSetting setting, CancellationToken cancellationToken = default)
    {
        var entry = _context.Entry(setting);
        if (entry.State == EntityState.Detached)
        {
            var tracked = _context.ChangeTracker.Entries<SystemSetting>()
                .FirstOrDefault(e => e.Entity.Id == setting.Id);
            if (tracked is not null)
            {
                tracked.CurrentValues.SetValues(setting);
                tracked.State = EntityState.Modified;
            }
            else
            {
                _context.SystemSettings.Update(setting);
            }
        }
        else
        {
            entry.State = EntityState.Modified;
        }
        await _context.SaveChangesAsync(cancellationToken);
    }
}
