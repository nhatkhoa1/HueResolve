using HueResolve.Models.Entities;
using Microsoft.EntityFrameworkCore;
using HueResolve.Data.Configurations;

namespace HueResolve.Data;

public class HueResolveDbContext : DbContext
{
    public HueResolveDbContext(DbContextOptions<HueResolveDbContext> options)
        : base(options)
    {
    }

    // DbSets
    public DbSet<Report> Reports { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<AdministrativeArea> AdministrativeAreas { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Assignment> Assignments { get; set; }
    public DbSet<ReportAttachment> ReportAttachments { get; set; }
    public DbSet<ReportStatusHistory> ReportStatusHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Áp dụng tất cả configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HueResolveDbContext).Assembly);

        // Một số default value bổ sung (nếu cần)
        modelBuilder.Entity<Report>()
            .Property(r => r.Status)
            .HasDefaultValue("TiepNhan");

        modelBuilder.Entity<Report>()
            .Property(r => r.ClassificationState)
            .HasDefaultValue("Pending");

        modelBuilder.Entity<Report>()
            .Property(r => r.NeedsReview)
            .HasDefaultValue(false);
    }
}