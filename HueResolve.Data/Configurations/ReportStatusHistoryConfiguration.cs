using HueResolve.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HueResolve.Data.Configurations;

public class ReportStatusHistoryConfiguration : IEntityTypeConfiguration<ReportStatusHistory>
{
    public void Configure(EntityTypeBuilder<ReportStatusHistory> builder)
    {
        builder.ToTable("ReportStatusHistories");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Id)
               .HasDefaultValueSql("newid()");

        builder.Property(h => h.Status)
               .HasMaxLength(32)
               .IsRequired();

        builder.Property(h => h.Note)
               .HasMaxLength(1000);

        builder.Property(h => h.UpdatedByName)
               .HasMaxLength(150);

        builder.Property(h => h.CreatedAtUtc)
               .HasDefaultValueSql("getutcdate()")
               .IsRequired();

        // Relationship
        builder.HasOne(h => h.Report)
               .WithMany(r => r.StatusHistories)
               .HasForeignKey(h => h.ReportId)
               .OnDelete(DeleteBehavior.Cascade);   // Xóa Report thì xóa lịch sử trạng thái
    }
}