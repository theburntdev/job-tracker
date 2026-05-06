using JobTracker.Application.Common;
using JobTracker.Domain.Common;
using JobTracker.Domain.Jobs;

namespace JobTracker.Application.JobApplications;

public interface IJobApplicationRepository : IRepository<JobApplication, JobApplicationId>
{
    Task<Page<JobApplication>> GetPagedAsync(int page, int pageSize, string sortBy, string sortDir, CancellationToken ct = default);
}
