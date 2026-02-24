using AirtimeDistributionCloud.Application.DTOs;
using AirtimeDistributionCloud.Core.Entities;
using AirtimeDistributionCloud.Core.Enums;
using AirtimeDistributionCloud.Core.Interfaces;

namespace AirtimeDistributionCloud.Application.Services;

public class AssetService(IUnitOfWork unitOfWork) : IAssetService
{
    // ---- Asset CRUD ----

    public async Task<IReadOnlyList<AssetDto>> GetAllAssetsAsync(CancellationToken ct = default)
    {
        var assets = await unitOfWork.Repository<Asset>().GetAllAsync(ct);
        var categories = (await unitOfWork.Repository<AssetCategory>().GetAllAsync(ct)).ToDictionary(c => c.Id);
        var branches = (await unitOfWork.Repository<Branch>().GetAllAsync(ct)).ToDictionary(b => b.Id);
        var users = (await unitOfWork.Repository<ApplicationUser>().GetAllAsync(ct)).ToDictionary(u => u.Id);

        return assets.Select(a => MapToDto(a, categories, branches, users)).ToList();
    }

    public async Task<AssetDto?> GetAssetByIdAsync(int id, CancellationToken ct = default)
    {
        var asset = await unitOfWork.Repository<Asset>().GetByIdAsync(id, ct);
        if (asset is null) return null;

        var categories = (await unitOfWork.Repository<AssetCategory>().GetAllAsync(ct)).ToDictionary(c => c.Id);
        var branches = (await unitOfWork.Repository<Branch>().GetAllAsync(ct)).ToDictionary(b => b.Id);
        var users = (await unitOfWork.Repository<ApplicationUser>().GetAllAsync(ct)).ToDictionary(u => u.Id);

        return MapToDto(asset, categories, branches, users);
    }

    public async Task<AssetDto> CreateAssetAsync(CreateAssetRequest request, string userId, CancellationToken ct = default)
    {
        var asset = new Asset
        {
            Name = request.Name,
            AssetCategoryId = request.AssetCategoryId,
            SerialNumber = request.SerialNumber,
            AssetTag = request.AssetTag,
            BranchId = request.BranchId,
            AssignedToUserId = request.AssignedToUserId,
            PurchaseDate = request.PurchaseDate,
            PurchaseValue = request.PurchaseValue,
            CurrentValue = request.CurrentValue,
            UsefulLifeMonths = request.UsefulLifeMonths,
            DepreciationMethod = request.DepreciationMethod,
            Condition = request.Condition,
            Status = AssetStatus.Active,
            PhotoPath = request.PhotoPath,
            DocumentPath = request.DocumentPath,
            Notes = request.Notes,
            CreatedBy = userId
        };

        await unitOfWork.Repository<Asset>().AddAsync(asset, ct);
        await unitOfWork.SaveChangesAsync(ct);

        var categories = (await unitOfWork.Repository<AssetCategory>().GetAllAsync(ct)).ToDictionary(c => c.Id);
        var branches = (await unitOfWork.Repository<Branch>().GetAllAsync(ct)).ToDictionary(b => b.Id);
        var users = (await unitOfWork.Repository<ApplicationUser>().GetAllAsync(ct)).ToDictionary(u => u.Id);

        return MapToDto(asset, categories, branches, users);
    }

    public async Task<AssetDto> UpdateAssetAsync(UpdateAssetRequest request, CancellationToken ct = default)
    {
        var asset = await unitOfWork.Repository<Asset>().GetByIdAsync(request.Id, ct)
            ?? throw new KeyNotFoundException($"Asset {request.Id} not found");

        asset.Name = request.Name;
        asset.AssetCategoryId = request.AssetCategoryId;
        asset.SerialNumber = request.SerialNumber;
        asset.AssetTag = request.AssetTag;
        asset.BranchId = request.BranchId;
        asset.AssignedToUserId = request.AssignedToUserId;
        asset.PurchaseDate = request.PurchaseDate;
        asset.PurchaseValue = request.PurchaseValue;
        asset.CurrentValue = request.CurrentValue;
        asset.UsefulLifeMonths = request.UsefulLifeMonths;
        asset.DepreciationMethod = request.DepreciationMethod;
        asset.Condition = request.Condition;
        asset.PhotoPath = request.PhotoPath;
        asset.DocumentPath = request.DocumentPath;
        asset.Notes = request.Notes;

        await unitOfWork.Repository<Asset>().UpdateAsync(asset, ct);
        await unitOfWork.SaveChangesAsync(ct);

        var categories = (await unitOfWork.Repository<AssetCategory>().GetAllAsync(ct)).ToDictionary(c => c.Id);
        var branches = (await unitOfWork.Repository<Branch>().GetAllAsync(ct)).ToDictionary(b => b.Id);
        var users = (await unitOfWork.Repository<ApplicationUser>().GetAllAsync(ct)).ToDictionary(u => u.Id);

        return MapToDto(asset, categories, branches, users);
    }

