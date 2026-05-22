using JobTracker.Application.Activities;
using JobTracker.Application.Activities.GetActivities;
using JobTracker.Application.Common;
using JobTracker.Domain.Jobs;
using JobTracker.Testing.Common.Builders;
using NSubstitute;

namespace JobTracker.Application.Tests.Activities.Queries;

public sealed class GetActivitiesQueryHandlerTests
{
    private readonly IActivityRepository _repo = Substitute.For<IActivityRepository>();
    private readonly GetActivitiesQueryHandler _handler;

    public GetActivitiesQueryHandlerTests()
    {
        _handler = new GetActivitiesQueryHandler(_repo);
    }

    [Fact]
    public async Task Handle_GivenPagedActivities_ThenReturnsItemsWithPaginationMetadata()
    {
        List<Activity> activities = Enumerable.Range(0, 3)
            .Select(_ => new ActivityBuilder().Build())
            .ToList();
        Page<Activity> pageResult = new Page<Activity>(activities, 42, 2, 3);
        _repo.GetPagedAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
             .Returns(pageResult);

        Page<ActivityResponse> result = await _handler.Handle(
            new GetActivitiesQuery(2, 3), CancellationToken.None);

        Assert.Equal(3, result.Items.Count);
        Assert.Equal(42, result.Total);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(3, result.PageSize);
    }

    [Fact]
    public async Task Handle_GivenNoActivities_ThenReturnsEmptyItemsWithZeroTotal()
    {
        Page<Activity> empty = new Page<Activity>(Array.Empty<Activity>(), 0, 1, 20);
        _repo.GetPagedAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
             .Returns(empty);

        Page<ActivityResponse> result = await _handler.Handle(
            new GetActivitiesQuery(1, 20), CancellationToken.None);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.Total);
    }

    [Fact]
    public async Task Handle_GivenQuery_ThenPassesPageAndPageSizeToRepository()
    {
        Page<Activity> empty = new Page<Activity>(Array.Empty<Activity>(), 0, 1, 20);
        _repo.GetPagedAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
             .Returns(empty);

        await _handler.Handle(new GetActivitiesQuery(3, 15), CancellationToken.None);

        await _repo.Received(1).GetPagedAsync(3, 15, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_GivenActivities_ThenMapsActivityDirectFieldsToResponse()
    {
        DateTimeOffset occurredAt = new DateTimeOffset(2026, 3, 1, 12, 0, 0, TimeSpan.Zero);
        Activity activity = new ActivityBuilder()
            .WithActivityType(ActivityType.Interview)
            .WithOccurredAt(occurredAt)
            .WithContactName("Jane Doe")
            .WithContactEmail("jane@example.com")
            .WithNotes("Great call")
            .Build();
        Page<Activity> pageResult = new Page<Activity>(new[] { activity }, 1, 1, 20);
        _repo.GetPagedAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
             .Returns(pageResult);

        Page<ActivityResponse> result = await _handler.Handle(
            new GetActivitiesQuery(1, 20), CancellationToken.None);

        ActivityResponse expected = new ActivityResponse(
            activity.Id.Value,
            activity.JobApplicationId.Value,
            null,
            null,
            ActivityType.Interview,
            occurredAt,
            "Jane Doe",
            "jane@example.com",
            "Great call",
            activity.CreatedAt);

        Assert.Equal(expected, result.Items[0]);
    }
}
