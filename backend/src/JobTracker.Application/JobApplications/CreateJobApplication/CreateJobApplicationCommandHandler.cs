using JobTracker.Application.Common;
using JobTracker.Domain.Common;
using JobTracker.Domain.Jobs;
using MediatR;

namespace JobTracker.Application.JobApplications.CreateJobApplication;

public sealed class CreateJobApplicationCommandHandler(
    IJobApplicationRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateJobApplicationCommand, Result<JobApplicationResponse>>
{
    private static readonly JobApplicationMapper _mapper = new();

    public async Task<Result<JobApplicationResponse>> Handle(CreateJobApplicationCommand cmd, CancellationToken ct)
    {
        var app = JobApplication.Create(
            cmd.Title,
            cmd.Company,
            cmd.Location,
            cmd.Url,
            cmd.Description,
            cmd.Stage,
            cmd.AppliedAt,
            cmd.PostedAt);

        await repository.AddAsync(app, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result<JobApplicationResponse>.Success(_mapper.ToResponse(app));
    }
}
