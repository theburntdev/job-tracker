using JobTracker.Application.Common;
using JobTracker.Application.JobApplications;
using JobTracker.Application.JobApplications.UpdateJobApplication;
using JobTracker.Domain.Common;
using JobTracker.Domain.Jobs;
using JobTracker.Testing.Common.Builders;
using NSubstitute;

namespace JobTracker.Application.Tests;

public sealed class UpdateJobApplicationCommandHandlerTests
{
    private readonly IJobApplicationRepository _repo = Substitute.For<IJobApplicationRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly UpdateJobApplicationCommandHandler _handler;

    public UpdateJobApplicationCommandHandlerTests()
    {
        _handler = new UpdateJobApplicationCommandHandler(_repo, _uow);
    }

    [Fact]
    public async Task Handle_GivenMissingEntity_ThenReturnsNotFound()
    {
        var id = Guid.NewGuid();
        _repo.GetByIdAsync(new JobApplicationId(id), Arg.Any<CancellationToken>())
             .Returns((JobApplication?)null);

        var cmd = new UpdateJobApplicationCommand(id, "X", "Y", null, null, null, Stage.Applied, null, null);
        var result = await _handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorKind.NotFound, result.Kind);
    }

    [Fact]
    public async Task Handle_GivenExistingEntity_ThenCallsUpdateAndSaveChanges()
    {
        var app = new JobApplicationBuilder().Build();
        _repo.GetByIdAsync(app.Id, Arg.Any<CancellationToken>()).Returns(app);

        var cmd = new UpdateJobApplicationCommand(app.Id.Value, "New Title", "NewCo",
            null, null, null, Stage.Screening, null, null);
        await _handler.Handle(cmd, CancellationToken.None);

        _repo.Received(1).Update(app);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_GivenExistingEntity_ThenReturnsSuccessWithUpdatedFields()
    {
        var app = new JobApplicationBuilder().Build();
        _repo.GetByIdAsync(app.Id, Arg.Any<CancellationToken>()).Returns(app);

        var cmd = new UpdateJobApplicationCommand(app.Id.Value, "Updated Title", "Updated Co",
            "Remote", null, null, Stage.Offer, null, null);
        var result = await _handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Updated Title", result.Value.Title);
        Assert.Equal("Updated Co", result.Value.Company);
        Assert.Equal(Stage.Offer, result.Value.Stage);
    }
}
