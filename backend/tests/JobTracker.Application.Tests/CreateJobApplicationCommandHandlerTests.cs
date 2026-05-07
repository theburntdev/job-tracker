using JobTracker.Application.Common;
using JobTracker.Application.JobApplications;
using JobTracker.Application.JobApplications.CreateJobApplication;
using JobTracker.Domain.Jobs;
using NSubstitute;

namespace JobTracker.Application.Tests;

public sealed class CreateJobApplicationCommandHandlerTests
{
    private readonly IJobApplicationRepository _repo = Substitute.For<IJobApplicationRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly CreateJobApplicationCommandHandler _handler;

    public CreateJobApplicationCommandHandlerTests()
    {
        _handler = new CreateJobApplicationCommandHandler(_repo, _uow);
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
}
