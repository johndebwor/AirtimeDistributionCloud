using AirtimeDistributionCloud.Core.Entities;
using AirtimeDistributionCloud.Core.Enums;
using AirtimeDistributionCloud.Core.Interfaces;
using AirtimeDistributionCloud.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AirtimeDistributionCloud.Infrastructure.Repositories;

public class AirtimeTransferRepository : Repository<AirtimeTransfer>, IAirtimeTransferRepository
{
    private readonly AppDbContext _appContext;

    public AirtimeTransferRepository(AppDbContext context) : base(context)
    {
        _appContext = context;
    }

    public async Task<IReadOnlyList<AirtimeTransfer>> GetByBranchAsync(int branchId, CancellationToken cancellationToken = default)
        => await _dbSet.AsNoTracking()
            .Include(at => at.Product)
            .Include(at => at.Dealer)
            .Where(at => at.BranchId == branchId)
            .OrderByDescending(at => at.CreatedDate)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<AirtimeTransfer>> GetByDealerAsync(int dealerId, CancellationToken cancellationToken = default)
        => await _dbSet.AsNoTracking()
            .Include(at => at.Product)
            .Include(at => at.Branch)
            .Where(at => at.DealerId == dealerId)
            .OrderByDescending(at => at.CreatedDate)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<AirtimeTransfer>> GetByStatusAsync(DepositStatus status, CancellationToken cancellationToken = default)
        => await _appContext.AirtimeTransfers
            .AsNoTracking()
            .Include(at => at.Product)
            .Include(at => at.Dealer)
            .Include(at => at.Branch)
            .Where(at => at.Status == status)
            .OrderByDescending(at => at.CreatedDate)
            .ToListAsync(cancellationToken);
}
