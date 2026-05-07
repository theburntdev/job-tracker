using JobTracker.Application.JobApplications.CreateJobApplication;
using JobTracker.Application.JobApplications.DeleteJobApplication;
using JobTracker.Application.JobApplications.GetJobApplications;
using JobTracker.Application.JobApplications.UpdateJobApplication;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace JobTracker.Api.Endpoints;

public static class JobApplicationEndpoints
{
    public static IEndpointRouteBuilder MapJobApplicationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/job-applications");

        group.MapPost("", async (
            CreateJobApplicationRequest request,
            ISender mediator,
            CancellationToken ct) =>
        {
            var cmd = new CreateJobApplicationCommand(
                request.Title,
                request.Company,
                request.Location,
                request.Url,
                request.Description,
                request.Stage,
                request.AppliedAt,
                request.PostedAt);
            var result = await mediator.Send(cmd, ct);
            return result.ToCreatedResult(app => $"/api/job-applications/{app.Id}");
        });

        group.MapGet("", async (
            ISender mediator,
            CancellationToken ct,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string sortBy = "updatedAt",
            [FromQuery] string sortDir = "desc") =>
        {
            var result = await mediator.Send(new GetJobApplicationsQuery(page, pageSize, sortBy, sortDir), ct);
            return TypedResults.Ok(result);
        });

        group.MapPut("{id:guid}", async (
            Guid id,
            UpdateJobApplicationRequest request,
            ISender mediator,
            CancellationToken ct) =>
        {
            var cmd = new UpdateJobApplicationCommand(
                id,
                request.Title,
                request.Company,
                request.Location,
                request.Url,
                request.Description,
                request.Stage,
                request.AppliedAt,
                request.PostedAt);
            var result = await mediator.Send(cmd, ct);
            return result.ToHttpResult();
        });

        group.MapDelete("{id:guid}", async (
            Guid id,
            ISender mediator,
            CancellationToken ct) =>
        {
            await mediator.Send(new DeleteJobApplicationCommand(id), ct);
            return TypedResults.NoContent();
        });

        return app;
    }
}
