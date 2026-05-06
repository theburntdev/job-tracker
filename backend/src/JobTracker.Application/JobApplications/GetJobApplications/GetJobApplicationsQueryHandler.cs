using JobTracker.Application.Common;
using MediatR;

namespace JobTracker.Application.JobApplications.GetJobApplications;

public sealed class GetJobApplicationsQueryHandler(IJobApplicationRepository repository)
    : IRequestHandler<GetJobApplicationsQuery, Page<JobApplicationResponse>>
{
    private static readonly JobApplicationMapper _mapper = new();

    public async Task<Page<JobApplicationResponse>> Handle(GetJobApplicationsQuery query, CancellationToken ct)
    {
        var page = await repository.GetPagedAsync(query.Page, query.PageSize, query.SortBy, query.SortDir, ct);
        return new Page<JobApplicationResponse>(
            page.Items.Select(_mapper.ToResponse).ToList().AsReadOnly(),
            page.Total,
            page.PageNumber,
            page.PageSize);
    }
}
