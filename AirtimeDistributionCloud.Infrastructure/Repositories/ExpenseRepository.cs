using AirtimeDistributionCloud.Core.Entities;
using AirtimeDistributionCloud.Core.Enums;
using AirtimeDistributionCloud.Core.Interfaces;
using AirtimeDistributionCloud.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AirtimeDistributionCloud.Infrastructure.Repositories;

public class ExpenseRepository : Repository<Expense>, IExpenseRepository
{
    private readonly AppDbContext _appContext;

    public ExpenseRepository(AppDbContext context) : base(context)
    {
        _appContext = context;
    }

    public async Task<IReadOnlyList<Expense>> GetByStatusAsync(DepositStatus status, CancellationToken cancellationToken = default)
    {
        return await _appContext.Expenses
            .AsNoTracking()
            .Include(e => e.Branch)
            .Where(e => e.Status == status)
            .OrderByDescending(e => e.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Expense>> GetByBranchAsync(int branchId, CancellationToken cancellationToken = default)
    {
        return await _appContext.Expenses
            .AsNoTracking()
            .Include(e => e.Branch)
            .Where(e => e.BranchId == branchId)
            .OrderByDescending(e => e.CreatedDate)
            .ToListAsync(cancellationToken);
    }
}
