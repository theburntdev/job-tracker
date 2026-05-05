using JobTracker.Application.Common;
using JobTracker.Application.Jobs;
using JobTracker.Domain.Common;
using JobTracker.Domain.Jobs;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.Infrastructure.Repositories;

internal sealed class JobRepository : IJobRepository
{
    private readonly AppDbContext _context;

    public JobRepository(AppDbContext context) => _context = context;

    public async Task<Job?> GetByIdAsync(JobId id, CancellationToken ct = default)
        => await _context.Jobs.FirstOrDefaultAsync(j => j.Id == id, ct);

    public async Task<Page<Job>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var total = await _context.Jobs.CountAsync(ct);
        var items = await _context.Jobs
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
        return new Page<Job>(items, total, page, pageSize);
    }

    public async Task AddAsync(Job entity, CancellationToken ct = default)
        => await _context.Jobs.AddAsync(entity, ct);

    public void Update(Job entity)
        => _context.Jobs.Update(entity);

    public void Delete(Job entity)
        => _context.Jobs.Remove(entity);
}
