using AirtimeDistributionCloud.Core.Entities;
using AirtimeDistributionCloud.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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
        builder.Property(d => d.Type).HasConversion(
            v => v.ToString(),
            v => v == "Individual" ? DealerType.Person : Enum.Parse<DealerType>(v)
        ).HasMaxLength(20);
        builder.Property(d => d.DealerNumber).IsRequired().HasMaxLength(20);
        builder.HasIndex(d => d.DealerNumber).IsUnique();
        builder.Property(d => d.PhoneNumber).HasMaxLength(20);
    }
}
