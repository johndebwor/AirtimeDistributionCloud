namespace AirtimeDistributionCloud.Core.Entities;

public class AssetAssignment : BaseEntity
{
    public int AssetId { get; set; }
    public Asset Asset { get; set; } = null!;

    public int? BranchId { get; set; }
    public Branch? Branch { get; set; }
    public string? AssignedToUserId { get; set; }

    public DateTime AssignedDate { get; set; }
    public DateTime? ReturnedDate { get; set; }
    public string? Notes { get; set; }
}
