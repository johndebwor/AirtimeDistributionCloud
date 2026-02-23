using AirtimeDistributionCloud.Core.Enums;

namespace AirtimeDistributionCloud.Core.Entities;

public class AirtimeDeposit : BaseEntity
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public decimal Amount { get; set; }
    public decimal BonusPercentage { get; set; }
    public decimal BonusAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public DepositStatus Status { get; set; } = DepositStatus.Pending;

    public string CreatedByUserId { get; set; } = string.Empty;
    public string? ApprovedByUserId { get; set; }
    public string? ApprovalNotes { get; set; }

    public string? CancelledByUserId { get; set; }
    public DateTime? CancelledDate { get; set; }
    public string? CancellationReason { get; set; }
}
