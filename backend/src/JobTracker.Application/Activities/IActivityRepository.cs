using JobTracker.Application.Common;
using JobTracker.Domain.Common;
using JobTracker.Domain.Jobs;

namespace JobTracker.Application.Activities;

public interface IActivityRepository : IRepository<Activity, ActivityId>
{
    Task<Page<Activity>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
}
