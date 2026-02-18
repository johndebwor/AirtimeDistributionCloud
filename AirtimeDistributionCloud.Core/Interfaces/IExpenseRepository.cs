using AirtimeDistributionCloud.Core.Entities;
using AirtimeDistributionCloud.Core.Enums;

namespace AirtimeDistributionCloud.Core.Interfaces;

public interface IExpenseRepository : IRepository<Expense>
{
    Task<IReadOnlyList<Expense>> GetByStatusAsync(DepositStatus status, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Expense>> GetByBranchAsync(int branchId, CancellationToken cancellationToken = default);
}
