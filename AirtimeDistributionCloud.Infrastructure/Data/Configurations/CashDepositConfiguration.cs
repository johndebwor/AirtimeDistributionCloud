using AirtimeDistributionCloud.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirtimeDistributionCloud.Infrastructure.Data.Configurations;

public class CashDepositConfiguration : IEntityTypeConfiguration<CashDeposit>
{
    public void Configure(EntityTypeBuilder<CashDeposit> builder)
    {
        builder.ToTable("CashDeposits");
        builder.HasKey(cd => cd.Id);
        builder.Property(cd => cd.Amount).HasColumnType("decimal(18,2)");
        builder.Property(cd => cd.CreatedBy).HasMaxLength(256);
        builder.Property(cd => cd.ModifiedBy).HasMaxLength(256);

        builder.HasOne(cd => cd.Branch)
            .WithMany(b => b.CashDeposits)
            .HasForeignKey(cd => cd.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cd => cd.Dealer)
            .WithMany(d => d.CashDeposits)
            .HasForeignKey(cd => cd.DealerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
