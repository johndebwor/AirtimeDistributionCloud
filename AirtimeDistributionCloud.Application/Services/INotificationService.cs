namespace AirtimeDistributionCloud.Application.Services;

public interface INotificationService
{
    Task SendTransferApprovalNotificationAsync(int transferId, CancellationToken cancellationToken = default);
}
