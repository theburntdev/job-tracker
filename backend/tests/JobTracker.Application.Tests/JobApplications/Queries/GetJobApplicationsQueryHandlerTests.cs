using JobTracker.Application.Common;
using JobTracker.Application.JobApplications;
using JobTracker.Application.JobApplications.GetJobApplications;
using JobTracker.Domain.Jobs;
using JobTracker.Testing.Common.Builders;
using NSubstitute;

namespace JobTracker.Application.Tests.JobApplications.Queries;

public sealed class GetJobApplicationsQueryHandlerTests
{
    private readonly IJobApplicationRepository _repo = Substitute.For<IJobApplicationRepository>();
    private readonly GetJobApplicationsQueryHandler _handler;

    public GetJobApplicationsQueryHandlerTests()
    {
        _handler = new GetJobApplicationsQueryHandler(_repo);
    }

    [Fact]
    public async Task Handle_GivenPagedResults_ThenReturnsItemsWithPaginationMetadata()
    {
        List<JobApplication> items = Enumerable.Range(0, 3)
            .Select(_ => new JobApplicationBuilder().Build())
            .ToList();
        Page<JobApplication> pageResult = new Page<JobApplication>(items, 30, 2, 3);
        _repo.GetPagedAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
             .Returns(pageResult);

        Page<JobApplicationResponse> result = await _handler.Handle(
            new GetJobApplicationsQuery(2, 3, "updatedAt", "desc"), CancellationToken.None);

        Assert.Equal(3, result.Items.Count);
        Assert.Equal(30, result.Total);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(3, result.PageSize);
    }

    [Fact]
    public async Task Handle_GivenAscendingSortDirection_ThenPassesSortDirectionToRepository()
    {
        Page<JobApplication> empty = new Page<JobApplication>(Array.Empty<JobApplication>(), 0, 1, 20);
        _repo.GetPagedAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
             .Returns(empty);

        await _handler.Handle(new GetJobApplicationsQuery(1, 20, "appliedAt", "asc"), CancellationToken.None);

        await _repo.Received(1).GetPagedAsync(1, 20, "appliedAt", "asc", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_GivenDescendingSortDirection_ThenPassesSortDirectionToRepository()
    {
        Page<JobApplication> empty = new Page<JobApplication>(Array.Empty<JobApplication>(), 0, 1, 20);
        _repo.GetPagedAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
             .Returns(empty);

        await _handler.Handle(new GetJobApplicationsQuery(1, 20, "updatedAt", "desc"), CancellationToken.None);

        await _repo.Received(1).GetPagedAsync(1, 20, "updatedAt", "desc", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_GivenNoData_ThenReturnsEmptyItemsWithZeroTotal()
    {
        Page<JobApplication> empty = new Page<JobApplication>(Array.Empty<JobApplication>(), 0, 1, 20);
        _repo.GetPagedAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
             .Returns(empty);

        Page<JobApplicationResponse> result = await _handler.Handle(
            new GetJobApplicationsQuery(1, 20, "updatedAt", "desc"), CancellationToken.None);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.Total);
    }

    [Fact]
    public async Task Handle_GivenResults_ThenMapsFieldsToResponse()
    {
        DateTimeOffset appliedAt = new DateTimeOffset(2025, 3, 1, 0, 0, 0, TimeSpan.Zero);
        JobApplication app = new JobApplicationBuilder()
            .WithTitle("Staff Engineer")
            .WithCompany("Globex")
            .WithStage(Stage.Interviewing)
            .WithAppliedAt(appliedAt)
            .Build();
        Page<JobApplication> pageResult = new Page<JobApplication>(new[] { app }, 1, 1, 20);
        _repo.GetPagedAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
             .Returns(pageResult);

        Page<JobApplicationResponse> result = await _handler.Handle(
            new GetJobApplicationsQuery(1, 20, "updatedAt", "desc"), CancellationToken.None);

        JobApplicationResponse response = result.Items[0];
        Assert.Equal("Staff Engineer", response.Title);
        Assert.Equal("Globex", response.Company);
        Assert.Equal(Stage.Interviewing, response.Stage);
        Assert.Equal(appliedAt, response.AppliedAt);
    }
}
