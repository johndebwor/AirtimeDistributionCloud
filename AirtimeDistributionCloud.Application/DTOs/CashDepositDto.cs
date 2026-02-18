namespace AirtimeDistributionCloud.Application.DTOs;

public record CashDepositDto(
    int Id, int BranchId, string BranchName, int DealerId,
    string DealerName, decimal Amount, DateTime DepositDate);

public record CreateCashDepositRequest(int BranchId, int DealerId, decimal Amount);
