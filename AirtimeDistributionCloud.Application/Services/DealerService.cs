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
        return dealers.Select(d => new DealerDto(d.Id, d.DealerNumber, d.Type, d.Name, d.Gender, d.IdType, d.IdTypeSpecification,
            d.IDNumber, d.CompanyRegNumber, d.Nationality, d.State, d.County, d.PhysicalAddress, d.PhoneNumber, d.DocumentPath, d.IsActive, d.PhotoPath)).ToList();
    }

    public async Task<DealerDto?> GetDealerByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var d = await _dealerRepository.GetByIdAsync(id, cancellationToken);
        return d is null ? null : new DealerDto(d.Id, d.DealerNumber, d.Type, d.Name, d.Gender, d.IdType, d.IdTypeSpecification,
            d.IDNumber, d.CompanyRegNumber, d.Nationality, d.State, d.County, d.PhysicalAddress, d.PhoneNumber, d.DocumentPath, d.IsActive, d.PhotoPath);
    }

    public async Task<DealerDto> CreateDealerAsync(CreateDealerRequest request, CancellationToken cancellationToken = default)
    {
        var dealer = new Dealer
        {
            Type = request.Type, Name = request.Name, Gender = request.Gender,
            IdType = request.IdType, IdTypeSpecification = request.IdTypeSpecification,
            IDNumber = request.IDNumber, CompanyRegNumber = request.CompanyRegNumber,
            Nationality = request.Nationality, State = request.State,
            County = request.County, PhysicalAddress = request.PhysicalAddress,
            PhoneNumber = request.PhoneNumber,
            DocumentPath = request.DocumentPath,
            PhotoPath = request.PhotoPath
        };

        await _dealerRepository.AddAsync(dealer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Auto-generate dealer number based on assigned Id
        dealer.DealerNumber = $"DLR-{dealer.Id:D4}";

        // Auto-create DealerProduct for all active products
        var products = await _productRepository.GetActiveProductsAsync(cancellationToken);
        foreach (var product in products)
        {
            var dp = new DealerProduct { DealerId = dealer.Id, ProductId = product.Id, CommissionRate = 0, Balance = 0 };
            await _unitOfWork.Repository<DealerProduct>().AddAsync(dp, cancellationToken);
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new DealerDto(dealer.Id, dealer.DealerNumber, dealer.Type, dealer.Name, dealer.Gender, dealer.IdType, dealer.IdTypeSpecification,
            dealer.IDNumber, dealer.CompanyRegNumber, dealer.Nationality, dealer.State, dealer.County, dealer.PhysicalAddress, dealer.PhoneNumber, dealer.DocumentPath, dealer.IsActive, dealer.PhotoPath);
    }

    public async Task UpdateDealerAsync(UpdateDealerRequest request, CancellationToken cancellationToken = default)
    {
        var dealer = await _unitOfWork.Repository<Dealer>().GetByIdAsync(request.Id, cancellationToken)
            ?? throw new InvalidOperationException("Dealer not found");
        dealer.Type = request.Type;
        dealer.Name = request.Name;
        dealer.Gender = request.Gender;
        dealer.IdType = request.IdType;
        dealer.IdTypeSpecification = request.IdTypeSpecification;
        dealer.IDNumber = request.IDNumber;
        dealer.CompanyRegNumber = request.CompanyRegNumber;
        dealer.Nationality = request.Nationality;
        dealer.State = request.State;
        dealer.County = request.County;
        dealer.PhysicalAddress = request.PhysicalAddress;
        dealer.PhoneNumber = request.PhoneNumber;
        if (request.DocumentPath is not null)
            dealer.DocumentPath = request.DocumentPath;
        if (request.PhotoPath is not null)
            dealer.PhotoPath = request.PhotoPath;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task SetDealerActiveAsync(int dealerId, bool isActive, CancellationToken cancellationToken = default)
    {
        var dealer = await _unitOfWork.Repository<Dealer>().GetByIdAsync(dealerId, cancellationToken)
            ?? throw new InvalidOperationException("Dealer not found");
        dealer.IsActive = isActive;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateCommissionAsync(UpdateCommissionRequest request, CancellationToken cancellationToken = default)
    {
        var dp = await _unitOfWork.Repository<DealerProduct>().GetByIdAsync(request.DealerProductId, cancellationToken)
            ?? throw new InvalidOperationException("Commission record not found");
        dp.CommissionRate = request.CommissionRate;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<decimal> GetDealerCashBalanceAsync(int dealerId, CancellationToken cancellationToken = default)
    {
        var totalDeposits = await _cashDepositRepository.GetTotalDepositsByDealerAsync(dealerId, cancellationToken);
        var dps = await _unitOfWork.Repository<DealerProduct>().FindAsync(dp => dp.DealerId == dealerId, cancellationToken);
        var airtimeBalance = dps.Sum(dp => dp.Balance);
        return totalDeposits - airtimeBalance;
    }

    public async Task<IReadOnlyDictionary<int, decimal>> GetAllDealerCashBalancesAsync(CancellationToken cancellationToken = default)
    {
        var allDeposits = await _cashDepositRepository.GetAllAsync(cancellationToken);
        var allDealerProducts = await _unitOfWork.Repository<DealerProduct>().GetAllAsync(cancellationToken);

        var depositsByDealer = allDeposits
            .GroupBy(d => d.DealerId)
            .ToDictionary(g => g.Key, g => g.Sum(d => d.Amount));

        var airtimeByDealer = allDealerProducts
            .GroupBy(dp => dp.DealerId)
            .ToDictionary(g => g.Key, g => g.Sum(dp => dp.Balance));

        var allIds = depositsByDealer.Keys.Union(airtimeByDealer.Keys);
        return allIds.ToDictionary(
            id => id,
            id => depositsByDealer.GetValueOrDefault(id) - airtimeByDealer.GetValueOrDefault(id));
    }

    public async Task<IReadOnlyList<DealerProductRateDto>> GetAllDealerProductRatesAsync(CancellationToken cancellationToken = default)
    {
        var dps = await _unitOfWork.Repository<DealerProduct>().GetAllAsync(cancellationToken);
        return dps.Select(dp => new DealerProductRateDto(dp.DealerId, dp.ProductId, dp.CommissionRate)).ToList();
    }
}
