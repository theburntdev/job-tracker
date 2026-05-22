using JobTracker.Application.Activities.GetActivities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace JobTracker.Api.Endpoints;

public static class ActivityEndpoints
{
    public static IEndpointRouteBuilder MapActivityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/activities");

        group.MapGet("", async (
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            ISender sender = null!,
            CancellationToken ct = default) =>
        {
            var result = await sender.Send(new GetActivitiesQuery(page, pageSize), ct);
            return TypedResults.Ok(result);
        });

        return app;
    }
}
