using JobTracker.Domain.Common;
using JobTracker.Domain.Jobs;
using MediatR;

namespace JobTracker.Application.JobApplications.CreateJobApplication;

public record CreateJobApplicationRequest(
    string Title,
    string Company,
    string? Location,
    string? Url,
    string? Description,
    Stage Stage,
    DateTimeOffset? AppliedAt,
    DateTimeOffset? PostedAt);

public record CreateJobApplicationCommand(
    string Title,
    string Company,
    string? Location,
    string? Url,
    string? Description,
    Stage Stage,
    DateTimeOffset? AppliedAt,
    DateTimeOffset? PostedAt) : IRequest<Result<JobApplicationResponse>>;
