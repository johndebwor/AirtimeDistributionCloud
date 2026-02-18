using AirtimeDistributionCloud.Core.Entities;
using AirtimeDistributionCloud.Core.Enums;

namespace AirtimeDistributionCloud.Core.Interfaces;

public interface IAirtimeDepositRepository : IRepository<AirtimeDeposit>
{
    Task<IReadOnlyList<AirtimeDeposit>> GetByStatusAsync(DepositStatus status, CancellationToken cancellationToken = default);
}
