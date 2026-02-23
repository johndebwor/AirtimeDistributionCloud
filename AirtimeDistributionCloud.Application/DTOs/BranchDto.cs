namespace AirtimeDistributionCloud.Application.DTOs;

public record BranchDto(int Id, string Name, string Location, bool IsActive, bool IsDefault);
public record CreateBranchRequest(string Name, string Location);
public record UpdateBranchRequest(int Id, string Name, string Location, bool IsActive);
