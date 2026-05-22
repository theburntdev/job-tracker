using JobTracker.Application.Common;
using MediatR;

namespace JobTracker.Application.Activities.GetActivities;

public record GetActivitiesQuery(int Page, int PageSize) : IRequest<Page<ActivityResponse>>;
