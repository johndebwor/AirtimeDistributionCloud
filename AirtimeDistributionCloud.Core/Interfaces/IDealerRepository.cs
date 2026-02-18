using AirtimeDistributionCloud.Core.Entities;

namespace AirtimeDistributionCloud.Core.Interfaces;

public interface IDealerRepository : IRepository<Dealer>
{
    Task<Dealer?> GetDealerWithProductsAsync(int dealerId, CancellationToken cancellationToken = default);
}
