using AirtimeDistributionCloud.Core.Enums;

namespace AirtimeDistributionCloud.Application.DTOs;

public record ExpenseDto(
    int Id, int BranchId, string BranchName, string Description,
    ExpenseCategory Category, decimal Amount, DateTime ExpenseDate,
    DepositStatus Status, string? ReceiptNumber,
    DateTime CreatedDate, string CreatedBy);

public record CreateExpenseRequest(
    int BranchId, string Description, ExpenseCategory Category,
    decimal Amount, string? ReceiptNumber);
