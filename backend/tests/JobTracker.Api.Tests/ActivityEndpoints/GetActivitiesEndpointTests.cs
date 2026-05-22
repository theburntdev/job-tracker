using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using JobTracker.Application.Activities;
using JobTracker.Application.Common;
using JobTracker.Infrastructure;
using JobTracker.Testing.Common.Builders;
using Microsoft.Extensions.DependencyInjection;

namespace JobTracker.Api.Tests.ActivityEndpoints;

public sealed class GetActivitiesEndpointTests : IAsyncLifetime
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
    public async Task GetActivities_WhenNoActivitiesExist_ThenReturns200WithEmptyPage()
    {
        await ResetAsync();

        var response = await _client.GetAsync("/api/activities?page=1&pageSize=20");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content.ReadFromJsonAsync<Page<ActivityResponse>>(JsonOptions);
        Assert.NotNull(page);
        Assert.Empty(page.Items);
        Assert.Equal(0, page.Total);
    }

    [Fact]
    public async Task GetActivities_WhenActivitiesExist_ThenReturnsActivitiesWithJobTitleAndCompany()
    {
        await ResetAsync();

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var app = new JobApplicationBuilder()
            .WithTitle("Senior Engineer")
            .WithCompany("Globex")
            .Build();
        await db.JobApplications.AddAsync(app);

        var activity = ActivityBuilder.Applied(app.Id);
        await db.Activities.AddAsync(activity.Build());
        await db.SaveChangesAsync();

        var response = await _client.GetAsync("/api/activities?page=1&pageSize=20");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content.ReadFromJsonAsync<Page<ActivityResponse>>(JsonOptions);
        Assert.NotNull(page);
        Assert.Single(page.Items);
        Assert.Equal("Senior Engineer", page.Items[0].JobTitle);
        Assert.Equal("Globex", page.Items[0].Company);
        Assert.Equal(app.Id.Value, page.Items[0].JobApplicationId);
    }

    [Fact]
    public async Task GetActivities_WithCustomPageSize_ThenRespectsPagination()
    {
        await ResetAsync();

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var app = new JobApplicationBuilder().Build();
        await db.JobApplications.AddAsync(app);

        for (int i = 0; i < 5; i++)
        {
            await db.Activities.AddAsync(new ActivityBuilder()
                .WithJobApplicationId(app.Id)
                .Build());
        }
        await db.SaveChangesAsync();

        var response = await _client.GetAsync("/api/activities?page=1&pageSize=2");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content.ReadFromJsonAsync<Page<ActivityResponse>>(JsonOptions);
        Assert.NotNull(page);
        Assert.Equal(2, page.Items.Count);
        Assert.Equal(5, page.Total);
        Assert.Equal(1, page.PageNumber);
        Assert.Equal(2, page.PageSize);
    }
}
