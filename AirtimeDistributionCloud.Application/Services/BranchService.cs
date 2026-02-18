using AirtimeDistributionCloud.Application.DTOs;
using AirtimeDistributionCloud.Core.Entities;
using AirtimeDistributionCloud.Core.Interfaces;

namespace AirtimeDistributionCloud.Application.Services;

public class BranchService : IBranchService
{
    private readonly IUnitOfWork _unitOfWork;

    public BranchService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<IReadOnlyList<BranchDto>> GetAllBranchesAsync(CancellationToken cancellationToken = default)
    {
        var branches = await _unitOfWork.Repository<Branch>().GetAllAsync(cancellationToken);
        return branches.Select(b => new BranchDto(b.Id, b.Name, b.Location, b.IsActive)).ToList();
    }

    public async Task<BranchDto?> GetBranchByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var b = await _unitOfWork.Repository<Branch>().GetByIdAsync(id, cancellationToken);
        return b is null ? null : new BranchDto(b.Id, b.Name, b.Location, b.IsActive);
    }

    public async Task<BranchDto> CreateBranchAsync(CreateBranchRequest request, CancellationToken cancellationToken = default)
    {
        var branch = new Branch { Name = request.Name, Location = request.Location, IsActive = true };
        await _unitOfWork.Repository<Branch>().AddAsync(branch, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new BranchDto(branch.Id, branch.Name, branch.Location, branch.IsActive);
    }

    public async Task<BranchDto> UpdateBranchAsync(UpdateBranchRequest request, CancellationToken cancellationToken = default)
    {
        var branch = await _unitOfWork.Repository<Branch>().GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Branch with Id {request.Id} not found");
        branch.Name = request.Name;
        branch.Location = request.Location;
        branch.IsActive = request.IsActive;
        await _unitOfWork.Repository<Branch>().UpdateAsync(branch, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new BranchDto(branch.Id, branch.Name, branch.Location, branch.IsActive);
    }
}
