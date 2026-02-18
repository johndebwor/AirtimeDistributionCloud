using AirtimeDistributionCloud.Application.DTOs;

namespace AirtimeDistributionCloud.Application.Services;

public interface IBranchService
{
    Task<IReadOnlyList<BranchDto>> GetAllBranchesAsync(CancellationToken cancellationToken = default);
    Task<BranchDto?> GetBranchByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<BranchDto> CreateBranchAsync(CreateBranchRequest request, CancellationToken cancellationToken = default);
    Task<BranchDto> UpdateBranchAsync(UpdateBranchRequest request, CancellationToken cancellationToken = default);
}
