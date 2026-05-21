using JobTracker.Application.Activities;
using JobTracker.Application.Common;
using JobTracker.Domain.Common;
using JobTracker.Domain.Jobs;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.Infrastructure.Repositories;

internal sealed class ActivityRepository : IActivityRepository
{
    private readonly AppDbContext _context;

    public ActivityRepository(AppDbContext context) => _context = context;

    public async Task<Activity?> GetByIdAsync(ActivityId id, CancellationToken ct = default)
        => await _context.Activities.FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<Page<Activity>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
        => await GetPagedAsync(page, pageSize, ct);

    public async Task<Page<Activity>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var total = await _context.Activities.CountAsync(ct);
        var offset = (page - 1) * pageSize;
        var items = await _context.Activities
            .Include("JobApplication")
            .OrderByDescending(a => a.OccurredAt)
            .ThenBy(a => a.Id)
            .Skip(offset)
            .Take(pageSize)
            .ToListAsync(ct);

        return new Page<Activity>(items, total, page, pageSize);
    }

    public async Task AddAsync(Activity entity, CancellationToken ct = default)
        => await _context.Activities.AddAsync(entity, ct);

    public void Update(Activity entity)
        => _context.Activities.Update(entity);

    public void Delete(Activity entity)
        => _context.Activities.Remove(entity);
}
