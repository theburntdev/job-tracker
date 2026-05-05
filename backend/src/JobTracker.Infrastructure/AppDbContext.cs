using Microsoft.EntityFrameworkCore;

namespace JobTracker.Infrastructure;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
}
