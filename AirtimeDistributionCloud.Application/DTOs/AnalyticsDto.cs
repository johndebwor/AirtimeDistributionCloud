namespace AirtimeDistributionCloud.Application.DTOs;

// ── Sales Stock ──

public record SalesStockSummaryDto(
    decimal TotalStockPurchased,
    decimal TotalStockDistributed,
    decimal CurrentStockRemaining,
    decimal StockUtilizationPercent,
    decimal TotalBonusReceived,
    int TotalDepositCount,
    int TotalTransferCount);

public record ProductStockDto(
    int ProductId,
    string ProductName,
    decimal CurrentBalance,
    decimal TotalDeposited,
    decimal TotalDistributed,
    decimal UtilizationPercent);

public record StockMovementDto(
    string PeriodLabel,
    decimal Deposited,
    decimal Distributed,
    decimal NetChange);

// ── Capital Growth ──

public record CapitalSummaryDto(
    decimal TotalAirtimeInvestment,
    decimal TotalBonusGained,
    decimal TotalCashCollected,
    decimal TotalCommissionsPaid,
    decimal TotalExpenses,
    decimal OutstandingDealerDebt,
    decimal CurrentStockValue,
    decimal NetCapital,
    decimal GrossProfit);

public record CapitalGrowthPointDto(
    string PeriodLabel,
    decimal CumulativeCashCollected,
    decimal CumulativeInvestment,
    decimal CumulativeExpenses,
    decimal NetCapital);

public record ProfitBreakdownDto(
    string PeriodLabel,
    decimal CashCollected,
    decimal AirtimeInvested,
    decimal ExpensesPaid,
    decimal Profit);

public enum AnalyticsPeriod
{
    Daily,
    Weekly,
    Monthly,
    Yearly
}
