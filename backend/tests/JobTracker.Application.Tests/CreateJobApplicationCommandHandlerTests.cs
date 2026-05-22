using JobTracker.Application.Activities;
using JobTracker.Application.Common;
using JobTracker.Application.JobApplications;
using JobTracker.Application.JobApplications.CreateJobApplication;
using JobTracker.Domain.Jobs;
using NSubstitute;

namespace JobTracker.Application.Tests;

public sealed class CreateJobApplicationCommandHandlerTests
{
    private readonly IJobApplicationRepository _repo = Substitute.For<IJobApplicationRepository>();
    private readonly IActivityRepository _activityRepo = Substitute.For<IActivityRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly CreateJobApplicationCommandHandler _handler;

    public CreateJobApplicationCommandHandlerTests()
    {
        _handler = new CreateJobApplicationCommandHandler(_repo, _activityRepo, _uow);
    }

    [Fact]
    public async Task Handle_GivenValidCommand_ThenCallsAddAsyncAndSaveChanges()
    {
        var cmd = new CreateJobApplicationCommand("Dev", "Acme", null, null, null, Stage.Applied, null, null);

        await _handler.Handle(cmd, CancellationToken.None);

        await _repo.Received(1).AddAsync(Arg.Any<JobApplication>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_GivenValidCommand_ThenReturnsSuccessWithMappedResponse()
    {
        var appliedAt = DateTimeOffset.UtcNow;
        var cmd = new CreateJobApplicationCommand(
            "Software Engineer", "Acme", "Remote", null, null, Stage.Applied, appliedAt, null);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Software Engineer", result.Value.Title);
        Assert.Equal("Acme", result.Value.Company);
        Assert.Equal("Remote", result.Value.Location);
        Assert.Equal(Stage.Applied, result.Value.Stage);
        Assert.Equal(appliedAt, result.Value.AppliedAt);
    }

    [Fact]
    public async Task Handle_GivenAppliedAtProvided_ThenCreatesAppliedActivityWithMatchingOccurredAt()
    {
        var appliedAt = new DateTimeOffset(2026, 1, 15, 10, 0, 0, TimeSpan.Zero);
        var cmd = new CreateJobApplicationCommand("Dev", "Acme", null, null, null, Stage.Applied, appliedAt, null);

        await _handler.Handle(cmd, CancellationToken.None);

        await _activityRepo.Received(1).AddAsync(
            Arg.Is<Activity>(a => a.ActivityType == ActivityType.Applied && a.OccurredAt == appliedAt),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_GivenAppliedAtIsNull_ThenDoesNotCreateActivity()
    {
        var cmd = new CreateJobApplicationCommand("Dev", "Acme", null, null, null, Stage.Applied, null, null);

        await _handler.Handle(cmd, CancellationToken.None);

        await _activityRepo.DidNotReceive().AddAsync(Arg.Any<Activity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_GivenAppliedAtProvided_ThenSavesExactlyOnce()
    {
        var cmd = new CreateJobApplicationCommand("Dev", "Acme", null, null, null, Stage.Applied, DateTimeOffset.UtcNow, null);

        await _handler.Handle(cmd, CancellationToken.None);

        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
