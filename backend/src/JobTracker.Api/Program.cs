using System.Text.Json.Serialization;
using JobTracker.Api.Endpoints;
using JobTracker.Application.JobApplications.GetJobApplications;
using JobTracker.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, services, cfg) =>
        cfg.ReadFrom.Configuration(ctx.Configuration)
           .ReadFrom.Services(services));

    builder.Services.AddOpenApi();
    builder.Services.AddMediatR(cfg =>
        cfg.RegisterServicesFromAssemblies(
            typeof(Program).Assembly,
            typeof(GetJobApplicationsQueryHandler).Assembly));

    builder.Services.ConfigureHttpJsonOptions(options =>
        options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.AddCors(options =>
        options.AddPolicy("Dev", policy =>
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod()));

    var app = builder.Build();

    await using (var scope = app.Services.CreateAsyncScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.UseCors("Dev");
    }

    app.MapJobApplicationEndpoints();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
