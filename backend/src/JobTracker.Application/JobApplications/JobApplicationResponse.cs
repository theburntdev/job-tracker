using JobTracker.Domain.Jobs;

namespace JobTracker.Application.JobApplications;

public record JobApplicationResponse(
    Guid Id,
    string Title,
    string Company,
    string? Location,
    string? Url,
    string? Description,
    Stage Stage,
    DateTimeOffset? AppliedAt,
    DateTimeOffset? PostedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
