using JobTracker.Application.Common;
using JobTracker.Domain.Common;
using MediatR;

namespace JobTracker.Application.JobApplications.DeleteJobApplication;

public sealed class DeleteJobApplicationCommandHandler(
    IJobApplicationRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteJobApplicationCommand, Unit>
{
    public async Task<Unit> Handle(DeleteJobApplicationCommand cmd, CancellationToken ct)
    {
        var app = await repository.GetByIdAsync(new JobApplicationId(cmd.Id), ct);
        if (app is null)
            return Unit.Value;

        repository.Delete(app);
        await unitOfWork.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
