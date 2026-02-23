using AirtimeDistributionCloud.Application.DTOs;
using AirtimeDistributionCloud.Core.Entities;
using AirtimeDistributionCloud.Core.Interfaces;

namespace AirtimeDistributionCloud.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<ProductDto>> GetAllProductsAsync(CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetAllAsync(cancellationToken);
        return products.Select(p => new ProductDto(p.Id, p.Name, p.BonusPercentage, p.AirtimeAccountBalance, p.IsActive)).ToList();
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken);
        return product is null ? null : new ProductDto(product.Id, product.Name, product.BonusPercentage, product.AirtimeAccountBalance, product.IsActive);
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = new Product
        {
            Name = request.Name,
            BonusPercentage = request.BonusPercentage,
            AirtimeAccountBalance = request.AirtimeAccountBalance,
            IsActive = true
        };

        await _productRepository.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Auto-create DealerProduct for all active dealers (mirrors CreateDealerAsync logic)
        var dealers = await _unitOfWork.Repository<Dealer>().FindAsync(d => d.IsActive, cancellationToken);
        foreach (var dealer in dealers)
        {
            var dp = new DealerProduct { DealerId = dealer.Id, ProductId = product.Id, CommissionRate = 0, Balance = 0, CommissionBalance = 0 };
            await _unitOfWork.Repository<DealerProduct>().AddAsync(dp, cancellationToken);
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ProductDto(product.Id, product.Name, product.BonusPercentage, product.AirtimeAccountBalance, product.IsActive);
    }

    public async Task<ProductDto> UpdateProductAsync(UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Product with Id {request.Id} not found");

        product.Name = request.Name;
        product.BonusPercentage = request.BonusPercentage;
        product.AirtimeAccountBalance = request.AirtimeAccountBalance;
        product.IsActive = request.IsActive;

        await _productRepository.UpdateAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ProductDto(product.Id, product.Name, product.BonusPercentage, product.AirtimeAccountBalance, product.IsActive);
    }
}
