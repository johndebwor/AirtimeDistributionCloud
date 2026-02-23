using AirtimeDistributionCloud.Core.Enums;

namespace AirtimeDistributionCloud.Core.Entities;

public class Expense : BaseEntity
{
    public int BranchId { get; set; }
    public Branch Branch { get; set; } = null!;

    public string Description { get; set; } = string.Empty;
    public int ExpenseCategoryId { get; set; }
    public ExpenseCategory ExpenseCategory { get; set; } = null!;
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; }
    public DepositStatus Status { get; set; } = DepositStatus.Pending;

    public string? ApprovedByUserId { get; set; }
    public string? ApprovalNotes { get; set; }
    public string? ReceiptNumber { get; set; }
    public string? ReceiptImagePath { get; set; }
}
