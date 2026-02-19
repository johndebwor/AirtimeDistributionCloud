using AirtimeDistributionCloud.Core.Enums;

namespace AirtimeDistributionCloud.Application.DTOs;

public record DealerDto(
    int Id, DealerType Type, string Name, string? Gender,
    string? IdType, string? IdTypeSpecification,
    string? IDNumber, string? CompanyRegNumber, string? Nationality,
    string? State, string? County, string? PhysicalAddress);

public record CreateDealerRequest(
    DealerType Type, string Name, string? Gender,
    string? IdType, string? IdTypeSpecification,
    string? IDNumber, string? CompanyRegNumber, string? Nationality,
    string? State, string? County, string? PhysicalAddress);

public record UpdateCommissionRequest(int DealerProductId, decimal CommissionRate);
