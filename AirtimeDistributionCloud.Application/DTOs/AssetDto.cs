using AirtimeDistributionCloud.Core.Enums;

namespace AirtimeDistributionCloud.Application.DTOs;

// --- Asset ---
public record AssetDto(
    int Id, string Name, int AssetCategoryId, string CategoryName,
    string? SerialNumber, string? AssetTag,
    int BranchId, string BranchName, string? AssignedToUserId, string? AssignedToUserName,
    DateTime? PurchaseDate, decimal PurchaseValue, decimal CurrentValue,
    int UsefulLifeMonths, string DepreciationMethod, decimal BookValue,
    AssetCondition Condition, AssetStatus Status,
    DateTime? DisposalDate, string? DisposalReason, string? DisposalNotes,
    string? PhotoPath, string? DocumentPath, string? Notes,
    DateTime CreatedDate, string CreatedBy);

public record CreateAssetRequest(
    string Name, int AssetCategoryId, string? SerialNumber, string? AssetTag,
    int BranchId, string? AssignedToUserId,
    DateTime? PurchaseDate, decimal PurchaseValue, decimal CurrentValue,
    int UsefulLifeMonths, string DepreciationMethod,
    AssetCondition Condition, string? PhotoPath, string? DocumentPath, string? Notes);

public record UpdateAssetRequest(
    int Id, string Name, int AssetCategoryId, string? SerialNumber, string? AssetTag,
    int BranchId, string? AssignedToUserId,
    DateTime? PurchaseDate, decimal PurchaseValue, decimal CurrentValue,
    int UsefulLifeMonths, string DepreciationMethod,
    AssetCondition Condition, string? PhotoPath, string? DocumentPath, string? Notes);

// --- Asset Category ---
public record AssetCategoryDto(int Id, string Name, string? Description, bool IsActive, int AssetCount);
public record CreateAssetCategoryRequest(string Name, string? Description);
public record UpdateAssetCategoryRequest(int Id, string Name, string? Description, bool IsActive);

// --- Asset Assignment ---
public record AssetAssignmentDto(
    int Id, int AssetId, string AssetName,
    int? BranchId, string? BranchName, string? AssignedToUserId, string? AssignedToUserName,
    DateTime AssignedDate, DateTime? ReturnedDate, string? Notes,
    DateTime CreatedDate, string CreatedBy);

public record CreateAssetAssignmentRequest(int AssetId, int? BranchId, string? AssignedToUserId, string? Notes);

// --- Asset Maintenance ---
public record AssetMaintenanceDto(
    int Id, int AssetId, string AssetName,
    DateTime MaintenanceDate, string Description, decimal Cost,
    DateTime? NextScheduledDate, string? PerformedBy, string? Notes,
    DateTime CreatedDate, string CreatedBy);

public record CreateAssetMaintenanceRequest(
    int AssetId, DateTime MaintenanceDate, string Description,
    decimal Cost, DateTime? NextScheduledDate, string? PerformedBy, string? Notes);
