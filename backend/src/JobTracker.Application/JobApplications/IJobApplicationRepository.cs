using JobTracker.Application.Common;
using JobTracker.Domain.Common;
using JobTracker.Domain.Jobs;

namespace JobTracker.Application.JobApplications;

public interface IJobApplicationRepository : IRepository<JobApplication, JobApplicationId>
{
}
