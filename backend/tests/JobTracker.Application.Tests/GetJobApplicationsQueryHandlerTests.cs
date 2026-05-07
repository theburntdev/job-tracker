using JobTracker.Application.Common;
using JobTracker.Application.JobApplications;
using JobTracker.Application.JobApplications.GetJobApplications;
using JobTracker.Domain.Jobs;
using JobTracker.Testing.Common.Builders;
using NSubstitute;

namespace JobTracker.Application.Tests;

public sealed class GetJobApplicationsQueryHandlerTests
{
    private readonly IJobApplicationRepository _repo = Substitute.For<IJobApplicationRepository>();
    private readonly GetJobApplicationsQueryHandler _handler;

    public GetJobApplicationsQueryHandlerTests()
    {
        _handler = new GetJobApplicationsQueryHandler(_repo);
    }

    [Fact]
    public async Task Handle_GivenQuery_ThenPassesParamsToRepository()
    {
        var empty = new Page<JobApplication>(Array.Empty<JobApplication>(), 0, 1, 10);
        _repo.GetPagedAsync(2, 10, "appliedAt", "asc", Arg.Any<CancellationToken>()).Returns(empty);

        await _handler.Handle(new GetJobApplicationsQuery(2, 10, "appliedAt", "asc"), CancellationToken.None);

        await _repo.Received(1).GetPagedAsync(2, 10, "appliedAt", "asc", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_GivenPagedResults_ThenMapsItemsToResponse()
    {
        var app = new JobApplicationBuilder().WithTitle("Dev").WithCompany("Corp").Build();
        var pageResult = new Page<JobApplication>(new[] { app }, 1, 1, 20);
        _repo.GetPagedAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
             .Returns(pageResult);

        var result = await _handler.Handle(new GetJobApplicationsQuery(1, 20, "updatedAt", "desc"), CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal("Dev", result.Items[0].Title);
        Assert.Equal("Corp", result.Items[0].Company);
    }

    [Fact]
    public async Task Handle_GivenPagedResults_ThenPreservesPaginationMetadata()
    {
        var items = Enumerable.Range(0, 5)
            .Select(_ => new JobApplicationBuilder().Build())
            .ToList();
        var pageResult = new Page<JobApplication>(items, 42, 3, 5);
        _repo.GetPagedAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
             .Returns(pageResult);

        var result = await _handler.Handle(new GetJobApplicationsQuery(3, 5, "updatedAt", "desc"), CancellationToken.None);

        Assert.Equal(42, result.Total);
        Assert.Equal(3, result.PageNumber);
        Assert.Equal(5, result.PageSize);
        Assert.Equal(5, result.Items.Count);
    }
}
