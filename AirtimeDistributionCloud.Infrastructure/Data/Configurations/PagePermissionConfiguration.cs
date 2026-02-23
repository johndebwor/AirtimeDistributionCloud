using AirtimeDistributionCloud.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirtimeDistributionCloud.Infrastructure.Data.Configurations;

public class PagePermissionConfiguration : IEntityTypeConfiguration<PagePermission>
{
    public void Configure(EntityTypeBuilder<PagePermission> builder)
    {
        builder.ToTable("PagePermissions");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.RoleName).IsRequired().HasMaxLength(50);
        builder.Property(p => p.PageKey).IsRequired().HasMaxLength(100);
        builder.HasIndex(p => new { p.RoleName, p.PageKey }).IsUnique();
    }
}
