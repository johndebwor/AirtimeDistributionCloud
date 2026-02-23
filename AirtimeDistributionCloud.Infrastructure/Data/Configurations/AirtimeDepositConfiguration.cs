using AirtimeDistributionCloud.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirtimeDistributionCloud.Infrastructure.Data.Configurations;

public class AirtimeDepositConfiguration : IEntityTypeConfiguration<AirtimeDeposit>
{
    public void Configure(EntityTypeBuilder<AirtimeDeposit> builder)
    {
        builder.ToTable("AirtimeDeposits");
        builder.HasKey(ad => ad.Id);
        builder.Property(ad => ad.Amount).HasColumnType("decimal(18,2)");
        builder.Property(ad => ad.BonusPercentage).HasColumnType("decimal(18,2)");
        builder.Property(ad => ad.BonusAmount).HasColumnType("decimal(18,2)");
        builder.Property(ad => ad.TotalAmount).HasColumnType("decimal(18,2)");
        builder.Property(ad => ad.CreatedBy).HasMaxLength(256);
        builder.Property(ad => ad.ModifiedBy).HasMaxLength(256);
        builder.Property(ad => ad.CreatedByUserId).HasMaxLength(450);
        builder.Property(ad => ad.ApprovedByUserId).HasMaxLength(450);
        builder.Property(ad => ad.ApprovalNotes).HasMaxLength(1000);
        builder.Property(ad => ad.CancelledByUserId).HasMaxLength(450);
        builder.Property(ad => ad.CancellationReason).HasMaxLength(1000);
        builder.Property(ad => ad.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(ad => ad.Product)
            .WithMany(p => p.AirtimeDeposits)
            .HasForeignKey(ad => ad.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
