using HueResolve.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HueResolve.Data.Configurations;

public class ReportAttachmentConfiguration : IEntityTypeConfiguration<ReportAttachment>
{
    public void Configure(EntityTypeBuilder<ReportAttachment> builder)
    {
        builder.ToTable("ReportAttachments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
               .HasDefaultValueSql("newid()");

        builder.Property(a => a.OriginalFileName)
               .HasMaxLength(260)
               .IsRequired();

        builder.Property(a => a.StoredFileName)
               .HasMaxLength(260)
               .IsRequired();

        builder.Property(a => a.RelativePath)
               .HasMaxLength(500)
               .IsRequired();

        builder.Property(a => a.ContentType)
               .HasMaxLength(120)
               .IsRequired();

        builder.Property(a => a.CreatedAtUtc)
               .HasDefaultValueSql("getutcdate()")
               .IsRequired();

        // Relationship
        builder.HasOne(a => a.Report)
               .WithMany(r => r.Attachments)
               .HasForeignKey(a => a.ReportId)
               .OnDelete(DeleteBehavior.Cascade);   // Xóa Report thì xóa luôn attachment
    }
}