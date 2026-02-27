using AirtimeDistributionCloud.Application.DTOs;
using AirtimeDistributionCloud.Core.Entities;
using AirtimeDistributionCloud.Core.Interfaces;

namespace AirtimeDistributionCloud.Application.Services;

public class CashDepositService : ICashDepositService
{
    private readonly ICashDepositRepository _cashDepositRepository;
    private readonly IDealerRepository _dealerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CashDepositService(ICashDepositRepository cashDepositRepository, IDealerRepository dealerRepository, IUnitOfWork unitOfWork)
    {
        _cashDepositRepository = cashDepositRepository;
        _dealerRepository = dealerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<CashDepositDto>> GetAllDepositsAsync(CancellationToken cancellationToken = default)
    {
        var deposits = await _cashDepositRepository.GetAllAsync(cancellationToken);
        var dealers = (await _dealerRepository.GetAllAsync(cancellationToken)).ToDictionary(d => d.Id);
        var branches = (await _unitOfWork.Repository<Branch>().GetAllAsync(cancellationToken)).ToDictionary(b => b.Id);

        return deposits.Select(d => new CashDepositDto(
            d.Id, d.TransactionNumber, d.BranchId, branches.GetValueOrDefault(d.BranchId)?.Name ?? "",
            d.DealerId, dealers.GetValueOrDefault(d.DealerId)?.Name ?? "",
            d.Amount, d.DepositDate)).ToList();
    }

    public async Task<IReadOnlyList<CashDepositDto>> GetDepositsByDealerAsync(int dealerId, CancellationToken cancellationToken = default)
    {
        var deposits = await _cashDepositRepository.GetByDealerAsync(dealerId, cancellationToken);
        var dealer = await _dealerRepository.GetByIdAsync(dealerId, cancellationToken);
        return deposits.Select(d => new CashDepositDto(
            d.Id, d.TransactionNumber, d.BranchId, d.Branch?.Name ?? "", d.DealerId,
            dealer?.Name ?? "", d.Amount, d.DepositDate)).ToList();
    }

    public async Task<CashDepositDto> CreateDepositAsync(CreateCashDepositRequest request, CancellationToken cancellationToken = default)
    {
        var dealer = await _dealerRepository.GetByIdAsync(request.DealerId, cancellationToken)
            ?? throw new KeyNotFoundException("Dealer not found");
        var branch = await _unitOfWork.Repository<Branch>().GetByIdAsync(request.BranchId, cancellationToken)
            ?? throw new KeyNotFoundException("Branch not found");

        var transactionNumber = $"DEP-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";

        var deposit = new CashDeposit
        {
            TransactionNumber = transactionNumber,
            BranchId = request.BranchId,
            DealerId = request.DealerId,
            Amount = request.Amount,
            DepositDate = DateTime.UtcNow
        };

        await _cashDepositRepository.AddAsync(deposit, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CashDepositDto(deposit.Id, deposit.TransactionNumber, deposit.BranchId, branch.Name,
            deposit.DealerId, dealer.Name, deposit.Amount, deposit.DepositDate);
    }
}
