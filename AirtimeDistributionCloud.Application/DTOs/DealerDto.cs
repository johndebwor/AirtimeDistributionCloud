using AirtimeDistributionCloud.Core.Enums;

namespace AirtimeDistributionCloud.Application.DTOs;

public record DealerDto(
    int Id, string DealerNumber, DealerType Type, string Name, string? Gender,
    string? IdType, string? IdTypeSpecification,
    string? IDNumber, string? CompanyRegNumber, string? Nationality,
    string? State, string? County, string? PhysicalAddress,
    string? PhoneNumber, string? DocumentPath, bool IsActive, string? PhotoPath = null);

public record CreateDealerRequest(
    DealerType Type, string Name, string? Gender,
    string? IdType, string? IdTypeSpecification,
    string? IDNumber, string? CompanyRegNumber, string? Nationality,
    string? State, string? County, string? PhysicalAddress,
    string? PhoneNumber, string? DocumentPath, string? PhotoPath = null);

public record UpdateDealerRequest(
    int Id, DealerType Type, string Name, string? Gender,
    string? IdType, string? IdTypeSpecification,
    string? IDNumber, string? CompanyRegNumber, string? Nationality,
    string? State, string? County, string? PhysicalAddress,
    string? PhoneNumber, string? DocumentPath, string? PhotoPath = null);

public record UpdateCommissionRequest(int DealerProductId, decimal CommissionRate);
