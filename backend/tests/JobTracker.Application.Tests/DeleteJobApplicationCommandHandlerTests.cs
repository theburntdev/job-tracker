using JobTracker.Application.Common;
using JobTracker.Application.JobApplications;
using JobTracker.Application.JobApplications.DeleteJobApplication;
using JobTracker.Domain.Common;
using JobTracker.Domain.Jobs;
using JobTracker.Testing.Common.Builders;
using NSubstitute;

namespace JobTracker.Application.Tests;

public sealed class DeleteJobApplicationCommandHandlerTests
{
    private readonly IJobApplicationRepository _repo = Substitute.For<IJobApplicationRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly DeleteJobApplicationCommandHandler _handler;

    public DeleteJobApplicationCommandHandlerTests()
    {
        _handler = new DeleteJobApplicationCommandHandler(_repo, _uow);
    }

    [Fact]
    public async Task Handle_GivenExistingApplication_ThenCallsDeleteAndSaveChanges()
    {
        var app = new JobApplicationBuilder().Build();
        _repo.GetByIdAsync(app.Id, Arg.Any<CancellationToken>()).Returns(app);

        await _handler.Handle(new DeleteJobApplicationCommand(app.Id.Value), CancellationToken.None);

        _repo.Received(1).Delete(app);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_GivenNonExistentId_ThenDoesNotCallDeleteOrSaveChanges()
    {
        var id = Guid.NewGuid();
        _repo.GetByIdAsync(new JobApplicationId(id), Arg.Any<CancellationToken>())
             .Returns((JobApplication?)null);

        await _handler.Handle(new DeleteJobApplicationCommand(id), CancellationToken.None);

        _repo.DidNotReceive().Delete(Arg.Any<JobApplication>());
        await _uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
