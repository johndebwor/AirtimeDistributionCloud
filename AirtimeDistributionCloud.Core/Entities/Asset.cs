using AirtimeDistributionCloud.Core.Enums;

namespace AirtimeDistributionCloud.Core.Entities;

public class Asset : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int AssetCategoryId { get; set; }
    public AssetCategory AssetCategory { get; set; } = null!;
    public string? SerialNumber { get; set; }
    public string? AssetTag { get; set; }

    // Location
    public int BranchId { get; set; }
    public Branch Branch { get; set; } = null!;
    public string? AssignedToUserId { get; set; }

    // Purchase info
    public DateTime? PurchaseDate { get; set; }
    public decimal PurchaseValue { get; set; }
    public decimal CurrentValue { get; set; }

    // Depreciation
    public int UsefulLifeMonths { get; set; }
    public string DepreciationMethod { get; set; } = "StraightLine";

    // Condition & Status
    public AssetCondition Condition { get; set; } = AssetCondition.New;
    public AssetStatus Status { get; set; } = AssetStatus.Active;

    // Disposal fields
    public DateTime? DisposalDate { get; set; }
    public string? DisposalReason { get; set; }
    public string? DisposalApprovedByUserId { get; set; }
    public string? DisposalNotes { get; set; }

    // Documentation
    public string? PhotoPath { get; set; }
    public string? DocumentPath { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public ICollection<AssetAssignment> Assignments { get; set; } = new List<AssetAssignment>();
    public ICollection<AssetMaintenance> MaintenanceRecords { get; set; } = new List<AssetMaintenance>();
}
