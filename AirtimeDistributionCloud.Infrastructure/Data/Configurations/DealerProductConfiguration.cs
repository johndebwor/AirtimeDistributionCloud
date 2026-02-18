using AirtimeDistributionCloud.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirtimeDistributionCloud.Infrastructure.Data.Configurations;

public class DealerProductConfiguration : IEntityTypeConfiguration<DealerProduct>
{
    public void Configure(EntityTypeBuilder<DealerProduct> builder)
    {
        builder.ToTable("DealerProducts");
        builder.HasKey(dp => dp.Id);
        builder.Property(dp => dp.CommissionRate).HasColumnType("decimal(18,2)");
        builder.Property(dp => dp.Balance).HasColumnType("decimal(18,2)");
        builder.Property(dp => dp.CreatedBy).HasMaxLength(256);
        builder.Property(dp => dp.ModifiedBy).HasMaxLength(256);

        builder.HasOne(dp => dp.Dealer)
            .WithMany(d => d.DealerProducts)
            .HasForeignKey(dp => dp.DealerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(dp => dp.Product)
            .WithMany(p => p.DealerProducts)
            .HasForeignKey(dp => dp.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(dp => new { dp.DealerId, dp.ProductId }).IsUnique();
    }
}
