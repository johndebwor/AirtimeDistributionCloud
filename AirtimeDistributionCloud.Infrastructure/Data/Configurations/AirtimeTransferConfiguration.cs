using AirtimeDistributionCloud.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirtimeDistributionCloud.Infrastructure.Data.Configurations;

public class AirtimeTransferConfiguration : IEntityTypeConfiguration<AirtimeTransfer>
{
    public void Configure(EntityTypeBuilder<AirtimeTransfer> builder)
    {
        builder.ToTable("AirtimeTransfers");
        builder.HasKey(at => at.Id);
        builder.Property(at => at.Amount).HasColumnType("decimal(18,2)");
        builder.Property(at => at.CreatedBy).HasMaxLength(256);
        builder.Property(at => at.ModifiedBy).HasMaxLength(256);
        builder.Property(at => at.TransferType).HasConversion<string>().HasMaxLength(20);
        builder.Property(at => at.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(at => at.ApprovedByUserId).HasMaxLength(450);
        builder.Property(at => at.ApprovalNotes).HasMaxLength(1000);

        builder.HasOne(at => at.Branch)
            .WithMany(b => b.AirtimeTransfers)
            .HasForeignKey(at => at.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(at => at.Product)
            .WithMany(p => p.AirtimeTransfers)
            .HasForeignKey(at => at.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(at => at.Dealer)
            .WithMany(d => d.AirtimeTransfers)
            .HasForeignKey(at => at.DealerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
