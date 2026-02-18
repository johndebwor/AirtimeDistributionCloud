using AirtimeDistributionCloud.Core.Entities;
using AirtimeDistributionCloud.Core.Interfaces;
using AirtimeDistributionCloud.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AirtimeDistributionCloud.Infrastructure.Repositories;

public class CashDepositRepository : Repository<CashDeposit>, ICashDepositRepository
{
    public CashDepositRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<CashDeposit>> GetByDealerAsync(int dealerId, CancellationToken cancellationToken = default)
        => await _dbSet.AsNoTracking()
            .Include(cd => cd.Branch)
            .Where(cd => cd.DealerId == dealerId)
            .OrderByDescending(cd => cd.DepositDate)
            .ToListAsync(cancellationToken);

    public async Task<decimal> GetTotalDepositsByDealerAsync(int dealerId, CancellationToken cancellationToken = default)
        => await _dbSet.Where(cd => cd.DealerId == dealerId).SumAsync(cd => cd.Amount, cancellationToken);
}
