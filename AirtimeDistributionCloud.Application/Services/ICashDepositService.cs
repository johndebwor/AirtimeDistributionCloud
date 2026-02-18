using AirtimeDistributionCloud.Application.DTOs;

namespace AirtimeDistributionCloud.Application.Services;

public interface ICashDepositService
{
    Task<IReadOnlyList<CashDepositDto>> GetAllDepositsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CashDepositDto>> GetDepositsByDealerAsync(int dealerId, CancellationToken cancellationToken = default);
    Task<CashDepositDto> CreateDepositAsync(CreateCashDepositRequest request, CancellationToken cancellationToken = default);
}
