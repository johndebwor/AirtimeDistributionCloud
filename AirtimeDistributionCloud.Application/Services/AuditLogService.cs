using AirtimeDistributionCloud.Application.DTOs;
using AirtimeDistributionCloud.Core.Interfaces;

namespace AirtimeDistributionCloud.Application.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _auditLogRepository;

    public AuditLogService(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task<IReadOnlyList<AuditLogDto>> GetFilteredLogsAsync(string? entityName, string? action, DateTime? from, DateTime? to, string? userId, CancellationToken cancellationToken = default)
    {
        var logs = await _auditLogRepository.GetFilteredAsync(entityName, action, from, to, userId, cancellationToken);
        return logs.Select(l => new AuditLogDto(l.Id, l.EntityName, l.EntityId, l.Action, l.OldValues, l.NewValues, l.UserId, l.UserName, l.Timestamp)).ToList();
    }

    public async Task<IReadOnlyList<AuditLogDto>> GetRecentLogsAsync(int count, CancellationToken cancellationToken = default)
    {
        var logs = await _auditLogRepository.GetRecentAsync(count, cancellationToken);
        return logs.Select(l => new AuditLogDto(l.Id, l.EntityName, l.EntityId, l.Action, l.OldValues, l.NewValues, l.UserId, l.UserName, l.Timestamp)).ToList();
    }
}
