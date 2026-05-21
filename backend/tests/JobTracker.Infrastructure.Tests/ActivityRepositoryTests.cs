using JobTracker.Domain.Common;
using JobTracker.Domain.Jobs;
using JobTracker.Infrastructure;
using JobTracker.Infrastructure.Repositories;
using JobTracker.Testing.Common.Builders;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace JobTracker.Infrastructure.Tests;

public sealed class ActivityRepositoryTests
{
    private static async Task<(SqliteConnection connection, AppDbContext context, ActivityRepository activityRepo, JobApplicationRepository appRepo)> CreateMigratedDbAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new AppDbContext(options);
        context.Database.GetService<IMigrator>().Migrate();

        return (connection, context, new ActivityRepository(context), new JobApplicationRepository(context));
    }

    [Fact]
    public async Task AddAsync_GivenNewActivity_ThenCanFetchById()
    {
        var (connection, context, activityRepo, appRepo) = await CreateMigratedDbAsync();
        await using (connection)
        await using (context)
        {
            var app = new JobApplicationBuilder().WithCompany("Acme").Build();
            await appRepo.AddAsync(app);
            await context.SaveChangesAsync();

            var activity = Activity.Create(app.Id, ActivityType.PhoneScreen, DateTimeOffset.UtcNow,
                contactName: "Jane", contactEmail: "jane@example.com", notes: "Went well");
            await activityRepo.AddAsync(activity);
            await context.SaveChangesAsync();

            var fetched = await activityRepo.GetByIdAsync(activity.Id);

            Assert.NotNull(fetched);
            Assert.Equal(activity.Id, fetched.Id);
            Assert.Equal(ActivityType.PhoneScreen, fetched.ActivityType);
            Assert.Equal("Jane", fetched.ContactName);
            Assert.Equal("jane@example.com", fetched.ContactEmail);
            Assert.Equal("Went well", fetched.Notes);
        }
    }

    [Fact]
    public async Task GetPagedAsync_GivenMultipleActivities_ThenReturnsSortedByOccurredAtDesc()
    {
        var (connection, context, activityRepo, appRepo) = await CreateMigratedDbAsync();
        await using (connection)
        await using (context)
        {
            var app = new JobApplicationBuilder().Build();
            await appRepo.AddAsync(app);
            await context.SaveChangesAsync();

            var older = Activity.Create(app.Id, ActivityType.Applied, DateTimeOffset.UtcNow.AddDays(-5));
            var newer = Activity.Create(app.Id, ActivityType.PhoneScreen, DateTimeOffset.UtcNow.AddDays(-1));
            var middle = Activity.Create(app.Id, ActivityType.Interview, DateTimeOffset.UtcNow.AddDays(-3));

            await activityRepo.AddAsync(older);
            await activityRepo.AddAsync(newer);
            await activityRepo.AddAsync(middle);
            await context.SaveChangesAsync();

            var page = await activityRepo.GetPagedAsync(1, 10);

            Assert.Equal(3, page.Total);
            Assert.Equal(ActivityType.PhoneScreen, page.Items[0].ActivityType);
            Assert.Equal(ActivityType.Interview, page.Items[1].ActivityType);
            Assert.Equal(ActivityType.Applied, page.Items[2].ActivityType);
        }
    }

    [Fact]
    public async Task GetPagedAsync_GivenActivity_ThenLoadsParentJobApplicationData()
    {
        var (connection, context, activityRepo, appRepo) = await CreateMigratedDbAsync();
        await using (connection)
        await using (context)
        {
            var app = new JobApplicationBuilder().WithCompany("Globex").WithTitle("Engineer").Build();
            await appRepo.AddAsync(app);
            await context.SaveChangesAsync();

            var activity = Activity.Create(app.Id, ActivityType.Interview, DateTimeOffset.UtcNow);
            await activityRepo.AddAsync(activity);
            await context.SaveChangesAsync();

            var page = await activityRepo.GetPagedAsync(1, 10);

            Assert.Single(page.Items);
            Assert.NotNull(page.Items[0].JobApplication);
            Assert.Equal("Globex", page.Items[0].JobApplication!.Company);
            Assert.Equal("Engineer", page.Items[0].JobApplication!.Title);
        }
    }

    [Fact]
    public async Task Delete_GivenJobApplicationDeleted_ThenCascadeDeletesActivities()
    {
        var (connection, context, activityRepo, appRepo) = await CreateMigratedDbAsync();
        await using (connection)
        await using (context)
        {
            var app = new JobApplicationBuilder().Build();
            await appRepo.AddAsync(app);
            await context.SaveChangesAsync();

            var activity = Activity.Create(app.Id, ActivityType.Applied, DateTimeOffset.UtcNow);
            await activityRepo.AddAsync(activity);
            await context.SaveChangesAsync();

            appRepo.Delete(app);
            await context.SaveChangesAsync();

            var fetched = await activityRepo.GetByIdAsync(activity.Id);
            Assert.Null(fetched);
        }
    }
}
