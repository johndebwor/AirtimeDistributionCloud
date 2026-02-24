using AirtimeDistributionCloud.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirtimeDistributionCloud.Infrastructure.Data.Configurations;

public class AssetAssignmentConfiguration : IEntityTypeConfiguration<AssetAssignment>
{
    public void Configure(EntityTypeBuilder<AssetAssignment> builder)
    {
        builder.ToTable("AssetAssignments");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.AssignedToUserId).HasMaxLength(450);
        builder.Property(a => a.Notes).HasMaxLength(1000);
        builder.Property(a => a.CreatedBy).HasMaxLength(256);
        builder.Property(a => a.ModifiedBy).HasMaxLength(256);

        builder.HasOne(a => a.Asset)
            .WithMany(asset => asset.Assignments)
            .HasForeignKey(a => a.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Branch)
            .WithMany()
            .HasForeignKey(a => a.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
