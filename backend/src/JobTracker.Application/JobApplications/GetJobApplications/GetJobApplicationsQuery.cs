using JobTracker.Application.Common;
using MediatR;

namespace JobTracker.Application.JobApplications.GetJobApplications;

public record GetJobApplicationsQuery(int Page, int PageSize, string SortBy, string SortDir) : IRequest<Page<JobApplicationResponse>>;
