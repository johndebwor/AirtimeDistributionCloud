namespace AirtimeDistributionCloud.Core.Entities;

public class Branch : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    public ICollection<AirtimeTransfer> AirtimeTransfers { get; set; } = new List<AirtimeTransfer>();
    public ICollection<CashDeposit> CashDeposits { get; set; } = new List<CashDeposit>();
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}
