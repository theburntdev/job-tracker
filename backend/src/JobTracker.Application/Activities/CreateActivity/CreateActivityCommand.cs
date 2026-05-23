using JobTracker.Domain.Common;
using JobTracker.Domain.Jobs;
using MediatR;

namespace JobTracker.Application.Activities.CreateActivity;

public record CreateActivityRequest(
    Guid JobApplicationId,
    ActivityType ActivityType,
    DateTimeOffset OccurredAt,
    string? ContactName,
    string? ContactEmail,
    string? Notes,
    Stage? NewStage);

public record CreateActivityCommand(
    Guid JobApplicationId,
    ActivityType ActivityType,
    DateTimeOffset OccurredAt,
    string? ContactName,
    string? ContactEmail,
    string? Notes,
    Stage? NewStage) : IRequest<Result<ActivityResponse>>;
