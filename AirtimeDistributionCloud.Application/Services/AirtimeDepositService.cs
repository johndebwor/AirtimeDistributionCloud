using AirtimeDistributionCloud.Application.DTOs;
using AirtimeDistributionCloud.Core.Entities;
using AirtimeDistributionCloud.Core.Enums;
using AirtimeDistributionCloud.Core.Interfaces;

namespace AirtimeDistributionCloud.Application.Services;

public class AirtimeDepositService : IAirtimeDepositService
{
    private readonly IAirtimeDepositRepository _depositRepository;
    private readonly IProductRepository _productRepository;
    private readonly ISystemSettingRepository _systemSettingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AirtimeDepositService(IAirtimeDepositRepository depositRepository, IProductRepository productRepository, ISystemSettingRepository systemSettingRepository, IUnitOfWork unitOfWork)
    {
        _depositRepository = depositRepository;
        _productRepository = productRepository;
        _systemSettingRepository = systemSettingRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<AirtimeDepositDto>> GetAllDepositsAsync(CancellationToken cancellationToken = default)
    {
        var deposits = await _depositRepository.GetAllAsync(cancellationToken);
        var products = (await _productRepository.GetAllAsync(cancellationToken)).ToDictionary(p => p.Id);
        return deposits.Select(d => MapToDto(d, products.GetValueOrDefault(d.ProductId)?.Name ?? "Unknown")).ToList();
    }

    public async Task<IReadOnlyList<AirtimeDepositDto>> GetDepositsByStatusAsync(DepositStatus status, CancellationToken cancellationToken = default)
    {
        var deposits = await _depositRepository.GetByStatusAsync(status, cancellationToken);
        return deposits.Select(d => MapToDto(d, d.Product?.Name ?? "Unknown")).ToList();
    }

    public async Task<AirtimeDepositDto> CreateDepositAsync(CreateAirtimeDepositRequest request, string userId, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken)
            ?? throw new KeyNotFoundException($"Product with Id {request.ProductId} not found");

        var autoApproveSetting = await _systemSettingRepository.GetByKeyAsync("AirtimeDepositAutoApprove", cancellationToken);
        var autoApprove = string.Equals(autoApproveSetting?.Value, "Yes", StringComparison.OrdinalIgnoreCase);

        var bonusAmount = request.Amount * (product.BonusPercentage / 100m);
        var totalAmount = request.Amount + bonusAmount;

        var deposit = new AirtimeDeposit
        {
            ProductId = request.ProductId,
            Amount = request.Amount,
            BonusPercentage = product.BonusPercentage,
            BonusAmount = bonusAmount,
            TotalAmount = totalAmount,
            Status = autoApprove ? DepositStatus.Approved : DepositStatus.Pending,
            CreatedByUserId = userId,
            ApprovedByUserId = autoApprove ? userId : null,
            ApprovalNotes = autoApprove ? "Auto-approved" : null
        };

        if (autoApprove)
            product.AirtimeAccountBalance += totalAmount;

        await _depositRepository.AddAsync(deposit, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(deposit, product.Name);
    }

    public async Task<AirtimeDepositDto> ApproveDepositAsync(int depositId, string approvedByUserId, string notes, CancellationToken cancellationToken = default)
    {
        var deposit = await _depositRepository.GetByIdAsync(depositId, cancellationToken)
            ?? throw new KeyNotFoundException($"Deposit with Id {depositId} not found");

        if (deposit.Status != DepositStatus.Pending)
            throw new InvalidOperationException("Only pending deposits can be approved");

        deposit.Status = DepositStatus.Approved;
        deposit.ApprovedByUserId = approvedByUserId;
        deposit.ApprovalNotes = notes;

        // Add to product balance
        var product = await _productRepository.GetByIdAsync(deposit.ProductId, cancellationToken)
            ?? throw new KeyNotFoundException($"Product not found");
        product.AirtimeAccountBalance += deposit.TotalAmount;

        await _depositRepository.UpdateAsync(deposit, cancellationToken);
        await _productRepository.UpdateAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(deposit, product.Name);
    }

    public async Task<AirtimeDepositDto> RejectDepositAsync(int depositId, string rejectedByUserId, string notes, CancellationToken cancellationToken = default)
    {
        var deposit = await _depositRepository.GetByIdAsync(depositId, cancellationToken)
            ?? throw new KeyNotFoundException($"Deposit with Id {depositId} not found");

        if (deposit.Status != DepositStatus.Pending)
            throw new InvalidOperationException("Only pending deposits can be rejected");

        deposit.Status = DepositStatus.Rejected;
        deposit.ApprovedByUserId = rejectedByUserId;
        deposit.ApprovalNotes = notes;

        await _depositRepository.UpdateAsync(deposit, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var product = await _productRepository.GetByIdAsync(deposit.ProductId, cancellationToken);
        return MapToDto(deposit, product?.Name ?? "Unknown");
    }

    public async Task<AirtimeDepositDto> CancelDepositAsync(int depositId, string cancelledByUserId, string reason, CancellationToken cancellationToken = default)
    {
        var deposit = await _depositRepository.GetByIdAsync(depositId, cancellationToken)
            ?? throw new KeyNotFoundException($"Deposit with Id {depositId} not found");

        if (deposit.Status == DepositStatus.Cancelled)
            throw new InvalidOperationException("Deposit is already cancelled");

        if (deposit.Status == DepositStatus.Rejected)
            throw new InvalidOperationException("Rejected deposits cannot be cancelled");

        var wasApproved = deposit.Status == DepositStatus.Approved;

        var product = await _productRepository.GetByIdAsync(deposit.ProductId, cancellationToken)
            ?? throw new KeyNotFoundException("Product not found");

        if (wasApproved)
        {
            if (product.AirtimeAccountBalance < deposit.TotalAmount)
                throw new InvalidOperationException(
                    $"Cannot cancel: product balance ({product.AirtimeAccountBalance:C2}) is less than deposit total ({deposit.TotalAmount:C2})");

            product.AirtimeAccountBalance -= deposit.TotalAmount;
            await _productRepository.UpdateAsync(product, cancellationToken);
        }

        deposit.Status = DepositStatus.Cancelled;
        deposit.CancelledByUserId = cancelledByUserId;
        deposit.CancelledDate = DateTime.UtcNow;
        deposit.CancellationReason = reason;

        await _depositRepository.UpdateAsync(deposit, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(deposit, product.Name);
    }

    private static AirtimeDepositDto MapToDto(AirtimeDeposit d, string productName) =>
        new(d.Id, d.ProductId, productName, d.Amount, d.BonusPercentage, d.BonusAmount,
            d.TotalAmount, d.Status, d.CreatedDate, d.CreatedBy);
}
