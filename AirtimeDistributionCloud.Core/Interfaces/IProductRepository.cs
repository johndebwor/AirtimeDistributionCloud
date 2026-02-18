using AirtimeDistributionCloud.Core.Entities;

namespace AirtimeDistributionCloud.Core.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<IReadOnlyList<Product>> GetActiveProductsAsync(CancellationToken cancellationToken = default);
}
