using AirtimeDistributionCloud.Application.DTOs;
using AirtimeDistributionCloud.Core.Enums;

namespace AirtimeDistributionCloud.Application.Services;

public interface IAirtimeDepositService
{
    Task<IReadOnlyList<AirtimeDepositDto>> GetAllDepositsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AirtimeDepositDto>> GetDepositsByStatusAsync(DepositStatus status, CancellationToken cancellationToken = default);
    Task<AirtimeDepositDto> CreateDepositAsync(CreateAirtimeDepositRequest request, string userId, CancellationToken cancellationToken = default);
    Task<AirtimeDepositDto> ApproveDepositAsync(int depositId, string approvedByUserId, string notes, CancellationToken cancellationToken = default);
    Task<AirtimeDepositDto> RejectDepositAsync(int depositId, string rejectedByUserId, string notes, CancellationToken cancellationToken = default);
    Task<AirtimeDepositDto> CancelDepositAsync(int depositId, string cancelledByUserId, string reason, CancellationToken cancellationToken = default);
}
