using HueResolve.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HueResolve.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Code)
               .HasMaxLength(32)
               .IsRequired();

        builder.Property(c => c.Name)
               .HasMaxLength(120)
               .IsRequired();

        builder.HasIndex(c => c.Code)
               .IsUnique()
               .HasDatabaseName("UQ_Categories_Code");

        // Relationship
        builder.HasMany(c => c.Reports)
               .WithOne(r => r.Category)
               .HasForeignKey(r => r.CategoryId);
    }
}