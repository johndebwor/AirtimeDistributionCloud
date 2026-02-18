using AirtimeDistributionCloud.Core.Entities;
using AirtimeDistributionCloud.Core.Interfaces;
using AirtimeDistributionCloud.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AirtimeDistributionCloud.Infrastructure.Repositories;

public class DealerRepository : Repository<Dealer>, IDealerRepository
{
    public DealerRepository(AppDbContext context) : base(context) { }

    public async Task<Dealer?> GetDealerWithProductsAsync(int dealerId, CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(d => d.DealerProducts)
                .ThenInclude(dp => dp.Product)
            .FirstOrDefaultAsync(d => d.Id == dealerId, cancellationToken);
}
