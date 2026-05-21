using JobTracker.Domain.Common;
using JobTracker.Domain.Jobs;

namespace JobTracker.Domain.Tests;

public sealed class ActivityTests
{
    [Fact]
    public void Create_GivenRequiredFields_ThenSetsAllFields()
    {
        var jobApplicationId = new JobApplicationId(Guid.NewGuid());
        var occurredAt = DateTimeOffset.UtcNow.AddDays(-1);

        var activity = Activity.Create(
            jobApplicationId,
            ActivityType.PhoneScreen,
            occurredAt,
            contactName: "Jane Smith",
            contactEmail: "jane@example.com",
            notes: "Went well");

        Assert.Equal(jobApplicationId, activity.JobApplicationId);
        Assert.Equal(ActivityType.PhoneScreen, activity.ActivityType);
        Assert.Equal(occurredAt, activity.OccurredAt);
        Assert.Equal("Jane Smith", activity.ContactName);
        Assert.Equal("jane@example.com", activity.ContactEmail);
        Assert.Equal("Went well", activity.Notes);
    }

    [Fact]
    public void Create_GivenNewActivity_ThenIdIsNonEmptyGuid()
    {
        var activity = Activity.Create(
            new JobApplicationId(Guid.NewGuid()),
            ActivityType.Applied,
            DateTimeOffset.UtcNow);

        Assert.NotEqual(Guid.Empty, activity.Id.Value);
    }

    [Fact]
    public void Create_GivenNewActivity_ThenCreatedAtIsUtcAndRecent()
    {
        var before = DateTimeOffset.UtcNow;
        var activity = Activity.Create(
            new JobApplicationId(Guid.NewGuid()),
            ActivityType.Applied,
            DateTimeOffset.UtcNow);
        var after = DateTimeOffset.UtcNow;

        Assert.InRange(activity.CreatedAt, before, after);
    }

    [Fact]
    public void Create_GivenNoOptionalFields_ThenOptionalFieldsAreNull()
    {
        var activity = Activity.Create(
            new JobApplicationId(Guid.NewGuid()),
            ActivityType.Interview,
            DateTimeOffset.UtcNow);

        Assert.Null(activity.ContactName);
        Assert.Null(activity.ContactEmail);
        Assert.Null(activity.Notes);
    }
}
