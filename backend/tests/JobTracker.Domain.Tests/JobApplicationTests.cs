using JobTracker.Domain.Jobs;
using JobTracker.Testing.Common.Builders;

namespace JobTracker.Domain.Tests;

public sealed class JobApplicationTests
{
    [Fact]
    public void Create_GivenTitleAndCompany_ThenSetsFields()
    {
        var app = JobApplication.Create("Software Engineer", "Acme Corp");

        Assert.Equal("Software Engineer", app.Title);
        Assert.Equal("Acme Corp", app.Company);
    }

    [Fact]
    public void Create_GivenNoStage_ThenDefaultStageIsApplied()
    {
        var app = JobApplication.Create("Dev", "Corp");

        Assert.Equal(Stage.Applied, app.Stage);
    }

    [Fact]
    public void Create_GivenNewApplication_ThenIdIsNonEmptyGuid()
    {
        var app = JobApplication.Create("Dev", "Corp");

        Assert.NotEqual(Guid.Empty, app.Id.Value);
    }

    [Fact]
    public void Create_GivenNewApplication_ThenTimestampsAreUtcAndRecent()
    {
        var before = DateTimeOffset.UtcNow;
        var app = JobApplication.Create("Dev", "Corp");
        var after = DateTimeOffset.UtcNow;

        Assert.InRange(app.CreatedAt, before, after);
        Assert.InRange(app.UpdatedAt, before, after);
        Assert.Equal(app.CreatedAt, app.UpdatedAt);
    }

    [Fact]
    public void Create_GivenMinimalInput_ThenOptionalFieldsAreNull()
    {
        var app = JobApplicationBuilder.Minimal().Build();

        Assert.Null(app.Location);
        Assert.Null(app.Url);
        Assert.Null(app.Description);
        Assert.Null(app.AppliedAt);
        Assert.Null(app.PostedAt);
    }

    [Fact]
    public void Update_GivenNewValues_ThenMutatesAllFields()
    {
        var app = new JobApplicationBuilder().WithTitle("Old").WithCompany("OldCo").Build();
        var newAppliedAt = DateTimeOffset.UtcNow.AddDays(-1);

        app.Update("New Title", "NewCo", "Remote", "https://example.com", "A description",
            Stage.Interviewing, newAppliedAt, null);

        Assert.Equal("New Title", app.Title);
        Assert.Equal("NewCo", app.Company);
        Assert.Equal("Remote", app.Location);
        Assert.Equal("https://example.com", app.Url);
        Assert.Equal("A description", app.Description);
        Assert.Equal(Stage.Interviewing, app.Stage);
        Assert.Equal(newAppliedAt, app.AppliedAt);
        Assert.Null(app.PostedAt);
    }

    [Fact]
    public void Update_GivenCallToUpdate_ThenAdvancesUpdatedAt()
    {
        var app = new JobApplicationBuilder().Build();
        var originalUpdatedAt = app.UpdatedAt;

        Thread.Sleep(15);
        app.Update(app.Title, app.Company, app.Location, app.Url,
            app.Description, app.Stage, app.AppliedAt, app.PostedAt);

        Assert.True(app.UpdatedAt > originalUpdatedAt);
    }
}
