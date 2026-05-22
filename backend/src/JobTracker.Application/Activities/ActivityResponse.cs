using JobTracker.Domain.Jobs;

namespace JobTracker.Application.Activities;

public record ActivityResponse(
    Guid Id,
    Guid JobApplicationId,
    string? JobTitle,
    string? Company,
    ActivityType ActivityType,
    DateTimeOffset OccurredAt,
    string? ContactName,
    string? ContactEmail,
    string? Notes,
    DateTimeOffset CreatedAt);
