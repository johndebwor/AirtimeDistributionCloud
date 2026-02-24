namespace AirtimeDistributionCloud.Core.Entities;

public class AssetAssignment : BaseEntity
{
    public int AssetId { get; set; }
    public Asset Asset { get; set; } = null!;

    // Assignment type: Individual, Branch, Department, Office
    public string AssignmentType { get; set; } = "Branch";

    public int? BranchId { get; set; }
    public Branch? Branch { get; set; }
    public string? AssignedToUserId { get; set; }

    // Contact info (Individual name or ReceivedBy name for Branch/Dept/Office)
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }

    // Department and Office fields
    public string? DepartmentName { get; set; }
    public string? OfficeName { get; set; }
    public string? Location { get; set; }

    public DateTime AssignedDate { get; set; }
    public DateTime? ReturnedDate { get; set; }
    public string? Notes { get; set; }
}
