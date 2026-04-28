using HueResolve.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HueResolve.Data.Configurations;

public class AssignmentConfiguration : IEntityTypeConfiguration<Assignment>
{
    public void Configure(EntityTypeBuilder<Assignment> builder)
    {
        builder.ToTable("Assignments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
               .HasDefaultValueSql("newid()");

        builder.Property(a => a.Note)
               .HasMaxLength(500);

        builder.Property(a => a.AssignedAtUtc)
               .HasDefaultValueSql("getutcdate()")
               .IsRequired();

        // Relationships
        builder.HasOne(a => a.Report)
               .WithMany(r => r.Assignments)
               .HasForeignKey(a => a.ReportId)
               .OnDelete(DeleteBehavior.Cascade);     // Quan trọng

        // AssigneeId liên kết với User, nhưng không cần navigation property "Assignee"
        builder.HasOne<User>()                          // Dùng kiểu User trực tiếp
               .WithMany()
               .HasForeignKey(a => a.AssigneeId)
               .OnDelete(DeleteBehavior.Restrict);     // Không cho xóa User khi đang có Assignment
    }
}