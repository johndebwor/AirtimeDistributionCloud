using AirtimeDistributionCloud.Core.Enums;

namespace AirtimeDistributionCloud.Application.DTOs;

public record ExpenseDto(
    int Id, int BranchId, string BranchName, string Description,
    int ExpenseCategoryId, string CategoryName, decimal Amount, DateTime ExpenseDate,
    DepositStatus Status, string? ReceiptNumber, string? ReceiptImagePath,
    DateTime CreatedDate, string CreatedBy);

public record CreateExpenseRequest(
    int BranchId, string Description, int ExpenseCategoryId,
    decimal Amount, string? ReceiptNumber, string? ReceiptImagePath);

public record ExpenseCategoryDto(
    int Id, string Name, string? Description, bool IsActive, int ExpenseCount);

public record CreateExpenseCategoryRequest(string Name, string? Description);
public record UpdateExpenseCategoryRequest(int Id, string Name, string? Description, bool IsActive);
