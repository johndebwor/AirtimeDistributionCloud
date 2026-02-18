using AirtimeDistributionCloud.Core.Entities;

namespace AirtimeDistributionCloud.Core.Interfaces;

public interface IAuditLogRepository
{
    Task<IReadOnlyList<AuditLog>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditLog>> GetFilteredAsync(string? entityName, string? action, DateTime? from, DateTime? to, string? userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditLog>> GetRecentAsync(int count, CancellationToken cancellationToken = default);
    Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
}
