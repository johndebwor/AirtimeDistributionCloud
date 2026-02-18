using AirtimeDistributionCloud.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirtimeDistributionCloud.Infrastructure.Data.Configurations;

public class DealerConfiguration : IEntityTypeConfiguration<Dealer>
{
    public void Configure(EntityTypeBuilder<Dealer> builder)
    {
        builder.ToTable("Dealers");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Name).IsRequired().HasMaxLength(300);
        builder.Property(d => d.Gender).HasMaxLength(20);
        builder.Property(d => d.IDNumber).HasMaxLength(50);
        builder.Property(d => d.CompanyRegNumber).HasMaxLength(100);
        builder.Property(d => d.Nationality).HasMaxLength(100);
        builder.Property(d => d.State).HasMaxLength(100);
        builder.Property(d => d.County).HasMaxLength(100);
        builder.Property(d => d.PhysicalAddress).HasMaxLength(500);
        builder.Property(d => d.CreatedBy).HasMaxLength(256);
        builder.Property(d => d.ModifiedBy).HasMaxLength(256);
        builder.Property(d => d.Type).HasConversion<string>().HasMaxLength(20);
    }
}
