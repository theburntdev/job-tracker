using JobTracker.Application.Activities.CreateActivity;
using JobTracker.Application.Activities.GetActivities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace JobTracker.Api.Endpoints;

public static class ActivityEndpoints
{
    public static IEndpointRouteBuilder MapActivityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/activities");

        group.MapPost("", async (
            CreateActivityRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var cmd = new CreateActivityCommand(
                request.JobApplicationId,
                request.ActivityType,
                request.OccurredAt,
                request.ContactName,
                request.ContactEmail,
                request.Notes,
                request.NewStage);
            var result = await sender.Send(cmd, ct);
            return result.ToCreatedResult(a => $"/api/activities/{a.Id}");
        });

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