    public async Task DeleteAssetAsync(int id, CancellationToken ct = default)
    {
        var asset = await unitOfWork.Repository<Asset>().GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Asset {id} not found");

        await unitOfWork.Repository<Asset>().DeleteAsync(asset, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    // ---- Asset Categories ----

    public async Task<IReadOnlyList<AssetCategoryDto>> GetAllCategoriesAsync(CancellationToken ct = default)
    {
        var categories = await unitOfWork.Repository<AssetCategory>().GetAllAsync(ct);
        var assets = await unitOfWork.Repository<Asset>().GetAllAsync(ct);
        var countByCategory = assets.GroupBy(a => a.AssetCategoryId).ToDictionary(g => g.Key, g => g.Count());

        return categories.Select(c => new AssetCategoryDto(
            c.Id, c.Name, c.Description, c.IsActive,
            countByCategory.GetValueOrDefault(c.Id, 0))).ToList();
    }

    public async Task<IReadOnlyList<AssetCategoryDto>> GetActiveCategoriesAsync(CancellationToken ct = default)
    {
        var categories = await unitOfWork.Repository<AssetCategory>().FindAsync(c => c.IsActive, ct);
        return categories.Select(c => new AssetCategoryDto(c.Id, c.Name, c.Description, c.IsActive, 0)).ToList();
    }

    public async Task<AssetCategoryDto> CreateCategoryAsync(CreateAssetCategoryRequest request, CancellationToken ct = default)
    {
        var category = new AssetCategory { Name = request.Name, Description = request.Description, IsActive = true };
        await unitOfWork.Repository<AssetCategory>().AddAsync(category, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return new AssetCategoryDto(category.Id, category.Name, category.Description, category.IsActive, 0);
    }

    public async Task<AssetCategoryDto> UpdateCategoryAsync(UpdateAssetCategoryRequest request, CancellationToken ct = default)
    {
        var category = await unitOfWork.Repository<AssetCategory>().GetByIdAsync(request.Id, ct)
            ?? throw new KeyNotFoundException($"Asset category {request.Id} not found");

        category.Name = request.Name;
        category.Description = request.Description;
        category.IsActive = request.IsActive;
        await unitOfWork.Repository<AssetCategory>().UpdateAsync(category, ct);
        await unitOfWork.SaveChangesAsync(ct);

        var count = (await unitOfWork.Repository<Asset>().FindAsync(a => a.AssetCategoryId == request.Id, ct)).Count;
        return new AssetCategoryDto(category.Id, category.Name, category.Description, category.IsActive, count);
    }

    public async Task SetCategoryActiveAsync(int id, bool isActive, CancellationToken ct = default)
    {
        var category = await unitOfWork.Repository<AssetCategory>().GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Asset category {id} not found");

        category.IsActive = isActive;
        await unitOfWork.Repository<AssetCategory>().UpdateAsync(category, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    // ---- Assignments ----

    public async Task<IReadOnlyList<AssetAssignmentDto>> GetAssignmentHistoryAsync(int assetId, CancellationToken ct = default)
    {
        var assignments = await unitOfWork.Repository<AssetAssignment>().FindAsync(a => a.AssetId == assetId, ct);
        var branches = (await unitOfWork.Repository<Branch>().GetAllAsync(ct)).ToDictionary(b => b.Id);
        var users = (await unitOfWork.Repository<ApplicationUser>().GetAllAsync(ct)).ToDictionary(u => u.Id);
        var asset = await unitOfWork.Repository<Asset>().GetByIdAsync(assetId, ct);

        return assignments.OrderByDescending(a => a.AssignedDate).Select(a => new AssetAssignmentDto(
            a.Id, a.AssetId, asset?.Name ?? "",
            a.BranchId, a.BranchId.HasValue && branches.TryGetValue(a.BranchId.Value, out var b) ? b.Name : null,
            a.AssignedToUserId,
            a.AssignedToUserId != null && users.TryGetValue(a.AssignedToUserId, out var u) ? u.FullName : null,
            a.AssignedDate, a.ReturnedDate, a.Notes,
            a.CreatedDate, a.CreatedBy)).ToList();
    }

    public async Task<AssetAssignmentDto> AssignAssetAsync(CreateAssetAssignmentRequest request, string userId, CancellationToken ct = default)
    {
        var asset = await unitOfWork.Repository<Asset>().GetByIdAsync(request.AssetId, ct)
            ?? throw new KeyNotFoundException($"Asset {request.AssetId} not found");

        // Close any open assignment
        var openAssignments = await unitOfWork.Repository<AssetAssignment>()
            .FindAsync(a => a.AssetId == request.AssetId && a.ReturnedDate == null, ct);
        foreach (var open in openAssignments)
        {
            open.ReturnedDate = DateTime.UtcNow;
            await unitOfWork.Repository<AssetAssignment>().UpdateAsync(open, ct);
        }

        // Create new assignment
        var assignment = new AssetAssignment
        {
            AssetId = request.AssetId,
            BranchId = request.BranchId,
            AssignedToUserId = request.AssignedToUserId,
            AssignedDate = DateTime.UtcNow,
            Notes = request.Notes,
            CreatedBy = userId
        };
        await unitOfWork.Repository<AssetAssignment>().AddAsync(assignment, ct);

        // Update asset current location
        if (request.BranchId.HasValue) asset.BranchId = request.BranchId.Value;
        asset.AssignedToUserId = request.AssignedToUserId;
        await unitOfWork.Repository<Asset>().UpdateAsync(asset, ct);

        await unitOfWork.SaveChangesAsync(ct);

        var branches = (await unitOfWork.Repository<Branch>().GetAllAsync(ct)).ToDictionary(b => b.Id);
        var users = (await unitOfWork.Repository<ApplicationUser>().GetAllAsync(ct)).ToDictionary(u => u.Id);

        return new AssetAssignmentDto(
            assignment.Id, assignment.AssetId, asset.Name,
            assignment.BranchId,
            assignment.BranchId.HasValue && branches.TryGetValue(assignment.BranchId.Value, out var b) ? b.Name : null,
            assignment.AssignedToUserId,
            assignment.AssignedToUserId != null && users.TryGetValue(assignment.AssignedToUserId, out var u) ? u.FullName : null,
            assignment.AssignedDate, assignment.ReturnedDate, assignment.Notes,
            assignment.CreatedDate, assignment.CreatedBy);
    }

    public async Task ReturnAssetAsync(int assignmentId, CancellationToken ct = default)
    {
        var assignment = await unitOfWork.Repository<AssetAssignment>().GetByIdAsync(assignmentId, ct)
            ?? throw new KeyNotFoundException($"Assignment {assignmentId} not found");

        assignment.ReturnedDate = DateTime.UtcNow;
        await unitOfWork.Repository<AssetAssignment>().UpdateAsync(assignment, ct);

        var asset = await unitOfWork.Repository<Asset>().GetByIdAsync(assignment.AssetId, ct);
        if (asset is not null)
        {
            asset.AssignedToUserId = null;
            await unitOfWork.Repository<Asset>().UpdateAsync(asset, ct);
        }

        await unitOfWork.SaveChangesAsync(ct);
    }

    // ---- Maintenance ----

    public async Task<IReadOnlyList<AssetMaintenanceDto>> GetMaintenanceRecordsAsync(int assetId, CancellationToken ct = default)
    {
        var records = await unitOfWork.Repository<AssetMaintenance>().FindAsync(m => m.AssetId == assetId, ct);
        var asset = await unitOfWork.Repository<Asset>().GetByIdAsync(assetId, ct);

        return records.OrderByDescending(m => m.MaintenanceDate).Select(m => new AssetMaintenanceDto(
            m.Id, m.AssetId, asset?.Name ?? "",
            m.MaintenanceDate, m.Description, m.Cost,
            m.NextScheduledDate, m.PerformedBy, m.Notes,
            m.CreatedDate, m.CreatedBy)).ToList();
    }

    public async Task<AssetMaintenanceDto> AddMaintenanceRecordAsync(CreateAssetMaintenanceRequest request, string userId, CancellationToken ct = default)
    {
        var asset = await unitOfWork.Repository<Asset>().GetByIdAsync(request.AssetId, ct)
            ?? throw new KeyNotFoundException($"Asset {request.AssetId} not found");

        var record = new AssetMaintenance
        {
            AssetId = request.AssetId,
            MaintenanceDate = request.MaintenanceDate,
            Description = request.Description,
            Cost = request.Cost,
            NextScheduledDate = request.NextScheduledDate,
            PerformedBy = request.PerformedBy,
            Notes = request.Notes,
            CreatedBy = userId
        };
        await unitOfWork.Repository<AssetMaintenance>().AddAsync(record, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return new AssetMaintenanceDto(
            record.Id, record.AssetId, asset.Name,
            record.MaintenanceDate, record.Description, record.Cost,
            record.NextScheduledDate, record.PerformedBy, record.Notes,
            record.CreatedDate, record.CreatedBy);
    }

    // ---- Disposal ----

    public async Task<AssetDto> DisposeAssetAsync(int assetId, string reason, string notes, string approvedByUserId, CancellationToken ct = default)
    {
        return await SetDisposalStatus(assetId, AssetStatus.Disposed, reason, notes, approvedByUserId, ct);
    }

    public async Task<AssetDto> WriteOffAssetAsync(int assetId, string reason, string notes, string approvedByUserId, CancellationToken ct = default)
    {
        return await SetDisposalStatus(assetId, AssetStatus.WrittenOff, reason, notes, approvedByUserId, ct);
    }

    // ---- Helpers ----

    private async Task<AssetDto> SetDisposalStatus(int assetId, AssetStatus status, string reason, string notes, string approvedByUserId, CancellationToken ct)
    {
        var asset = await unitOfWork.Repository<Asset>().GetByIdAsync(assetId, ct)
            ?? throw new KeyNotFoundException($"Asset {assetId} not found");

        if (asset.Status is AssetStatus.Disposed or AssetStatus.WrittenOff)
            throw new InvalidOperationException($"Asset is already {asset.Status}");

        asset.Status = status;
        asset.DisposalDate = DateTime.UtcNow;
        asset.DisposalReason = reason;
        asset.DisposalNotes = notes;
        asset.DisposalApprovedByUserId = approvedByUserId;
        asset.CurrentValue = 0;

        await unitOfWork.Repository<Asset>().UpdateAsync(asset, ct);
        await unitOfWork.SaveChangesAsync(ct);

        var categories = (await unitOfWork.Repository<AssetCategory>().GetAllAsync(ct)).ToDictionary(c => c.Id);
        var branches = (await unitOfWork.Repository<Branch>().GetAllAsync(ct)).ToDictionary(b => b.Id);
        var users = (await unitOfWork.Repository<ApplicationUser>().GetAllAsync(ct)).ToDictionary(u => u.Id);

        return MapToDto(asset, categories, branches, users);
    }

    private static AssetDto MapToDto(Asset a, Dictionary<int, AssetCategory> categories,
        Dictionary<int, Branch> branches, Dictionary<string, ApplicationUser> users)
    {
        return new AssetDto(
            a.Id, a.Name, a.AssetCategoryId,
            categories.TryGetValue(a.AssetCategoryId, out var cat) ? cat.Name : "",
            a.SerialNumber, a.AssetTag,
            a.BranchId,
            branches.TryGetValue(a.BranchId, out var br) ? br.Name : "",
            a.AssignedToUserId,
            a.AssignedToUserId != null && users.TryGetValue(a.AssignedToUserId, out var usr) ? usr.FullName : null,
            a.PurchaseDate, a.PurchaseValue, a.CurrentValue,
            a.UsefulLifeMonths, a.DepreciationMethod,
            CalculateBookValue(a.PurchaseValue, a.PurchaseDate, a.UsefulLifeMonths, a.DepreciationMethod),
            a.Condition, a.Status,
            a.DisposalDate, a.DisposalReason, a.DisposalNotes,
            a.PhotoPath, a.DocumentPath, a.Notes,
            a.CreatedDate, a.CreatedBy);
    }

    private static decimal CalculateBookValue(decimal purchaseValue, DateTime? purchaseDate, int usefulLifeMonths, string method)
    {
        if (purchaseDate is null || usefulLifeMonths <= 0) return purchaseValue;

        var monthsElapsed = (int)((DateTime.UtcNow - purchaseDate.Value).TotalDays / 30.44);
        if (monthsElapsed <= 0) return purchaseValue;

        if (method == "DecliningBalance")
        {
            var rate = 2.0 / usefulLifeMonths;
            var value = (double)purchaseValue * Math.Pow(1 - rate, monthsElapsed);
            return Math.Max(0, Math.Round((decimal)value, 2));
        }

        // StraightLine (default)
        var depreciationPerMonth = purchaseValue / usefulLifeMonths;
        var totalDepreciation = depreciationPerMonth * Math.Min(monthsElapsed, usefulLifeMonths);
        return Math.Max(0, purchaseValue - totalDepreciation);
    }
}
