namespace AirtimeDistributionCloud.Application.Services;

public record NotificationResult(
    List<string> WhatsAppUrls,
    string? FormattedMessage = null,
    string? WhatsAppGroupUrl = null,
    List<string>? WhatsAppApiResults = null);

public interface INotificationService
{
    Task<NotificationResult> SendTransferApprovalNotificationAsync(int transferId, CancellationToken cancellationToken = default);
}
