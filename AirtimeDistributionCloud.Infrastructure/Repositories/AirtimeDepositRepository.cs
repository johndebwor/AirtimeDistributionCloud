using AirtimeDistributionCloud.Core.Entities;
using AirtimeDistributionCloud.Core.Enums;
using AirtimeDistributionCloud.Core.Interfaces;
using AirtimeDistributionCloud.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AirtimeDistributionCloud.Infrastructure.Repositories;

public class AirtimeDepositRepository : Repository<AirtimeDeposit>, IAirtimeDepositRepository
{
    public AirtimeDepositRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<AirtimeDeposit>> GetByStatusAsync(DepositStatus status, CancellationToken cancellationToken = default)
        => await _dbSet.AsNoTracking()
            .Include(ad => ad.Product)
            .Where(ad => ad.Status == status)
            .OrderByDescending(ad => ad.CreatedDate)
            .ToListAsync(cancellationToken);
}
