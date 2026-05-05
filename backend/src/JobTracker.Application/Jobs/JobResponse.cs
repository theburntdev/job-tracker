namespace JobTracker.Application.Jobs;

public record JobResponse(
    Guid Id,
    string Title,
    string Company,
    string? Location,
    string? Url,
    string? Description,
    DateTimeOffset? PostedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
