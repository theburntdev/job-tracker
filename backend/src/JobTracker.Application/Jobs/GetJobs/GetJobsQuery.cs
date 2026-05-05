using JobTracker.Application.Common;
using MediatR;

namespace JobTracker.Application.Jobs.GetJobs;

public record GetJobsQuery(int Page, int PageSize) : IRequest<Page<JobResponse>>;
