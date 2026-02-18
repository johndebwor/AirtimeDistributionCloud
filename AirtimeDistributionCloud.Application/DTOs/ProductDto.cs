namespace AirtimeDistributionCloud.Application.DTOs;

public record ProductDto(
    int Id,
    string Name,
    decimal BonusPercentage,
    decimal AirtimeAccountBalance,
    bool IsActive);

public record CreateProductRequest(
    string Name,
    decimal BonusPercentage,
    decimal AirtimeAccountBalance);

public record UpdateProductRequest(
    int Id,
    string Name,
    decimal BonusPercentage,
    decimal AirtimeAccountBalance,
    bool IsActive);
