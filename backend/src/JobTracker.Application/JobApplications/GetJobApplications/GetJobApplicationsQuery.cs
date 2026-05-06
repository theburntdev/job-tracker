using JobTracker.Application.Common;
using MediatR;

namespace JobTracker.Application.JobApplications.GetJobApplications;

public record GetJobApplicationsQuery(int Page, int PageSize) : IRequest<Page<JobApplicationResponse>>;
