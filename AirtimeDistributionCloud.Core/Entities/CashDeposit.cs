namespace AirtimeDistributionCloud.Core.Entities;

public class CashDeposit : BaseEntity
{
    public int BranchId { get; set; }
    public Branch Branch { get; set; } = null!;

    public int DealerId { get; set; }
    public Dealer Dealer { get; set; } = null!;

    public string TransactionNumber { get; set; } = "";

    public decimal Amount { get; set; }
    public DateTime DepositDate { get; set; }
}
