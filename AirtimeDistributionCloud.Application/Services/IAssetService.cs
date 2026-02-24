using AirtimeDistributionCloud.Application.DTOs;
using AirtimeDistributionCloud.Core.Enums;

namespace AirtimeDistributionCloud.Application.Services;

public interface IAssetService
{
    // Asset CRUD
    Task<IReadOnlyList<AssetDto>> GetAllAssetsAsync(CancellationToken ct = default);
    Task<AssetDto?> GetAssetByIdAsync(int id, CancellationToken ct = default);
    Task<AssetDto> CreateAssetAsync(CreateAssetRequest request, string userId, CancellationToken ct = default);
    Task<AssetDto> UpdateAssetAsync(UpdateAssetRequest request, CancellationToken ct = default);
    Task DeleteAssetAsync(int id, CancellationToken ct = default);

    // Asset Categories
    Task<IReadOnlyList<AssetCategoryDto>> GetAllCategoriesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<AssetCategoryDto>> GetActiveCategoriesAsync(CancellationToken ct = default);
    Task<AssetCategoryDto> CreateCategoryAsync(CreateAssetCategoryRequest request, CancellationToken ct = default);
    Task<AssetCategoryDto> UpdateCategoryAsync(UpdateAssetCategoryRequest request, CancellationToken ct = default);
    Task SetCategoryActiveAsync(int id, bool isActive, CancellationToken ct = default);

    // Assignments
    Task<IReadOnlyList<AssetAssignmentDto>> GetAssignmentHistoryAsync(int assetId, CancellationToken ct = default);
    Task<AssetAssignmentDto> AssignAssetAsync(CreateAssetAssignmentRequest request, string userId, CancellationToken ct = default);
    Task ReturnAssetAsync(int assignmentId, CancellationToken ct = default);

    // Maintenance
    Task<IReadOnlyList<AssetMaintenanceDto>> GetMaintenanceRecordsAsync(int assetId, CancellationToken ct = default);
    Task<AssetMaintenanceDto> AddMaintenanceRecordAsync(CreateAssetMaintenanceRequest request, string userId, CancellationToken ct = default);

    // Disposal
    Task<AssetDto> DisposeAssetAsync(int assetId, string reason, string notes, string approvedByUserId, CancellationToken ct = default);
    Task<AssetDto> WriteOffAssetAsync(int assetId, string reason, string notes, string approvedByUserId, CancellationToken ct = default);
}
