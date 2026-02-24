using AirtimeDistributionCloud.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirtimeDistributionCloud.Infrastructure.Data.Configurations;

public class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.ToTable("Assets");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name).IsRequired().HasMaxLength(200);
        builder.Property(a => a.SerialNumber).HasMaxLength(100);
        builder.Property(a => a.AssetTag).HasMaxLength(50);
        builder.Property(a => a.Currency).IsRequired().HasMaxLength(10).HasDefaultValue("SSP");
        builder.Property(a => a.PurchaseValue).HasColumnType("decimal(18,2)");
        builder.Property(a => a.CurrentValue).HasColumnType("decimal(18,2)");
        builder.Property(a => a.DepreciationMethod).HasMaxLength(50);
        builder.Property(a => a.Condition).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.AssignedToUserId).HasMaxLength(450);
        builder.Property(a => a.DisposalReason).HasMaxLength(500);
        builder.Property(a => a.DisposalApprovedByUserId).HasMaxLength(450);
        builder.Property(a => a.DisposalNotes).HasMaxLength(1000);
        builder.Property(a => a.Notes).HasMaxLength(1000);
        builder.Property(a => a.CreatedBy).HasMaxLength(256);
        builder.Property(a => a.ModifiedBy).HasMaxLength(256);

        builder.HasOne(a => a.AssetCategory)
            .WithMany(c => c.Assets)
            .HasForeignKey(a => a.AssetCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Branch)
            .WithMany()
            .HasForeignKey(a => a.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.SerialNumber);
        builder.HasIndex(a => a.AssetTag);
    }
}
