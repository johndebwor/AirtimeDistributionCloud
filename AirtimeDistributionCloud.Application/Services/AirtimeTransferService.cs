using AirtimeDistributionCloud.Application.DTOs;
using AirtimeDistributionCloud.Core.Entities;
using AirtimeDistributionCloud.Core.Enums;
using AirtimeDistributionCloud.Core.Interfaces;

namespace AirtimeDistributionCloud.Application.Services;

public class AirtimeTransferService : IAirtimeTransferService
{
    private readonly IAirtimeTransferRepository _transferRepository;
    private readonly IProductRepository _productRepository;
    private readonly IDealerRepository _dealerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;

    public AirtimeTransferService(
        IAirtimeTransferRepository transferRepository,
        IProductRepository productRepository,
        IDealerRepository dealerRepository,
        IUnitOfWork unitOfWork,
        INotificationService notificationService)
    {
        _transferRepository = transferRepository;
        _productRepository = productRepository;
        _dealerRepository = dealerRepository;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task<IReadOnlyList<AirtimeTransferDto>> GetAllTransfersAsync(CancellationToken cancellationToken = default)
    {
        var transfers = await _transferRepository.GetAllAsync(cancellationToken);
        var products = (await _productRepository.GetAllAsync(cancellationToken)).ToDictionary(p => p.Id);
        var dealers = (await _dealerRepository.GetAllAsync(cancellationToken)).ToDictionary(d => d.Id);
        var branches = (await _unitOfWork.Repository<Branch>().GetAllAsync(cancellationToken)).ToDictionary(b => b.Id);

        return transfers.Select(t => new AirtimeTransferDto(
            t.Id, t.TransferNumber, t.BranchId, branches.GetValueOrDefault(t.BranchId)?.Name ?? "",
            t.ProductId, products.GetValueOrDefault(t.ProductId)?.Name ?? "",
            t.DealerId, dealers.GetValueOrDefault(t.DealerId)?.Name ?? "",
            t.Amount, t.LoanAmount, t.CommissionAmount, t.TransferType, t.Status, t.CreatedDate)).ToList();
    }

    public async Task<IReadOnlyList<AirtimeTransferDto>> GetTransfersByDealerAsync(int dealerId, CancellationToken cancellationToken = default)
    {
        var transfers = await _transferRepository.GetByDealerAsync(dealerId, cancellationToken);
        return transfers.Select(t => new AirtimeTransferDto(
            t.Id, t.TransferNumber, t.BranchId, t.Branch?.Name ?? "", t.ProductId, t.Product?.Name ?? "",
            t.DealerId, "", t.Amount, t.LoanAmount, t.CommissionAmount, t.TransferType, t.Status, t.CreatedDate)).ToList();
    }

    public async Task<IReadOnlyList<AirtimeTransferDto>> GetTransfersByStatusAsync(DepositStatus status, CancellationToken cancellationToken = default)
    {
        var transfers = await _transferRepository.GetByStatusAsync(status, cancellationToken);
        return transfers.Select(t => new AirtimeTransferDto(
            t.Id, t.TransferNumber, t.BranchId, t.Branch?.Name ?? "", t.ProductId, t.Product?.Name ?? "",
            t.DealerId, t.Dealer?.Name ?? "", t.Amount, t.LoanAmount, t.CommissionAmount, t.TransferType, t.Status, t.CreatedDate)).ToList();
    }

    public async Task<AirtimeTransferDto> CreateTransferAsync(CreateAirtimeTransferRequest request, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken)
            ?? throw new KeyNotFoundException($"Product not found");
        var dealer = await _dealerRepository.GetDealerWithProductsAsync(request.DealerId, cancellationToken)
            ?? throw new KeyNotFoundException($"Dealer not found");
        var branch = await _unitOfWork.Repository<Branch>().GetByIdAsync(request.BranchId, cancellationToken)
            ?? throw new KeyNotFoundException($"Branch not found");

        // Calculate commission split
        var dealerProducts = await _unitOfWork.Repository<DealerProduct>()
            .FindAsync(dp => dp.DealerId == request.DealerId && dp.ProductId == request.ProductId, cancellationToken);
        var dealerProduct = dealerProducts.FirstOrDefault();
        var commissionRate = dealerProduct?.CommissionRate ?? 0;
        var loanAmount = commissionRate > 0 ? request.Amount / (1 + commissionRate / 100m) : request.Amount;
        var commissionAmount = request.Amount - loanAmount;

        if (product.AirtimeAccountBalance < request.Amount)
            throw new InvalidOperationException("Insufficient product airtime balance");

        var transferNumber = $"TRF-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";

        var transfer = new AirtimeTransfer
        {
            TransferNumber = transferNumber,
            BranchId = request.BranchId,
            ProductId = request.ProductId,
            DealerId = request.DealerId,
            Amount = request.Amount,
            LoanAmount = loanAmount,
            CommissionAmount = commissionAmount,
            TransferType = request.TransferType,
            Status = DepositStatus.Pending
        };

        await _transferRepository.AddAsync(transfer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AirtimeTransferDto(transfer.Id, transfer.TransferNumber, transfer.BranchId, branch.Name,
            transfer.ProductId, product.Name, transfer.DealerId, dealer.Name,
            transfer.Amount, transfer.LoanAmount, transfer.CommissionAmount,
            transfer.TransferType, transfer.Status, transfer.CreatedDate);
    }

    public async Task<(AirtimeTransferDto Transfer, NotificationResult Notification)> ApproveTransferAsync(int transferId, string approvedByUserId, string notes, CancellationToken cancellationToken = default)
    {
        var transfer = await _transferRepository.GetByIdAsync(transferId, cancellationToken)
            ?? throw new KeyNotFoundException($"Transfer with Id {transferId} not found");

        if (transfer.Status != DepositStatus.Pending)
            throw new InvalidOperationException("Only pending transfers can be approved");

        var product = await _productRepository.GetByIdAsync(transfer.ProductId, cancellationToken)
            ?? throw new KeyNotFoundException("Product not found");

        if (product.AirtimeAccountBalance < transfer.Amount)
            throw new InvalidOperationException("Insufficient product airtime balance");

        var dealer = await _dealerRepository.GetDealerWithProductsAsync(transfer.DealerId, cancellationToken)
            ?? throw new KeyNotFoundException("Dealer not found");

        transfer.Status = DepositStatus.Approved;
        transfer.ApprovedByUserId = approvedByUserId;
        transfer.ApprovalNotes = notes;

        product.AirtimeAccountBalance -= transfer.Amount;

        var dealerProduct = dealer.DealerProducts.FirstOrDefault(dp => dp.ProductId == transfer.ProductId);
        if (dealerProduct != null)
        {
            dealerProduct.Balance += transfer.LoanAmount;
            dealerProduct.CommissionBalance += transfer.CommissionAmount;
        }

        await _transferRepository.UpdateAsync(transfer, cancellationToken);
        await _productRepository.UpdateAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var notification = new NotificationResult(new List<string>());
        try
        {
            notification = await _notificationService.SendTransferApprovalNotificationAsync(transfer.Id, cancellationToken);
        }
        catch
        {
            // Notification failure should not roll back the approval
        }

        var branch = await _unitOfWork.Repository<Branch>().GetByIdAsync(transfer.BranchId, cancellationToken);
        var dto = new AirtimeTransferDto(transfer.Id, transfer.TransferNumber, transfer.BranchId, branch?.Name ?? "",
            transfer.ProductId, product.Name, transfer.DealerId, dealer.Name,
            transfer.Amount, transfer.LoanAmount, transfer.CommissionAmount,
            transfer.TransferType, transfer.Status, transfer.CreatedDate);
        return (dto, notification);
    }

    public async Task<AirtimeTransferDto> CancelTransferAsync(int transferId, string cancelledByUserId, string reason, CancellationToken cancellationToken = default)
    {
        var transfer = await _transferRepository.GetByIdAsync(transferId, cancellationToken)
            ?? throw new KeyNotFoundException($"Transfer with Id {transferId} not found");

        if (transfer.Status == DepositStatus.Cancelled)
            throw new InvalidOperationException("Transfer is already cancelled");

        if (transfer.Status == DepositStatus.Rejected)
            throw new InvalidOperationException("Rejected transfers cannot be cancelled");

        var wasApproved = transfer.Status == DepositStatus.Approved;

        var product = await _productRepository.GetByIdAsync(transfer.ProductId, cancellationToken)
            ?? throw new KeyNotFoundException("Product not found");
        var dealer = await _dealerRepository.GetDealerWithProductsAsync(transfer.DealerId, cancellationToken)
            ?? throw new KeyNotFoundException("Dealer not found");

        if (wasApproved)
        {
            var dealerProduct = dealer.DealerProducts.FirstOrDefault(dp => dp.ProductId == transfer.ProductId);
            if (dealerProduct != null)
            {
                if (dealerProduct.Balance < transfer.LoanAmount)
                    throw new InvalidOperationException(
                        $"Cannot cancel: dealer balance ({dealerProduct.Balance:C2}) is less than loan amount ({transfer.LoanAmount:C2})");

                if (dealerProduct.CommissionBalance < transfer.CommissionAmount)
                    throw new InvalidOperationException(
                        $"Cannot cancel: dealer commission balance ({dealerProduct.CommissionBalance:C2}) is less than commission ({transfer.CommissionAmount:C2})");

                dealerProduct.Balance -= transfer.LoanAmount;
                dealerProduct.CommissionBalance -= transfer.CommissionAmount;
            }

            product.AirtimeAccountBalance += transfer.Amount;
            await _productRepository.UpdateAsync(product, cancellationToken);
        }

        transfer.Status = DepositStatus.Cancelled;
        transfer.CancelledByUserId = cancelledByUserId;
        transfer.CancelledDate = DateTime.UtcNow;
        transfer.CancellationReason = reason;

        await _transferRepository.UpdateAsync(transfer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var branch = await _unitOfWork.Repository<Branch>().GetByIdAsync(transfer.BranchId, cancellationToken);
        return new AirtimeTransferDto(transfer.Id, transfer.TransferNumber, transfer.BranchId, branch?.Name ?? "",
            transfer.ProductId, product.Name, transfer.DealerId, dealer.Name,
            transfer.Amount, transfer.LoanAmount, transfer.CommissionAmount,
            transfer.TransferType, transfer.Status, transfer.CreatedDate);
    }

    public async Task<AirtimeTransferDto> RejectTransferAsync(int transferId, string rejectedByUserId, string notes, CancellationToken cancellationToken = default)
    {
        var transfer = await _transferRepository.GetByIdAsync(transferId, cancellationToken)
            ?? throw new KeyNotFoundException($"Transfer with Id {transferId} not found");

        if (transfer.Status != DepositStatus.Pending)
            throw new InvalidOperationException("Only pending transfers can be rejected");

        transfer.Status = DepositStatus.Rejected;
        transfer.ApprovedByUserId = rejectedByUserId;
        transfer.ApprovalNotes = notes;

        await _transferRepository.UpdateAsync(transfer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var product = await _productRepository.GetByIdAsync(transfer.ProductId, cancellationToken);
        var dealer = await _dealerRepository.GetByIdAsync(transfer.DealerId, cancellationToken);
        var branch = await _unitOfWork.Repository<Branch>().GetByIdAsync(transfer.BranchId, cancellationToken);
        return new AirtimeTransferDto(transfer.Id, transfer.TransferNumber, transfer.BranchId, branch?.Name ?? "",
            transfer.ProductId, product?.Name ?? "", transfer.DealerId, dealer?.Name ?? "",
            transfer.Amount, transfer.LoanAmount, transfer.CommissionAmount,
            transfer.TransferType, transfer.Status, transfer.CreatedDate);
    }
}
