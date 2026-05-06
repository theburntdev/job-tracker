using JobTracker.Domain.Common;
using JobTracker.Domain.Jobs;
using JobTracker.Infrastructure;
using JobTracker.Testing.Common.Builders;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace JobTracker.Infrastructure.Tests;

public sealed class JobApplicationTimestampAndOrderingTests
{
    [Fact]
    public async Task Sqlite_OrderBy_UpdatedAt_does_not_throw_with_unix_millisecond_columns()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var ctx = new AppDbContext(options);
        IMigrator migrator = ctx.Database.GetService<IMigrator>()!;
        migrator.Migrate();

        var a = JobApplicationBuilder.Minimal().WithTitle("a").Build();
        var b = JobApplicationBuilder.Minimal().WithTitle("b").Build();
        ctx.JobApplications.AddRange(a, b);
        await ctx.SaveChangesAsync();

        await ctx.Database.ExecuteSqlRawAsync(
            """UPDATE "JobApplications" SET "UpdatedAt" = {0} WHERE "Id" = {1}""",
            100L,
            a.Id.Value);

        await ctx.Database.ExecuteSqlRawAsync(
            """UPDATE "JobApplications" SET "UpdatedAt" = {0} WHERE "Id" = {1}""",
            200L,
            b.Id.Value);

        List<JobApplication> ordered = await ctx.JobApplications
            .OrderByDescending(j => j.UpdatedAt)
            .ThenBy(j => j.Id)
            .ToListAsync();

        Assert.Equal("b", ordered[0].Title);
        Assert.Equal("a", ordered[1].Title);
    }

    [Fact]
    public async Task Timestamp_migration_preserves_legacy_text_DateTimeOffset_values()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var ctx = new AppDbContext(options);
        IMigrator migrator = ctx.Database.GetService<IMigrator>()!;
        migrator.Migrate("20260506174006_RenameJobsToJobApplications");

        var id = Guid.NewGuid();
        await ctx.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO "JobApplications" ("Id", "Title", "Company", "Location", "Url", "Description", "Stage", "AppliedAt", "PostedAt", "CreatedAt", "UpdatedAt")
            VALUES ({0}, 'T', 'C', NULL, NULL, NULL, 0, '2023-01-02T10:00:00.0000000+00:00', NULL, '2023-01-02T10:00:00.0000000+00:00', '2023-06-02T10:00:00.0000000+00:00')
            """,
            id);

        migrator.Migrate();

        JobApplication ja = await ctx.JobApplications.SingleAsync(j => j.Id == new JobApplicationId(id));
        DateTimeOffset expectedApplied = DateTimeOffset.Parse("2023-01-02T10:00:00+00:00");
        DateTimeOffset expectedCreated = DateTimeOffset.Parse("2023-01-02T10:00:00+00:00");
        DateTimeOffset expectedUpdated = DateTimeOffset.Parse("2023-06-02T10:00:00+00:00");

        Assert.Equal(expectedApplied.ToUnixTimeMilliseconds(), ja.AppliedAt!.Value.ToUnixTimeMilliseconds());
        Assert.Equal(expectedCreated.ToUnixTimeMilliseconds(), ja.CreatedAt.ToUnixTimeMilliseconds());
        Assert.Equal(expectedUpdated.ToUnixTimeMilliseconds(), ja.UpdatedAt.ToUnixTimeMilliseconds());
    }
}
