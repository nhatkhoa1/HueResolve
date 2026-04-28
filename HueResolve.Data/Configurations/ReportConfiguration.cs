using HueResolve.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HueResolve.Data.Configurations;

public class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.ToTable("Reports");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.TrackingCode)
               .HasMaxLength(32)
               .IsRequired();

        builder.Property(r => r.Title)
               .HasMaxLength(160)
               .IsRequired();

        builder.Property(r => r.Description)
               .HasMaxLength(4000)
               .IsRequired();

        builder.Property(r => r.ReporterName)
               .HasMaxLength(150)
               .IsRequired();

        builder.Property(r => r.ReporterPhone)
               .HasMaxLength(32)
               .IsRequired();

        builder.Property(r => r.AddressText)
               .HasMaxLength(255)
               .IsRequired();

        builder.Property(r => r.WardName)
               .HasMaxLength(150)
               .IsRequired();

        builder.Property(r => r.DistrictName)
               .HasMaxLength(150)
               .IsRequired();

        builder.Property(r => r.ClassificationState)
               .HasMaxLength(32)
               .IsRequired();

        builder.Property(r => r.Status)
               .HasMaxLength(32)
               .IsRequired();

        builder.Property(r => r.ResolutionNote)
               .HasMaxLength(1000);

        builder.Property(r => r.AdminFeedback)
               .HasColumnType("nvarchar(max)");

        builder.HasIndex(r => r.TrackingCode)
               .IsUnique()
               .HasDatabaseName("UQ_Reports_TrackingCode");

        // Relationships
        builder.HasOne(r => r.Category)
               .WithMany(c => c.Reports)
               .HasForeignKey(r => r.CategoryId);

        builder.HasOne(r => r.AdministrativeArea)
               .WithMany()
               .HasForeignKey(r => r.AdministrativeAreaId);

        builder.HasOne(r => r.Category)
               .WithMany(c => c.Reports)
               .HasForeignKey(r => r.CategoryId);

        builder.HasMany(r => r.StatusHistories)
               .WithOne(h => h.Report)
               .HasForeignKey(h => h.ReportId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.Attachments)
               .WithOne(a => a.Report)
               .HasForeignKey(a => a.ReportId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.Assignments)
               .WithOne(a => a.Report)
               .HasForeignKey(a => a.ReportId);
    }
}