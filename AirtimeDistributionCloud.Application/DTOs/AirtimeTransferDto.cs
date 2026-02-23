using AirtimeDistributionCloud.Core.Enums;

namespace AirtimeDistributionCloud.Application.DTOs;

public record AirtimeTransferDto(
    int Id, int BranchId, string BranchName, int ProductId,
    string ProductName, int DealerId, string DealerName,
    decimal Amount, decimal LoanAmount, decimal CommissionAmount,
    TransferType TransferType, DepositStatus Status, DateTime CreatedDate);

public record CreateAirtimeTransferRequest(
    int BranchId, int ProductId, int DealerId,
    decimal Amount, TransferType TransferType);

public record DealerProductRateDto(int DealerId, int ProductId, decimal CommissionRate);
