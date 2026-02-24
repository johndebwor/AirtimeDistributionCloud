using AirtimeDistributionCloud.Application.DTOs;
using AirtimeDistributionCloud.Core.Enums;

namespace AirtimeDistributionCloud.Application.Services;

public interface IAirtimeTransferService
{
    Task<IReadOnlyList<AirtimeTransferDto>> GetAllTransfersAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AirtimeTransferDto>> GetTransfersByDealerAsync(int dealerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AirtimeTransferDto>> GetTransfersByStatusAsync(DepositStatus status, CancellationToken cancellationToken = default);
    Task<AirtimeTransferDto> CreateTransferAsync(CreateAirtimeTransferRequest request, CancellationToken cancellationToken = default);
    Task<(AirtimeTransferDto Transfer, NotificationResult Notification)> ApproveTransferAsync(int transferId, string approvedByUserId, string notes, CancellationToken cancellationToken = default);
    Task<AirtimeTransferDto> RejectTransferAsync(int transferId, string rejectedByUserId, string notes, CancellationToken cancellationToken = default);
    Task<AirtimeTransferDto> CancelTransferAsync(int transferId, string cancelledByUserId, string reason, CancellationToken cancellationToken = default);
}
