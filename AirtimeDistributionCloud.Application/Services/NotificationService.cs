using AirtimeDistributionCloud.Core.Entities;
using AirtimeDistributionCloud.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace AirtimeDistributionCloud.Application.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDealerRepository _dealerRepository;
    private readonly IAirtimeTransferRepository _transferRepository;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IUnitOfWork unitOfWork,
        IDealerRepository dealerRepository,
        IAirtimeTransferRepository transferRepository,
        ILogger<NotificationService> logger)
    {
        _unitOfWork = unitOfWork;
        _dealerRepository = dealerRepository;
        _transferRepository = transferRepository;
        _logger = logger;
    }

    public async Task SendTransferApprovalNotificationAsync(int transferId, CancellationToken cancellationToken = default)
    {
        var settings = await _unitOfWork.Repository<SystemSetting>().GetAllAsync(cancellationToken);
        var settingsDict = settings.ToDictionary(s => s.Key, s => s.Value);

        if (!settingsDict.TryGetValue("TransferNotificationEnabled", out var enabled) ||
            !string.Equals(enabled, "Yes", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var transfer = await _transferRepository.GetByIdAsync(transferId, cancellationToken);
        if (transfer is null) return;

        var dealer = await _dealerRepository.GetDealerWithProductsAsync(transfer.DealerId, cancellationToken);
        if (dealer is null) return;

        var product = await _unitOfWork.Repository<Product>().GetByIdAsync(transfer.ProductId, cancellationToken);
        var branch = await _unitOfWork.Repository<Branch>().GetByIdAsync(transfer.BranchId, cancellationToken);

        var totalBalance = dealer.DealerProducts.Sum(dp => dp.Balance);

        var template = settingsDict.GetValueOrDefault("TransferNotificationTemplate", "");
        if (string.IsNullOrWhiteSpace(template)) return;

        var message = template
            .Replace("{{Name}}", dealer.Name)
            .Replace("{{DealerNumber}}", dealer.DealerNumber)
            .Replace("{{AirtimeTransferId}}", transfer.Id.ToString())
            .Replace("{{Product}}", product?.Name ?? "")
            .Replace("{{AirtimeAmount}}", transfer.LoanAmount.ToString("N2"))
            .Replace("{{CommissionAmount}}", transfer.CommissionAmount.ToString("N2"))
            .Replace("{{Branch}}", branch?.Name ?? "")
            .Replace("{{CreatedDate}}", transfer.CreatedDate.ToString("dd/MM/yyyy HH:mm"))
            .Replace("{{TotalBalance}}", totalBalance.ToString("N2"));

        var methodValue = settingsDict.GetValueOrDefault("TransferNotificationMethod", "Email");
        var methods = methodValue.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var method in methods)
        {
            switch (method)
            {
                case "Email":
                    _logger.LogInformation("Transfer notification (Email) for dealer {DealerNumber}: {Message}",
                        dealer.DealerNumber, message);
                    break;

                case "DealerWhatsApp":
                    if (!string.IsNullOrWhiteSpace(dealer.PhoneNumber))
                    {
                        var phone = dealer.PhoneNumber.TrimStart('+');
                        var waUrl = $"https://wa.me/{phone}?text={Uri.EscapeDataString(message)}";
                        _logger.LogInformation("Transfer notification (WhatsApp) for dealer {DealerNumber}: {Url}",
                            dealer.DealerNumber, waUrl);
                    }
                    else
                    {
                        _logger.LogWarning("Cannot send WhatsApp notification to dealer {DealerNumber}: no phone number",
                            dealer.DealerNumber);
                    }
                    break;

                case "WhatsAppGroup":
                    var groupUrl = settingsDict.GetValueOrDefault("TransferNotificationWhatsAppGroupUrl", "");
                    if (!string.IsNullOrWhiteSpace(groupUrl))
                    {
                        _logger.LogInformation("Transfer notification (WhatsApp Group) for dealer {DealerNumber}. Group: {GroupUrl}, Message: {Message}",
                            dealer.DealerNumber, groupUrl, message);
                    }
                    else
                    {
                        _logger.LogWarning("Cannot send WhatsApp Group notification: no group URL configured");
                    }
                    break;

                default:
                    _logger.LogWarning("Unknown notification method: {Method}", method);
                    break;
            }
        }
    }
}
