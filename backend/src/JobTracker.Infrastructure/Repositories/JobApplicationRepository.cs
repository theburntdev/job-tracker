using JobTracker.Application.Common;
using JobTracker.Application.JobApplications;
using JobTracker.Domain.Common;
using JobTracker.Domain.Jobs;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.Infrastructure.Repositories;

internal sealed class JobApplicationRepository : IJobApplicationRepository
{
    private readonly AppDbContext _context;

    public JobApplicationRepository(AppDbContext context) => _context = context;

    public async Task<JobApplication?> GetByIdAsync(JobApplicationId id, CancellationToken ct = default)
        => await _context.JobApplications.FirstOrDefaultAsync(j => j.Id == id, ct);

    public async Task<Page<JobApplication>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
        => await GetPagedAsync(page, pageSize, "updatedAt", "desc", ct);

    public async Task<Page<JobApplication>> GetPagedAsync(int page, int pageSize, string sortBy, string sortDir, CancellationToken ct = default)
    {
        IQueryable<JobApplication> query = _context.JobApplications;
        var ascending = string.Equals(sortDir, "asc", StringComparison.OrdinalIgnoreCase);
        var byAppliedAt = string.Equals(sortBy, "appliedAt", StringComparison.OrdinalIgnoreCase);

        if (byAppliedAt)
            query = ascending
                ? query.OrderBy(j => j.AppliedAt).ThenBy(j => j.Id)
                : query.OrderByDescending(j => j.AppliedAt).ThenBy(j => j.Id);
        else
            query = ascending
                ? query.OrderBy(j => j.UpdatedAt).ThenBy(j => j.Id)
                : query.OrderByDescending(j => j.UpdatedAt).ThenBy(j => j.Id);

        var total = await _context.JobApplications.CountAsync(ct);
        var offset = (page - 1) * pageSize;
        var items = await query.Skip(offset).Take(pageSize).ToListAsync(ct);

        return new Page<JobApplication>(items, total, page, pageSize);
    }

    public async Task AddAsync(JobApplication entity, CancellationToken ct = default)
        => await _context.JobApplications.AddAsync(entity, ct);

    public void Update(JobApplication entity)
        => _context.JobApplications.Update(entity);

    public void Delete(JobApplication entity)
        => _context.JobApplications.Remove(entity);
}
