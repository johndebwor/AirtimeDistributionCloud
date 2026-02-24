namespace AirtimeDistributionCloud.Core.Entities;

public class AssetMaintenance : BaseEntity
{
    public int AssetId { get; set; }
    public Asset Asset { get; set; } = null!;

    public DateTime MaintenanceDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public DateTime? NextScheduledDate { get; set; }
    public string? PerformedBy { get; set; }
    public string? Notes { get; set; }
}
