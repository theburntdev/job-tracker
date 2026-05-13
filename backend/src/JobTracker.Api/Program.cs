using System.Text.Json.Serialization;
using FluentValidation;
using JobTracker.Api;
using JobTracker.Api.Endpoints;
using JobTracker.Application.Common;
using JobTracker.Application.JobApplications.GetJobApplications;
using JobTracker.Infrastructure;
using JobTracker.Infrastructure.Persistence;
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
    builder.Services.AddValidatorsFromAssembly(typeof(GetJobApplicationsQueryHandler).Assembly);

    builder.Services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssemblies(
            typeof(Program).Assembly,
            typeof(GetJobApplicationsQueryHandler).Assembly);
        cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    });

    builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
    builder.Services.AddProblemDetails();

    builder.Services.ConfigureHttpJsonOptions(options =>
        options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

    var dbPath = Environment.GetEnvironmentVariable("DB_PATH");
    if (!string.IsNullOrEmpty(dbPath))
        builder.Configuration["ConnectionStrings:Default"] = $"Data Source={dbPath}";

    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("Dev", policy =>
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod());

        options.AddPolicy("Tauri", policy =>
            policy.WithOrigins("https://tauri.localhost", "http://tauri.localhost", "tauri://localhost")
                  .AllowAnyHeader()
                  .AllowAnyMethod());
    });

    var app = builder.Build();

    await using (var scope = app.Services.CreateAsyncScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    app.UseExceptionHandler();

    // Testing env also needs the Dev CORS policy (Vite on 5173)
    app.UseCors(
        app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing")
            ? "Dev"
            : "Tauri");

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.MapGet("/health", () => Results.Ok());

    app.MapJobApplicationEndpoints();

    // Test-only endpoint — wipes all job applications for per-test isolation
    if (app.Environment.IsEnvironment("Testing"))
    {
        app.MapDelete("/api/test/reset", async (AppDbContext db) =>
        {
            await db.JobApplications.ExecuteDeleteAsync();
            return Results.NoContent();
        });
    }

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
