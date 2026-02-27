using AirtimeDistributionCloud.Application.DTOs;
using AirtimeDistributionCloud.Core.Entities;
using AirtimeDistributionCloud.Core.Enums;
using AirtimeDistributionCloud.Core.Interfaces;

namespace AirtimeDistributionCloud.Application.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAirtimeDepositRepository _depositRepo;
    private readonly IAirtimeTransferRepository _transferRepo;
    private readonly ICashDepositRepository _cashRepo;
    private readonly IExpenseRepository _expenseRepo;
    private readonly IProductRepository _productRepo;

    public AnalyticsService(
        IUnitOfWork unitOfWork,
        IAirtimeDepositRepository depositRepo,
        IAirtimeTransferRepository transferRepo,
        ICashDepositRepository cashRepo,
        IExpenseRepository expenseRepo,
        IProductRepository productRepo)
    {
        _unitOfWork = unitOfWork;
        _depositRepo = depositRepo;
        _transferRepo = transferRepo;
        _cashRepo = cashRepo;
        _expenseRepo = expenseRepo;
        _productRepo = productRepo;
    }

    public async Task<SalesStockSummaryDto> GetStockSummaryAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken ct = default)
    {
        var deposits = (await _depositRepo.GetAllAsync(ct))
            .Where(d => d.Status == DepositStatus.Approved);
        var transfers = (await _transferRepo.GetAllAsync(ct))
            .Where(t => t.Status == DepositStatus.Approved);
        var products = await _productRepo.GetAllAsync(ct);

        if (fromDate.HasValue)
        {
            deposits = deposits.Where(d => d.CreatedDate.Date >= fromDate.Value.Date);
            transfers = transfers.Where(t => t.CreatedDate.Date >= fromDate.Value.Date);
        }
        if (toDate.HasValue)
        {
            deposits = deposits.Where(d => d.CreatedDate.Date <= toDate.Value.Date);
            transfers = transfers.Where(t => t.CreatedDate.Date <= toDate.Value.Date);
        }

        var depositList = deposits.ToList();
        var transferList = transfers.ToList();

        var totalPurchased = depositList.Sum(d => d.TotalAmount);
        var totalDistributed = transferList.Sum(t => t.LoanAmount);
        var currentStock = products.Sum(p => p.AirtimeAccountBalance);
        var utilization = totalPurchased > 0 ? totalDistributed / totalPurchased * 100 : 0;

        return new SalesStockSummaryDto(
            totalPurchased, totalDistributed, currentStock, utilization,
            depositList.Sum(d => d.BonusAmount),
            depositList.Count, transferList.Count);
    }

    public async Task<IReadOnlyList<ProductStockDto>> GetProductStockBreakdownAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken ct = default)
    {
        var deposits = (await _depositRepo.GetAllAsync(ct))
            .Where(d => d.Status == DepositStatus.Approved);
        var transfers = (await _transferRepo.GetAllAsync(ct))
            .Where(t => t.Status == DepositStatus.Approved);
        var products = await _productRepo.GetAllAsync(ct);

        if (fromDate.HasValue)
        {
            deposits = deposits.Where(d => d.CreatedDate.Date >= fromDate.Value.Date);
            transfers = transfers.Where(t => t.CreatedDate.Date >= fromDate.Value.Date);
        }
        if (toDate.HasValue)
        {
            deposits = deposits.Where(d => d.CreatedDate.Date <= toDate.Value.Date);
            transfers = transfers.Where(t => t.CreatedDate.Date <= toDate.Value.Date);
        }

        var depositsByProduct = deposits.GroupBy(d => d.ProductId)
            .ToDictionary(g => g.Key, g => g.Sum(d => d.TotalAmount));
        var transfersByProduct = transfers.GroupBy(t => t.ProductId)
            .ToDictionary(g => g.Key, g => g.Sum(t => t.LoanAmount));

        return products.Select(p =>
        {
            var deposited = depositsByProduct.GetValueOrDefault(p.Id);
            var distributed = transfersByProduct.GetValueOrDefault(p.Id);
            var util = deposited > 0 ? distributed / deposited * 100 : 0;
            return new ProductStockDto(p.Id, p.Name, p.AirtimeAccountBalance, deposited, distributed, util);
        }).ToList();
    }

    public async Task<IReadOnlyList<StockMovementDto>> GetStockMovementAsync(AnalyticsPeriod period, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken ct = default)
    {
        var deposits = (await _depositRepo.GetAllAsync(ct))
            .Where(d => d.Status == DepositStatus.Approved).ToList();
        var transfers = (await _transferRepo.GetAllAsync(ct))
            .Where(t => t.Status == DepositStatus.Approved).ToList();

        var buckets = GetPeriodBuckets(period, fromDate, toDate);

        return buckets.Select(b =>
        {
            var dep = deposits.Where(d => d.CreatedDate.Date >= b.Start && d.CreatedDate.Date <= b.End)
                .Sum(d => d.TotalAmount);
            var dist = transfers.Where(t => t.CreatedDate.Date >= b.Start && t.CreatedDate.Date <= b.End)
                .Sum(t => t.LoanAmount);
            return new StockMovementDto(b.Label, dep, dist, dep - dist);
        }).ToList();
    }

    public async Task<CapitalSummaryDto> GetCapitalSummaryAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken ct = default)
    {
        var deposits = (await _depositRepo.GetAllAsync(ct))
            .Where(d => d.Status == DepositStatus.Approved);
        var transfers = (await _transferRepo.GetAllAsync(ct))
            .Where(t => t.Status == DepositStatus.Approved);
        var cashDeposits = await _cashRepo.GetAllAsync(ct);
        var expenses = (await _expenseRepo.GetAllAsync(ct))
            .Where(e => e.Status == DepositStatus.Approved);
        var products = await _productRepo.GetAllAsync(ct);
        var dealerProducts = await _unitOfWork.Repository<DealerProduct>().GetAllAsync(ct);

        if (fromDate.HasValue)
        {
            deposits = deposits.Where(d => d.CreatedDate.Date >= fromDate.Value.Date);
            transfers = transfers.Where(t => t.CreatedDate.Date >= fromDate.Value.Date);
            cashDeposits = cashDeposits.Where(d => d.DepositDate.Date >= fromDate.Value.Date).ToList();
            expenses = expenses.Where(e => e.ExpenseDate.Date >= fromDate.Value.Date);
        }
        if (toDate.HasValue)
        {
            deposits = deposits.Where(d => d.CreatedDate.Date <= toDate.Value.Date);
            transfers = transfers.Where(t => t.CreatedDate.Date <= toDate.Value.Date);
            cashDeposits = cashDeposits.Where(d => d.DepositDate.Date <= toDate.Value.Date).ToList();
            expenses = expenses.Where(e => e.ExpenseDate.Date <= toDate.Value.Date);
        }

        var totalInvestment = deposits.Sum(d => d.Amount);
        var totalBonus = deposits.Sum(d => d.BonusAmount);
        var totalCash = cashDeposits.Sum(d => d.Amount);
        var totalCommissions = transfers.Sum(t => t.CommissionAmount);
        var totalExpenses = expenses.Sum(e => e.Amount);
        var outstandingDebt = dealerProducts.Sum(dp => dp.Balance);
        var currentStock = products.Sum(p => p.AirtimeAccountBalance);
        var netCapital = totalCash + currentStock - totalExpenses;
        var grossProfit = totalCash - totalInvestment - totalExpenses;

        return new CapitalSummaryDto(
            totalInvestment, totalBonus, totalCash, totalCommissions,
            totalExpenses, outstandingDebt, currentStock, netCapital, grossProfit);
    }

    public async Task<IReadOnlyList<CapitalGrowthPointDto>> GetCapitalGrowthTrendAsync(AnalyticsPeriod period, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken ct = default)
    {
        var deposits = (await _depositRepo.GetAllAsync(ct))
            .Where(d => d.Status == DepositStatus.Approved).ToList();
        var cashDeposits = (await _cashRepo.GetAllAsync(ct)).ToList();
        var expenses = (await _expenseRepo.GetAllAsync(ct))
            .Where(e => e.Status == DepositStatus.Approved).ToList();
        var products = await _productRepo.GetAllAsync(ct);
        var currentStock = products.Sum(p => p.AirtimeAccountBalance);

        var buckets = GetPeriodBuckets(period, fromDate, toDate);

        decimal cumCash = 0, cumInvestment = 0, cumExpenses = 0;
        var result = new List<CapitalGrowthPointDto>();

        foreach (var b in buckets)
        {
            cumCash += cashDeposits.Where(d => d.DepositDate.Date >= b.Start && d.DepositDate.Date <= b.End)
                .Sum(d => d.Amount);
            cumInvestment += deposits.Where(d => d.CreatedDate.Date >= b.Start && d.CreatedDate.Date <= b.End)
                .Sum(d => d.Amount);
            cumExpenses += expenses.Where(e => e.ExpenseDate.Date >= b.Start && e.ExpenseDate.Date <= b.End)
                .Sum(e => e.Amount);

            result.Add(new CapitalGrowthPointDto(b.Label, cumCash, cumInvestment, cumExpenses,
                cumCash + currentStock - cumExpenses));
        }

        return result;
    }

    public async Task<IReadOnlyList<ProfitBreakdownDto>> GetProfitBreakdownAsync(AnalyticsPeriod period, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken ct = default)
    {
        var deposits = (await _depositRepo.GetAllAsync(ct))
            .Where(d => d.Status == DepositStatus.Approved).ToList();
        var cashDeposits = (await _cashRepo.GetAllAsync(ct)).ToList();
        var expenses = (await _expenseRepo.GetAllAsync(ct))
            .Where(e => e.Status == DepositStatus.Approved).ToList();

        var buckets = GetPeriodBuckets(period, fromDate, toDate);

        return buckets.Select(b =>
        {
            var cash = cashDeposits.Where(d => d.DepositDate.Date >= b.Start && d.DepositDate.Date <= b.End)
                .Sum(d => d.Amount);
            var invested = deposits.Where(d => d.CreatedDate.Date >= b.Start && d.CreatedDate.Date <= b.End)
                .Sum(d => d.Amount);
            var exp = expenses.Where(e => e.ExpenseDate.Date >= b.Start && e.ExpenseDate.Date <= b.End)
                .Sum(e => e.Amount);
            return new ProfitBreakdownDto(b.Label, cash, invested, exp, cash - invested - exp);
        }).ToList();
    }

    private static List<(string Label, DateTime Start, DateTime End)> GetPeriodBuckets(
        AnalyticsPeriod period, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var now = DateTime.UtcNow.Date;
        var buckets = new List<(string Label, DateTime Start, DateTime End)>();

        // Resolve the effective range
        DateTime rangeEnd = toDate.HasValue ? (toDate.Value.Date > now ? now : toDate.Value.Date) : now;
        DateTime rangeStart = fromDate.HasValue
            ? fromDate.Value.Date
            : period switch
            {
                AnalyticsPeriod.Daily   => now.AddDays(-29),
                AnalyticsPeriod.Weekly  => now.AddDays(-7 * 11),
                AnalyticsPeriod.Yearly  => new DateTime(now.Year - 4, 1, 1),
                _                       => new DateTime(now.AddMonths(-11).Year, now.AddMonths(-11).Month, 1)
            };

        if (rangeStart > rangeEnd) rangeStart = rangeEnd;

        switch (period)
        {
            case AnalyticsPeriod.Daily:
                for (var day = rangeStart; day <= rangeEnd; day = day.AddDays(1))
                    buckets.Add((day.ToString("dd MMM"), day, day));
                break;

            case AnalyticsPeriod.Weekly:
                var ws = rangeStart;
                while (ws <= rangeEnd)
                {
                    var we = ws.AddDays(6);
                    if (we > rangeEnd) we = rangeEnd;
                    buckets.Add(($"{ws:dd MMM}", ws, we));
                    ws = ws.AddDays(7);
                }
                break;

            case AnalyticsPeriod.Monthly:
                var ms = new DateTime(rangeStart.Year, rangeStart.Month, 1);
                while (ms <= rangeEnd)
                {
                    var me = ms.AddMonths(1).AddDays(-1);
                    if (me > rangeEnd) me = rangeEnd;
                    buckets.Add((ms.ToString("MMM yyyy"), ms, me));
                    ms = ms.AddMonths(1);
                }
                break;

            case AnalyticsPeriod.Yearly:
                for (var y = rangeStart.Year; y <= rangeEnd.Year; y++)
                {
                    var ys = y == rangeStart.Year ? rangeStart : new DateTime(y, 1, 1);
                    var ye = y == rangeEnd.Year ? rangeEnd : new DateTime(y, 12, 31);
                    buckets.Add((y.ToString(), ys, ye));
                }
                break;
        }

        return buckets;
    }
}
