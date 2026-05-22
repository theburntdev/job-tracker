using JobTracker.Application.Common;
using MediatR;

namespace JobTracker.Application.Activities.GetActivities;

public sealed class GetActivitiesQueryHandler : IRequestHandler<GetActivitiesQuery, Page<ActivityResponse>>
{
    private readonly IActivityRepository _repo;
    private readonly ActivityMapper _mapper = new();

    public GetActivitiesQueryHandler(IActivityRepository repo)
    {
        _repo = repo;
    }

    public async Task<Page<ActivityResponse>> Handle(GetActivitiesQuery request, CancellationToken cancellationToken)
    {
        var page = await _repo.GetPagedAsync(request.Page, request.PageSize, cancellationToken);
        var items = page.Items.Select(_mapper.ToResponse).ToList();
        return new Page<ActivityResponse>(items, page.Total, page.PageNumber, page.PageSize);
    }
}
