using AirtimeDistributionCloud.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirtimeDistributionCloud.Infrastructure.Data.Configurations;

public class AssetMaintenanceConfiguration : IEntityTypeConfiguration<AssetMaintenance>
{
    public void Configure(EntityTypeBuilder<AssetMaintenance> builder)
    {
        builder.ToTable("AssetMaintenanceRecords");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Description).IsRequired().HasMaxLength(500);
        builder.Property(m => m.Cost).HasColumnType("decimal(18,2)");
        builder.Property(m => m.PerformedBy).HasMaxLength(200);
        builder.Property(m => m.Notes).HasMaxLength(1000);
        builder.Property(m => m.CreatedBy).HasMaxLength(256);
        builder.Property(m => m.ModifiedBy).HasMaxLength(256);

        builder.HasOne(m => m.Asset)
            .WithMany(a => a.MaintenanceRecords)
            .HasForeignKey(m => m.AssetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
