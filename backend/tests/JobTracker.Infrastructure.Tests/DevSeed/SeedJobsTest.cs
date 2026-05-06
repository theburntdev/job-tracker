using JobTracker.Infrastructure;
using JobTracker.Testing.Common.Builders;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.Infrastructure.Tests.DevSeed;

public sealed class SeedJobsTest
{
    // Manual seed only: when RUN_DEV_SEED is unset, this test returns immediately (harmless in full runs).
    // PowerShell: $env:RUN_DEV_SEED = "true"; dotnet test backend/tests/JobTracker.Infrastructure.Tests --filter SeedJobsTest
    // cmd.exe:        set RUN_DEV_SEED=true&& dotnet test backend/tests/JobTracker.Infrastructure.Tests --filter SeedJobsTest
    [Fact]
    public async Task Seed_Dev_Database_With_Sample_JobApplications()
    {
        if (Environment.GetEnvironmentVariable("RUN_DEV_SEED") != "true")
            return;

        var dbPath = Environment.GetEnvironmentVariable("DEV_DB_PATH")
            ?? Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory,
                "../../../../../src/JobTracker.Api/jobtracker.db"));

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={dbPath}")
            .Options;

        await using var context = new AppDbContext(options);
        await context.Database.MigrateAsync();

        const int total = 50;
        var third = total / 3;

        var jobApplications =
            Enumerable.Range(0, third).Select(_ => JobApplicationBuilder.Remote().Build())
            .Concat(Enumerable.Range(0, third).Select(_ => JobApplicationBuilder.NoLink().Build()))
            .Concat(Enumerable.Range(0, total - third * 2).Select(_ => JobApplicationBuilder.Minimal().Build()))
            .ToList();

        await context.JobApplications.AddRangeAsync(jobApplications);
        await context.SaveChangesAsync();

        Assert.Equal(total, jobApplications.Count);
    }
}
