using AirtimeDistributionCloud.Core.Entities;
using AirtimeDistributionCloud.Core.Interfaces;

namespace AirtimeDistributionCloud.Application.Services;

public class PermissionService(IUnitOfWork unitOfWork) : IPermissionService
{
    private Dictionary<string, Dictionary<string, bool>>? _cache;

    public async Task<bool> IsAllowedAsync(string roleName, string pageKey)
    {
        var cache = await GetCacheAsync();
        return cache.TryGetValue(roleName, out var pages) && pages.TryGetValue(pageKey, out var allowed) && allowed;
    }

    public async Task<Dictionary<string, Dictionary<string, bool>>> GetAllPermissionsAsync()
    {
        return await GetCacheAsync();
    }

    public async Task SetPermissionAsync(string roleName, string pageKey, bool isAllowed)
    {
        // FindAsync uses AsNoTracking — just grab the Id
        var existing = (await unitOfWork.Repository<PagePermission>()
            .FindAsync(p => p.RoleName == roleName && p.PageKey == pageKey))
            .FirstOrDefault();

        if (existing is not null)
        {
            // GetByIdAsync uses EF's identity map: returns the already-tracked instance
            // if present, avoiding duplicate-tracking conflicts across successive saves.
            var tracked = await unitOfWork.Repository<PagePermission>().GetByIdAsync(existing.Id);
            if (tracked is not null)
                tracked.IsAllowed = isAllowed;
            // No UpdateAsync call needed — EF detects the property change automatically.
        }
        else
        {
            await unitOfWork.Repository<PagePermission>().AddAsync(new PagePermission
            {
                RoleName = roleName,
                PageKey = pageKey,
                IsAllowed = isAllowed
            });
        }

        await unitOfWork.SaveChangesAsync();
        _cache = null; // invalidate cache
    }

    private async Task<Dictionary<string, Dictionary<string, bool>>> GetCacheAsync()
    {
        if (_cache is not null) return _cache;

        var permissions = await unitOfWork.Repository<PagePermission>().GetAllAsync();

        _cache = new Dictionary<string, Dictionary<string, bool>>();
        foreach (var p in permissions)
        {
            if (!_cache.TryGetValue(p.RoleName, out var pages))
            {
                pages = new Dictionary<string, bool>();
                _cache[p.RoleName] = pages;
            }
            pages[p.PageKey] = p.IsAllowed;
        }

        return _cache;
    }
}
