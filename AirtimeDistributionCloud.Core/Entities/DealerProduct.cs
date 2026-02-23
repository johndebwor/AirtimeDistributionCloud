namespace AirtimeDistributionCloud.Core.Entities;

public class DealerProduct : BaseEntity
{
    public int DealerId { get; set; }
    public Dealer Dealer { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public decimal CommissionRate { get; set; }
    public decimal Balance { get; set; }
    public decimal CommissionBalance { get; set; }
}
