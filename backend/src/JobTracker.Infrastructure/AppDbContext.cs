using JobTracker.Domain.Jobs;
using JobTracker.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.Infrastructure;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<JobApplication> JobApplications => Set<JobApplication>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new JobApplicationConfiguration());
    }
}
