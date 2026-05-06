using JobTracker.Application.Common;
using JobTracker.Domain.Common;
using MediatR;

namespace JobTracker.Application.JobApplications.UpdateJobApplication;

public sealed class UpdateJobApplicationCommandHandler(
    IJobApplicationRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateJobApplicationCommand, Result<JobApplicationResponse>>
{
    private static readonly JobApplicationMapper _mapper = new();

    public async Task<Result<JobApplicationResponse>> Handle(UpdateJobApplicationCommand cmd, CancellationToken ct)
    {
        var app = await repository.GetByIdAsync(new JobApplicationId(cmd.Id), ct);
        if (app is null)
            return Result<JobApplicationResponse>.Failure("Job application not found.", ErrorKind.NotFound);

        app.Update(cmd.Title, cmd.Company, cmd.Location, cmd.Url, cmd.Description, cmd.Stage, cmd.AppliedAt, cmd.PostedAt);
        repository.Update(app);
        await unitOfWork.SaveChangesAsync(ct);

        return Result<JobApplicationResponse>.Success(_mapper.ToResponse(app));
    }
}
