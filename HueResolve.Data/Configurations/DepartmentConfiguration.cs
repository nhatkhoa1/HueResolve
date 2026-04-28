using HueResolve.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HueResolve.Data.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Name)
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(d => d.Description)
               .HasMaxLength(500);

        builder.Property(d => d.Email)
               .HasMaxLength(100);

        builder.Property(d => d.IsActive)
               .HasDefaultValue(true);

        builder.HasMany(d => d.Users)
               .WithOne(u => u.Department)
               .HasForeignKey(u => u.DepartmentId);
    }
}