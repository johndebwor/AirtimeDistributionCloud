namespace AirtimeDistributionCloud.Application.DTOs;

public record AuditLogDto(
    int Id, string EntityName, string EntityId, string Action,
    string? OldValues, string? NewValues,
    string? UserId, string? UserName, DateTime Timestamp);
