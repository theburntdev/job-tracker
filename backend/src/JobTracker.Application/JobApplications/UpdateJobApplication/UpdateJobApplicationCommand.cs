using JobTracker.Domain.Common;
using JobTracker.Domain.Jobs;
using MediatR;

namespace JobTracker.Application.JobApplications.UpdateJobApplication;

public record UpdateJobApplicationRequest(
    string Title,
    string Company,
    string? Location,
    string? Url,
    string? Description,
    Stage Stage,
    DateTimeOffset? AppliedAt,
    DateTimeOffset? PostedAt);

public record UpdateJobApplicationCommand(
    Guid Id,
    string Title,
    string Company,
    string? Location,
    string? Url,
    string? Description,
    Stage Stage,
    DateTimeOffset? AppliedAt,
    DateTimeOffset? PostedAt) : IRequest<Result<JobApplicationResponse>>;
