using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using JobTracker.Application.Activities;
using JobTracker.Domain.Jobs;
using JobTracker.Infrastructure;
using JobTracker.Testing.Common.Builders;
using Microsoft.Extensions.DependencyInjection;

namespace JobTracker.Api.Tests.ActivityEndpoints;

[Collection("ApiIntegration")]
public sealed class CreateActivityEndpointTests : IAsyncLifetime
{
    private JobTrackerApiFactory _factory = null!;
    private HttpClient _client = null!;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public async Task InitializeAsync()
    {
        _factory = new JobTrackerApiFactory();
        _client = _factory.CreateClient();

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        _factory.Dispose();
        return Task.CompletedTask;
    }

    private async Task ResetAsync()
    {
        await _client.DeleteAsync("/api/test/reset");
    }

    [Fact]
    public async Task PostActivity_GivenEmptyJobApplicationId_ThenReturns422()
    {
        await ResetAsync();

        var request = new
        {
            jobApplicationId = Guid.Empty,
            activityType = "PhoneScreen",
            occurredAt = DateTimeOffset.UtcNow
        };

        var response = await _client.PostAsJsonAsync("/api/activities", request, JsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostActivity_GivenUnknownJobApplicationId_ThenReturns404()
    {
        await ResetAsync();

        var request = new
        {
            jobApplicationId = Guid.NewGuid(),
            activityType = "PhoneScreen",
            occurredAt = DateTimeOffset.UtcNow
        };

        var response = await _client.PostAsJsonAsync("/api/activities", request, JsonOptions);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostActivity_GivenNoNewStage_ThenStageUnchanged()
    {
        await ResetAsync();

        await using var setupScope = _factory.Services.CreateAsyncScope();
        var setupDb = setupScope.ServiceProvider.GetRequiredService<AppDbContext>();

        var app = new JobApplicationBuilder().WithStage(Stage.Applied).Build();
        await setupDb.JobApplications.AddAsync(app);
        await setupDb.SaveChangesAsync();

        var request = new
        {
            jobApplicationId = app.Id.Value,
            activityType = "PhoneScreen",
            occurredAt = DateTimeOffset.UtcNow
        };

        await _client.PostAsJsonAsync("/api/activities", request, JsonOptions);

        await using var assertScope = _factory.Services.CreateAsyncScope();
        var assertDb = assertScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var unchanged = await assertDb.JobApplications.FindAsync(app.Id);
        Assert.Equal(Stage.Applied, unchanged!.Stage);
    }

    [Fact]
    public async Task PostActivity_GivenNewStage_ThenUpdatesJobApplicationStage()
    {
        await ResetAsync();

        await using var setupScope = _factory.Services.CreateAsyncScope();
        var setupDb = setupScope.ServiceProvider.GetRequiredService<AppDbContext>();

        var app = new JobApplicationBuilder().WithStage(Stage.Applied).Build();
        await setupDb.JobApplications.AddAsync(app);
        await setupDb.SaveChangesAsync();

        var request = new
        {
            jobApplicationId = app.Id.Value,
            activityType = "PhoneScreen",
            occurredAt = DateTimeOffset.UtcNow,
            newStage = "Screening"
        };

        await _client.PostAsJsonAsync("/api/activities", request, JsonOptions);

        await using var assertScope = _factory.Services.CreateAsyncScope();
        var assertDb = assertScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var updated = await assertDb.JobApplications.FindAsync(app.Id);
        Assert.Equal(Stage.Screening, updated!.Stage);
    }

    [Fact]
    public async Task PostActivity_GivenValidRequest_ThenReturns201WithActivity()
    {
        await ResetAsync();

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var app = new JobApplicationBuilder().WithStage(Stage.Applied).Build();
        await db.JobApplications.AddAsync(app);
        await db.SaveChangesAsync();

        var request = new
        {
            jobApplicationId = app.Id.Value,
            activityType = "PhoneScreen",
            occurredAt = DateTimeOffset.UtcNow
        };

        var response = await _client.PostAsJsonAsync("/api/activities", request, JsonOptions);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        var body = await response.Content.ReadFromJsonAsync<ActivityResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(app.Id.Value, body.JobApplicationId);
        Assert.Equal(ActivityType.PhoneScreen, body.ActivityType);
        Assert.NotEqual(Guid.Empty, body.Id);
    }
}
