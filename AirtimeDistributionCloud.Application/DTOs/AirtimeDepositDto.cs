using AirtimeDistributionCloud.Core.Enums;

namespace AirtimeDistributionCloud.Application.DTOs;

public record AirtimeDepositDto(
    int Id, int ProductId, string ProductName, decimal Amount,
    decimal BonusPercentage, decimal BonusAmount, decimal TotalAmount,
    DepositStatus Status, DateTime CreatedDate, string CreatedBy);

public record CreateAirtimeDepositRequest(int ProductId, decimal Amount);
