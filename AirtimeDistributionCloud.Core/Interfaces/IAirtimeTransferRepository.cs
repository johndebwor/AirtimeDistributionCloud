using AirtimeDistributionCloud.Core.Entities;
using AirtimeDistributionCloud.Core.Enums;

namespace AirtimeDistributionCloud.Core.Interfaces;

public interface IAirtimeTransferRepository : IRepository<AirtimeTransfer>
{
    Task<IReadOnlyList<AirtimeTransfer>> GetByBranchAsync(int branchId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AirtimeTransfer>> GetByDealerAsync(int dealerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AirtimeTransfer>> GetByStatusAsync(DepositStatus status, CancellationToken cancellationToken = default);
}
