using System.Net;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using AirtimeDistributionCloud.Core.Entities;
using AirtimeDistributionCloud.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace AirtimeDistributionCloud.Application.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDealerRepository _dealerRepository;
    private readonly IAirtimeTransferRepository _transferRepository;
    private readonly ICashDepositRepository _cashDepositRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IUnitOfWork unitOfWork,
        IDealerRepository dealerRepository,
        IAirtimeTransferRepository transferRepository,
        ICashDepositRepository cashDepositRepository,
        IHttpClientFactory httpClientFactory,
        ILogger<NotificationService> logger)
    {
        _unitOfWork = unitOfWork;
        _dealerRepository = dealerRepository;
        _transferRepository = transferRepository;
        _cashDepositRepository = cashDepositRepository;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<NotificationResult> SendTransferApprovalNotificationAsync(int transferId, CancellationToken cancellationToken = default)
    {
        var whatsAppUrls = new List<string>();
        var whatsAppApiResults = new List<string>();

        var settings = await _unitOfWork.Repository<SystemSetting>().GetAllAsync(cancellationToken);
        var settingsDict = settings.ToDictionary(s => s.Key, s => s.Value);

        if (!settingsDict.TryGetValue("TransferNotificationEnabled", out var enabled) ||
            !string.Equals(enabled, "Yes", StringComparison.OrdinalIgnoreCase))
        {
            return new NotificationResult(whatsAppUrls);
        }

        var transfer = await _transferRepository.GetByIdAsync(transferId, cancellationToken);
        if (transfer is null) return new NotificationResult(whatsAppUrls);

        var dealer = await _dealerRepository.GetDealerWithProductsAsync(transfer.DealerId, cancellationToken);
        if (dealer is null) return new NotificationResult(whatsAppUrls);

        var product = await _unitOfWork.Repository<Product>().GetByIdAsync(transfer.ProductId, cancellationToken);
        var branch = await _unitOfWork.Repository<Branch>().GetByIdAsync(transfer.BranchId, cancellationToken);

        var totalBalance = dealer.DealerProducts.Sum(dp => dp.Balance);
        var totalCashDeposits = await _cashDepositRepository.GetTotalDepositsByDealerAsync(transfer.DealerId, cancellationToken);
        var cashBalance = totalCashDeposits - totalBalance;
        var dealerTransfers = await _transferRepository.GetByDealerAsync(transfer.DealerId, cancellationToken);
        var totalCommission = dealerTransfers
            .Where(t => t.Status == Core.Enums.DepositStatus.Approved)
            .Sum(t => t.CommissionAmount);
        var companyName = settingsDict.GetValueOrDefault("CompanyName", "");

        var template = settingsDict.GetValueOrDefault("TransferNotificationTemplate", "");
        if (string.IsNullOrWhiteSpace(template)) return new NotificationResult(whatsAppUrls);

        var message = template
            .Replace("{{Name}}", dealer.Name)
            .Replace("{{DealerNumber}}", dealer.DealerNumber)
            .Replace("{{AirtimeTransferId}}", transfer.Id.ToString())
            .Replace("{{Product}}", product?.Name ?? "")
            .Replace("{{AirtimeAmount}}", transfer.LoanAmount.ToString("N2"))
            .Replace("{{CommissionAmount}}", transfer.CommissionAmount.ToString("N2"))
            .Replace("{{Branch}}", branch?.Name ?? "")
            .Replace("{{CreatedDate}}", transfer.CreatedDate.ToString("dd/MM/yyyy HH:mm"))
            .Replace("{{TotalBalance}}", totalBalance.ToString("N2"))
            .Replace("{{TotalCommission}}", totalCommission.ToString("N2"))
            .Replace("{{CashBalance}}", cashBalance.ToString("N2"))
            .Replace("{{CompanyName}}", companyName);

        var methodValue = settingsDict.GetValueOrDefault("TransferNotificationMethod", "Email");
        var methods = methodValue.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        string? formattedMessage = null;
        string? whatsAppGroupUrl = null;

        foreach (var method in methods)
        {
            try
            {
                switch (method)
                {
                    case "Email":
                        await SendEmailNotificationAsync(settingsDict, dealer, message, cancellationToken);
                        break;

                    case "DealerWhatsApp":
                        var apiResult = await SendDealerWhatsAppAsync(settingsDict, dealer, message, cancellationToken);
                        if (apiResult != null) whatsAppApiResults.Add(apiResult);
                        break;

                    case "WhatsAppGroup":
                        // Return the formatted message so the UI can show it for copy/paste
                        formattedMessage = message;
                        whatsAppGroupUrl = settingsDict.GetValueOrDefault("TransferNotificationWhatsAppGroupUrl", "");
                        _logger.LogInformation("WhatsApp Group message prepared for transfer {TransferId}", transferId);
                        break;

                    default:
                        _logger.LogWarning("Unknown notification method: {Method}", method);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send {Method} notification for transfer {TransferId}",
                    method, transferId);
            }
        }

        return new NotificationResult(whatsAppUrls, formattedMessage, whatsAppGroupUrl, whatsAppApiResults);
    }

    private async Task SendEmailNotificationAsync(Dictionary<string, string> settings, Dealer dealer, string message, CancellationToken cancellationToken)
    {
        var smtpHost = settings.GetValueOrDefault("SmtpHost", "");
        if (string.IsNullOrWhiteSpace(smtpHost))
        {
            _logger.LogWarning("Cannot send email notification: SMTP server not configured");
            return;
        }

        var smtpPort = int.TryParse(settings.GetValueOrDefault("SmtpPort", "587"), out var port) ? port : 587;
        var smtpUsername = settings.GetValueOrDefault("SmtpUsername", "");
        var smtpPassword = settings.GetValueOrDefault("SmtpPassword", "");
        var senderEmail = settings.GetValueOrDefault("SmtpSenderEmail", "");
        var senderName = settings.GetValueOrDefault("SmtpSenderName", "RasidBase");
        var useSsl = !settings.TryGetValue("SmtpUseSsl", out var ssl) ||
                     string.Equals(ssl, "Yes", StringComparison.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(senderEmail) && string.IsNullOrWhiteSpace(smtpUsername))
        {
            _logger.LogWarning("Cannot send email notification: no sender email configured");
            return;
        }

        var recipientEmail = !string.IsNullOrWhiteSpace(senderEmail) ? senderEmail : smtpUsername;

        using var smtp = new SmtpClient(smtpHost)
        {
            Port = smtpPort,
            EnableSsl = useSsl
        };

        if (!string.IsNullOrWhiteSpace(smtpUsername))
            smtp.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

        var from = new MailAddress(
            string.IsNullOrWhiteSpace(senderEmail) ? smtpUsername : senderEmail,
            senderName);

        var mailMessage = new MailMessage(from, new MailAddress(recipientEmail))
        {
            Subject = $"Transfer Approved - {dealer.Name} ({dealer.DealerNumber})",
            Body = message,
            IsBodyHtml = false
        };

        await smtp.SendMailAsync(mailMessage, cancellationToken);
        _logger.LogInformation("Email notification sent for dealer {DealerNumber} to {Recipient}",
            dealer.DealerNumber, recipientEmail);
    }

    private async Task<string?> SendDealerWhatsAppAsync(Dictionary<string, string> settings, Dealer dealer, string message, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dealer.PhoneNumber))
        {
            _logger.LogWarning("Cannot send WhatsApp notification to dealer {DealerNumber}: no phone number",
                dealer.DealerNumber);
            return null;
        }

        var accessToken = settings.GetValueOrDefault("WhatsAppApiAccessToken", "");
        var phoneNumberId = settings.GetValueOrDefault("WhatsAppApiPhoneNumberId", "");

        if (string.IsNullOrWhiteSpace(accessToken) || string.IsNullOrWhiteSpace(phoneNumberId))
        {
            _logger.LogWarning("Cannot send WhatsApp API notification: API credentials not configured");
            return null;
        }

        var phone = dealer.PhoneNumber.TrimStart('+');
        var payload = new
        {
            messaging_product = "whatsapp",
            to = phone,
            type = "text",
            text = new { body = message }
        };

        using var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(
            $"https://graph.facebook.com/v21.0/{phoneNumberId}/messages",
            content,
            cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation("WhatsApp API message sent to dealer {DealerNumber} ({Phone})",
                dealer.DealerNumber, phone);
            return $"Sent to {dealer.Name} ({dealer.DealerNumber})";
        }

        var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogError("WhatsApp API failed for dealer {DealerNumber}: {StatusCode} - {Error}",
            dealer.DealerNumber, response.StatusCode, errorBody);
        return null;
    }

}
