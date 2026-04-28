using HueResolve.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HueResolve.Data.Configurations;

public class AdministrativeAreaConfiguration : IEntityTypeConfiguration<AdministrativeArea>
{
    public void Configure(EntityTypeBuilder<AdministrativeArea> builder)
    {
        builder.ToTable("AdministrativeAreas");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.DistrictName)
               .HasMaxLength(150)
               .IsRequired();

        builder.Property(a => a.WardName)
               .HasMaxLength(150)
               .IsRequired();

        builder.HasIndex(a => new { a.DistrictName, a.WardName })
               .IsUnique()
               .HasDatabaseName("UQ_AdministrativeAreas_DistrictWard");
    }
}