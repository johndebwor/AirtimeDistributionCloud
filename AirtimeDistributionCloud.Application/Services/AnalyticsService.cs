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

    public async Task<IReadOnlyList<StockMovementDto>> GetStockMovementAsync(AnalyticsPeriod period, CancellationToken ct = default)
    {
        var deposits = (await _depositRepo.GetAllAsync(ct))
            .Where(d => d.Status == DepositStatus.Approved).ToList();
        var transfers = (await _transferRepo.GetAllAsync(ct))
            .Where(t => t.Status == DepositStatus.Approved).ToList();

        var buckets = GetPeriodBuckets(period);

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

    public async Task<IReadOnlyList<CapitalGrowthPointDto>> GetCapitalGrowthTrendAsync(AnalyticsPeriod period, CancellationToken ct = default)
    {
        var deposits = (await _depositRepo.GetAllAsync(ct))
            .Where(d => d.Status == DepositStatus.Approved).ToList();
        var cashDeposits = (await _cashRepo.GetAllAsync(ct)).ToList();
        var expenses = (await _expenseRepo.GetAllAsync(ct))
            .Where(e => e.Status == DepositStatus.Approved).ToList();
        var products = await _productRepo.GetAllAsync(ct);
        var currentStock = products.Sum(p => p.AirtimeAccountBalance);

        var buckets = GetPeriodBuckets(period);

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

    public async Task<IReadOnlyList<ProfitBreakdownDto>> GetProfitBreakdownAsync(AnalyticsPeriod period, CancellationToken ct = default)
    {
        var deposits = (await _depositRepo.GetAllAsync(ct))
            .Where(d => d.Status == DepositStatus.Approved).ToList();
        var cashDeposits = (await _cashRepo.GetAllAsync(ct)).ToList();
        var expenses = (await _expenseRepo.GetAllAsync(ct))
            .Where(e => e.Status == DepositStatus.Approved).ToList();

        var buckets = GetPeriodBuckets(period);

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

    private static List<(string Label, DateTime Start, DateTime End)> GetPeriodBuckets(AnalyticsPeriod period)
    {
        var now = DateTime.UtcNow.Date;
        var buckets = new List<(string Label, DateTime Start, DateTime End)>();

        switch (period)
        {
            case AnalyticsPeriod.Daily:
                for (int i = 29; i >= 0; i--)
                {
                    var day = now.AddDays(-i);
                    buckets.Add((day.ToString("dd MMM"), day, day));
                }
                break;

            case AnalyticsPeriod.Weekly:
                for (int i = 11; i >= 0; i--)
                {
                    var weekEnd = now.AddDays(-i * 7);
                    var weekStart = weekEnd.AddDays(-6);
                    buckets.Add(($"{weekStart:dd MMM}", weekStart, weekEnd));
                }
                break;

            case AnalyticsPeriod.Monthly:
                for (int i = 11; i >= 0; i--)
                {
                    var month = now.AddMonths(-i);
                    var start = new DateTime(month.Year, month.Month, 1);
                    var end = start.AddMonths(1).AddDays(-1);
                    if (end > now) end = now;
                    buckets.Add((start.ToString("MMM yyyy"), start, end));
                }
                break;

            case AnalyticsPeriod.Yearly:
                for (int i = 4; i >= 0; i--)
                {
                    var year = now.Year - i;
                    var start = new DateTime(year, 1, 1);
                    var end = new DateTime(year, 12, 31);
                    if (end > now) end = now;
                    buckets.Add((year.ToString(), start, end));
                }
                break;
        }

        return buckets;
    }
}
