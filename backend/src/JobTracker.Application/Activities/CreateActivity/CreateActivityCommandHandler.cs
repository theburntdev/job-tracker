using JobTracker.Application.Common;
using JobTracker.Application.JobApplications;
using JobTracker.Domain.Common;
using JobTracker.Domain.Jobs;
using MediatR;

namespace JobTracker.Application.Activities.CreateActivity;

public sealed class CreateActivityCommandHandler(
    IJobApplicationRepository jobApplicationRepository,
    IActivityRepository activityRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateActivityCommand, Result<ActivityResponse>>
{
    public async Task<Result<ActivityResponse>> Handle(CreateActivityCommand cmd, CancellationToken ct)
    {
        var jobApp = await jobApplicationRepository.GetByIdAsync(new JobApplicationId(cmd.JobApplicationId), ct);
        if (jobApp is null)
            return Result<ActivityResponse>.Failure("Job application not found.", ErrorKind.NotFound);

        var activity = Activity.Create(
            jobApp.Id,
            cmd.ActivityType,
            cmd.OccurredAt,
            cmd.ContactName,
            cmd.ContactEmail,
            cmd.Notes);

        await activityRepository.AddAsync(activity, ct);

        if (cmd.NewStage.HasValue)
        {
            jobApp.UpdateStage(cmd.NewStage.Value);
            jobApplicationRepository.Update(jobApp);
        }

        await unitOfWork.SaveChangesAsync(ct);

        return Result<ActivityResponse>.Success(new ActivityResponse(
            activity.Id.Value,
            activity.JobApplicationId.Value,
            jobApp.Title,
            jobApp.Company,
            activity.ActivityType,
            activity.OccurredAt,
            activity.ContactName,
            activity.ContactEmail,
            activity.Notes,
            activity.CreatedAt));
    }
}
