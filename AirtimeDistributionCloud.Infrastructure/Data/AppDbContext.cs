using System.Text.Json;
using AirtimeDistributionCloud.Core.Entities;
using AirtimeDistributionCloud.Core.Interfaces;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AirtimeDistributionCloud.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    private readonly ICurrentUserService? _currentUserService;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService currentUserService)
        : base(options)
    {
        _currentUserService = currentUserService;
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Dealer> Dealers => Set<Dealer>();
    public DbSet<DealerProduct> DealerProducts => Set<DealerProduct>();
    public DbSet<AirtimeDeposit> AirtimeDeposits => Set<AirtimeDeposit>();
    public DbSet<AirtimeTransfer> AirtimeTransfers => Set<AirtimeTransfer>();
    public DbSet<CashDeposit> CashDeposits => Set<CashDeposit>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var auditEntries = CaptureAuditEntries();

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedDate = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.ModifiedDate = DateTime.UtcNow;
                    break;
            }
        }

        var result = await base.SaveChangesAsync(cancellationToken);

        if (auditEntries.Count > 0)
        {
            foreach (var audit in auditEntries)
            {
                if (audit.EntityId == "0" && audit.TempEntry != null)
                {
                    var idProp = audit.TempEntry.Properties.FirstOrDefault(p => p.Metadata.Name == "Id");
                    if (idProp != null)
                        audit.EntityId = idProp.CurrentValue?.ToString() ?? "0";
                }

                AuditLogs.Add(new AuditLog
                {
                    EntityName = audit.EntityName,
                    EntityId = audit.EntityId,
                    Action = audit.Action,
                    OldValues = audit.OldValues,
                    NewValues = audit.NewValues,
                    UserId = audit.UserId,
                    UserName = audit.UserName,
                    Timestamp = DateTime.UtcNow
                });
            }

            await base.SaveChangesAsync(cancellationToken);
        }

        return result;
    }

    private List<AuditEntryData> CaptureAuditEntries()
    {
        ChangeTracker.DetectChanges();
        var entries = new List<AuditEntryData>();

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is AuditLog || entry.Entity is SystemSetting)
                continue;

            if (entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            var entityName = entry.Entity.GetType().Name;
            string? userId = null;
            string? userName = null;
            try
            {
                userId = _currentUserService?.UserId;
                userName = _currentUserService?.UserName;
            }
            catch
            {
                // Outside Blazor component scope (e.g. seeding, background tasks)
            }

            var audit = new AuditEntryData
            {
                EntityName = entityName,
                UserId = userId,
                UserName = userName
            };

            switch (entry.State)
            {
                case EntityState.Added:
                    audit.Action = "Create";
                    audit.EntityId = "0";
                    audit.TempEntry = entry;
                    var newValues = new Dictionary<string, object?>();
                    foreach (var prop in entry.Properties)
                    {
                        if (prop.Metadata.Name != "Id")
                            newValues[prop.Metadata.Name] = prop.CurrentValue;
                    }
                    audit.NewValues = JsonSerializer.Serialize(newValues);
                    break;

                case EntityState.Modified:
                    audit.Action = "Update";
                    var idProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "Id");
                    audit.EntityId = idProp?.CurrentValue?.ToString() ?? "0";
                    var oldVals = new Dictionary<string, object?>();
                    var changedVals = new Dictionary<string, object?>();
                    foreach (var prop in entry.Properties.Where(p => p.IsModified))
                    {
                        oldVals[prop.Metadata.Name] = prop.OriginalValue;
                        changedVals[prop.Metadata.Name] = prop.CurrentValue;
                    }
                    if (oldVals.Count > 0)
                    {
                        audit.OldValues = JsonSerializer.Serialize(oldVals);
                        audit.NewValues = JsonSerializer.Serialize(changedVals);
                    }
                    break;

                case EntityState.Deleted:
                    audit.Action = "Delete";
                    var deletedId = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "Id");
                    audit.EntityId = deletedId?.CurrentValue?.ToString() ?? "0";
                    var deletedVals = new Dictionary<string, object?>();
                    foreach (var prop in entry.Properties)
                    {
                        deletedVals[prop.Metadata.Name] = prop.OriginalValue;
                    }
                    audit.OldValues = JsonSerializer.Serialize(deletedVals);
                    break;
            }

            entries.Add(audit);
        }

        return entries;
    }

    private class AuditEntryData
    {
        public string EntityName { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry? TempEntry { get; set; }
    }
}
