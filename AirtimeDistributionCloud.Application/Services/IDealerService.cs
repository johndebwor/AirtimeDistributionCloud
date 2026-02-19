using AirtimeDistributionCloud.Application.DTOs;

namespace AirtimeDistributionCloud.Application.Services;

public interface IDealerService
{
    Task<IReadOnlyList<DealerDto>> GetAllDealersAsync(CancellationToken cancellationToken = default);
    Task<DealerDto?> GetDealerByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<DealerDto> CreateDealerAsync(CreateDealerRequest request, CancellationToken cancellationToken = default);
    Task<decimal> GetDealerCashBalanceAsync(int dealerId, CancellationToken cancellationToken = default);
    Task UpdateCommissionAsync(UpdateCommissionRequest request, CancellationToken cancellationToken = default);
}
