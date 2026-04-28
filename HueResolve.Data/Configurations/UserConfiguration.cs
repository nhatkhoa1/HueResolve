using HueResolve.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HueResolve.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.FullName)
               .HasMaxLength(150)
               .IsRequired();

        builder.Property(u => u.Username)
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(u => u.PasswordHash)
               .HasMaxLength(500)
               .IsRequired();

        builder.Property(u => u.PhoneNumber)
               .HasMaxLength(20);

        builder.Property(u => u.AddressText)
               .HasMaxLength(255);

        builder.HasIndex(u => u.Username)
               .IsUnique()
               .HasDatabaseName("UQ_Users_Username");

        builder.HasOne(u => u.Role)
               .WithMany(r => r.Users)
               .HasForeignKey(u => u.RoleId);

        builder.HasOne(u => u.Department)
               .WithMany(d => d.Users)
               .HasForeignKey(u => u.DepartmentId);
    }
}