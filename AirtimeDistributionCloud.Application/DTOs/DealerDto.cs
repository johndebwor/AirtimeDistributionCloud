using AirtimeDistributionCloud.Core.Enums;

namespace AirtimeDistributionCloud.Application.DTOs;

public record DealerDto(
    int Id, DealerType Type, string Name, string? Gender,
    string? IDNumber, string? CompanyRegNumber, string? Nationality,
    string? State, string? County, string? PhysicalAddress);

public record CreateDealerRequest(
    DealerType Type, string Name, string? Gender,
    string? IDNumber, string? CompanyRegNumber, string? Nationality,
    string? State, string? County, string? PhysicalAddress);
