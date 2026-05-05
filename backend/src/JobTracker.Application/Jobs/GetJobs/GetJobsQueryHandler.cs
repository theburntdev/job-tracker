using JobTracker.Application.Common;
using MediatR;

namespace JobTracker.Application.Jobs.GetJobs;

public sealed class GetJobsQueryHandler(IJobRepository repository)
    : IRequestHandler<GetJobsQuery, Page<JobResponse>>
{
    private static readonly JobMapper _mapper = new();

    public async Task<Page<JobResponse>> Handle(GetJobsQuery query, CancellationToken ct)
    {
        var page = await repository.GetAllAsync(query.Page, query.PageSize, ct);
        return new Page<JobResponse>(
            page.Items.Select(_mapper.ToResponse).ToList().AsReadOnly(),
            page.Total,
            page.PageNumber,
            page.PageSize);
    }
}
