using JobTracker.Application.JobApplications.GetJobApplications;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace JobTracker.Api.Endpoints;

public static class JobApplicationEndpoints
{
    public static IEndpointRouteBuilder MapJobApplicationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/job-applications");

        group.MapGet("", async (
            ISender mediator,
            CancellationToken ct,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20) =>
        {
            var result = await mediator.Send(new GetJobApplicationsQuery(page, pageSize), ct);
            return TypedResults.Ok(result);
        });

        return app;
    }
}
