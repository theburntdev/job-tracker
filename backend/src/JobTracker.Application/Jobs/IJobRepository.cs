using JobTracker.Application.Common;
using JobTracker.Domain.Common;
using JobTracker.Domain.Jobs;

namespace JobTracker.Application.Jobs;

public interface IJobRepository : IRepository<Job, JobId>
{
}
