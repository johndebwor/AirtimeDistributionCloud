using AirtimeDistributionCloud.Application.DTOs;
using AirtimeDistributionCloud.Core.Entities;
using AirtimeDistributionCloud.Core.Enums;
using AirtimeDistributionCloud.Core.Interfaces;

namespace AirtimeDistributionCloud.Application.Services;

public class DealerService : IDealerService
{
    private readonly IDealerRepository _dealerRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICashDepositRepository _cashDepositRepository;
    private readonly IAirtimeTransferRepository _transferRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DealerService(
        IDealerRepository dealerRepository,
        IProductRepository productRepository,
        ICashDepositRepository cashDepositRepository,
        IAirtimeTransferRepository transferRepository,
        IUnitOfWork unitOfWork)
    {
        _dealerRepository = dealerRepository;
        _productRepository = productRepository;
        _cashDepositRepository = cashDepositRepository;
        _transferRepository = transferRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<DealerDto>> GetAllDealersAsync(CancellationToken cancellationToken = default)
    {
        var dealers = await _dealerRepository.GetAllAsync(cancellationToken);
        return dealers.Select(d => new DealerDto(d.Id, d.Type, d.Name, d.Gender, d.IDNumber,
            d.CompanyRegNumber, d.Nationality, d.State, d.County, d.PhysicalAddress)).ToList();
    }

    public async Task<DealerDto?> GetDealerByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var d = await _dealerRepository.GetByIdAsync(id, cancellationToken);
        return d is null ? null : new DealerDto(d.Id, d.Type, d.Name, d.Gender, d.IDNumber,
            d.CompanyRegNumber, d.Nationality, d.State, d.County, d.PhysicalAddress);
    }

    public async Task<DealerDto> CreateDealerAsync(CreateDealerRequest request, CancellationToken cancellationToken = default)
    {
        var dealer = new Dealer
        {
            Type = request.Type, Name = request.Name, Gender = request.Gender,
            IDNumber = request.IDNumber, CompanyRegNumber = request.CompanyRegNumber,
            Nationality = request.Nationality, State = request.State,
            County = request.County, PhysicalAddress = request.PhysicalAddress
        };

        await _dealerRepository.AddAsync(dealer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Auto-create DealerProduct for all active products
        var products = await _productRepository.GetActiveProductsAsync(cancellationToken);
        foreach (var product in products)
        {
            var dp = new DealerProduct { DealerId = dealer.Id, ProductId = product.Id, CommissionRate = 0, Balance = 0 };
            await _unitOfWork.Repository<DealerProduct>().AddAsync(dp, cancellationToken);
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new DealerDto(dealer.Id, dealer.Type, dealer.Name, dealer.Gender, dealer.IDNumber,
            dealer.CompanyRegNumber, dealer.Nationality, dealer.State, dealer.County, dealer.PhysicalAddress);
    }

    public async Task<decimal> GetDealerCashBalanceAsync(int dealerId, CancellationToken cancellationToken = default)
    {
        var totalDeposits = await _cashDepositRepository.GetTotalDepositsByDealerAsync(dealerId, cancellationToken);
        var transfers = await _transferRepository.GetByDealerAsync(dealerId, cancellationToken);
        var cashTransfers = transfers.Where(t => t.TransferType == TransferType.Cash).Sum(t => t.Amount);
        return totalDeposits - cashTransfers;
    }
}
