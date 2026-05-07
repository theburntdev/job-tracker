using MediatR;

namespace JobTracker.Application.JobApplications.DeleteJobApplication;

public record DeleteJobApplicationCommand(Guid Id) : IRequest<Unit>;
