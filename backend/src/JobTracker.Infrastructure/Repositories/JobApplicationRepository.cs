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
        var total = await _context.JobApplications.CountAsync(ct);

        // EF Core's SQLite provider rejects DateTimeOffset in ORDER BY; bypass with raw SQL.
        // col and dir are whitelist-controlled — no injection risk.
        var col = sortBy == "appliedAt" ? "AppliedAt" : "UpdatedAt";
        var dir = sortDir == "asc" ? "ASC" : "DESC";
        var offset = (page - 1) * pageSize;

        var sql = "SELECT * FROM \"JobApplications\" ORDER BY \"" + col + "\" " + dir
                + " LIMIT " + pageSize + " OFFSET " + offset;
        var items = await _context.JobApplications
            .FromSqlRaw(sql)
            .ToListAsync(ct);

        return new Page<JobApplication>(items, total, page, pageSize);
    }

    public async Task AddAsync(JobApplication entity, CancellationToken ct = default)
        => await _context.JobApplications.AddAsync(entity, ct);

    public void Update(JobApplication entity)
        => _context.JobApplications.Update(entity);

    public void Delete(JobApplication entity)
        => _context.JobApplications.Remove(entity);
}
