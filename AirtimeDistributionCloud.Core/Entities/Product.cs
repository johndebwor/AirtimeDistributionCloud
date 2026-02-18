namespace AirtimeDistributionCloud.Core.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal BonusPercentage { get; set; }
    public decimal AirtimeAccountBalance { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<DealerProduct> DealerProducts { get; set; } = new List<DealerProduct>();
    public ICollection<AirtimeDeposit> AirtimeDeposits { get; set; } = new List<AirtimeDeposit>();
    public ICollection<AirtimeTransfer> AirtimeTransfers { get; set; } = new List<AirtimeTransfer>();
}
