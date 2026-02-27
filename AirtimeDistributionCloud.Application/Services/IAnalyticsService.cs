using AirtimeDistributionCloud.Application.DTOs;

namespace AirtimeDistributionCloud.Application.Services;

public interface IAnalyticsService
{
    Task<SalesStockSummaryDto> GetStockSummaryAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken ct = default);
    Task<IReadOnlyList<ProductStockDto>> GetProductStockBreakdownAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken ct = default);
    Task<IReadOnlyList<StockMovementDto>> GetStockMovementAsync(AnalyticsPeriod period, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken ct = default);
    Task<CapitalSummaryDto> GetCapitalSummaryAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken ct = default);
    Task<IReadOnlyList<CapitalGrowthPointDto>> GetCapitalGrowthTrendAsync(AnalyticsPeriod period, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken ct = default);
    Task<IReadOnlyList<ProfitBreakdownDto>> GetProfitBreakdownAsync(AnalyticsPeriod period, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken ct = default);
}
