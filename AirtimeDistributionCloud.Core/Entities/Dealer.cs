using AirtimeDistributionCloud.Core.Enums;

namespace AirtimeDistributionCloud.Core.Entities;

public class Dealer : BaseEntity
{
    public DealerType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Gender { get; set; }
    public string? IdType { get; set; }
    public string? IdTypeSpecification { get; set; }
    public string? IDNumber { get; set; }
    public string? CompanyRegNumber { get; set; }
    public string? Nationality { get; set; }
    public string? State { get; set; }
    public string? County { get; set; }
    public string? PhysicalAddress { get; set; }

    public ICollection<DealerProduct> DealerProducts { get; set; } = new List<DealerProduct>();
    public ICollection<AirtimeTransfer> AirtimeTransfers { get; set; } = new List<AirtimeTransfer>();
    public ICollection<CashDeposit> CashDeposits { get; set; } = new List<CashDeposit>();
}
