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

public sealed class JobApplicationRepositoryTests
{
    private static async Task<(SqliteConnection connection, AppDbContext context, JobApplicationRepository repository)> CreateMigratedDbAsync()
    {
        SqliteConnection connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        AppDbContext context = new AppDbContext(options);
        IMigrator migrator = context.Database.GetService<IMigrator>()!;
        migrator.Migrate();

        JobApplicationRepository repository = new JobApplicationRepository(context);
        return (connection, context, repository);
    }

    [Fact]
    public async Task AddAsync_GivenNewApplication_ThenCanFetchById()
    {
        var (connection, context, repository) = await CreateMigratedDbAsync();
        await using (connection)
        await using (context)
        {
            JobApplication app = new JobApplicationBuilder()
                .WithTitle("Backend Engineer")
                .WithCompany("Acme")
                .Build();

            await repository.AddAsync(app);
            await context.SaveChangesAsync();

            JobApplication? fetched = await repository.GetByIdAsync(app.Id);

            Assert.NotNull(fetched);
            Assert.Equal(app.Id, fetched.Id);
            Assert.Equal("Backend Engineer", fetched.Title);
            Assert.Equal("Acme", fetched.Company);
        }
    }

    [Fact]
    public async Task GetByIdAsync_GivenMissingId_ThenReturnsNull()
    {
        var (connection, context, repository) = await CreateMigratedDbAsync();
        await using (connection)
        await using (context)
        {
            JobApplicationId missingId = new JobApplicationId(Guid.NewGuid());

            JobApplication? result = await repository.GetByIdAsync(missingId);

            Assert.Null(result);
        }
    }

    [Fact]
    public async Task Update_GivenExistingApplication_ThenPersistsChanges()
    {
        var (connection, context, repository) = await CreateMigratedDbAsync();
        await using (connection)
        await using (context)
        {
            JobApplication app = new JobApplicationBuilder()
                .WithTitle("Original Title")
                .WithCompany("Corp")
                .WithStage(Stage.Applied)
                .Build();

            await repository.AddAsync(app);
            await context.SaveChangesAsync();

            app.Update("Updated Title", "Corp", null, null, null, Stage.Screening, null, null);
            repository.Update(app);
            await context.SaveChangesAsync();

            JobApplication? fetched = await repository.GetByIdAsync(app.Id);

            Assert.NotNull(fetched);
            Assert.Equal("Updated Title", fetched.Title);
            Assert.Equal(Stage.Screening, fetched.Stage);
        }
    }
}
