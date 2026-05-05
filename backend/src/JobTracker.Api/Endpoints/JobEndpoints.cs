using JobTracker.Application.Jobs.GetJobs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace JobTracker.Api.Endpoints;

public static class JobEndpoints
{
    public static IEndpointRouteBuilder MapJobEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/jobs");

        group.MapGet("", async (
            ISender mediator,
            CancellationToken ct,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20) =>
        {
            var result = await mediator.Send(new GetJobsQuery(page, pageSize), ct);
            return TypedResults.Ok(result);
        });

        return app;
    }
}
