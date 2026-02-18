using AirtimeDistributionCloud.Core.Entities;

namespace AirtimeDistributionCloud.Core.Interfaces;

public interface ICashDepositRepository : IRepository<CashDeposit>
{
    Task<IReadOnlyList<CashDeposit>> GetByDealerAsync(int dealerId, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalDepositsByDealerAsync(int dealerId, CancellationToken cancellationToken = default);
}
