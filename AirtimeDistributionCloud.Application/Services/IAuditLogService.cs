using AirtimeDistributionCloud.Application.DTOs;

namespace AirtimeDistributionCloud.Application.Services;

public interface IAuditLogService
{
    Task<IReadOnlyList<AuditLogDto>> GetFilteredLogsAsync(string? entityName, string? action, DateTime? from, DateTime? to, string? userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditLogDto>> GetRecentLogsAsync(int count, CancellationToken cancellationToken = default);
}
