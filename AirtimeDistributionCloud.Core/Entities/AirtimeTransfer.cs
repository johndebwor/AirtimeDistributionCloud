using AirtimeDistributionCloud.Core.Enums;

namespace AirtimeDistributionCloud.Core.Entities;

public class AirtimeTransfer : BaseEntity
{
    public int BranchId { get; set; }
    public Branch Branch { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int DealerId { get; set; }
    public Dealer Dealer { get; set; } = null!;

    public string TransferNumber { get; set; } = "";

    public decimal Amount { get; set; }
    public decimal LoanAmount { get; set; }
    public decimal CommissionAmount { get; set; }
    public TransferType TransferType { get; set; }

    public DepositStatus Status { get; set; } = DepositStatus.Pending;
    public string? ApprovedByUserId { get; set; }
    public string? ApprovalNotes { get; set; }

    public string? CancelledByUserId { get; set; }
    public DateTime? CancelledDate { get; set; }
    public string? CancellationReason { get; set; }
}
