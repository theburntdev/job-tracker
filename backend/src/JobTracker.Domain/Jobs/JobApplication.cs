using JobTracker.Domain.Common;

namespace JobTracker.Domain.Jobs;

public sealed class JobApplication
{
    public JobApplicationId Id { get; private set; }
    public string Title { get; private set; } = null!;
    public string Company { get; private set; } = null!;
    public string? Location { get; private set; }
    public string? Url { get; private set; }
    public string? Description { get; private set; }
    public Stage Stage { get; private set; }
    public DateTimeOffset? AppliedAt { get; private set; }
    public DateTimeOffset? PostedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private JobApplication() { }

    public void Update(
        string title,
        string company,
        string? location,
        string? url,
        string? description,
        Stage stage,
        DateTimeOffset? appliedAt,
        DateTimeOffset? postedAt)
    {
        Title = title;
        Company = company;
        Location = location;
        Url = url;
        Description = description;
        Stage = stage;
        AppliedAt = appliedAt;
        PostedAt = postedAt;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public static JobApplication Create(
        string title,
        string company,
        string? location = null,
        string? url = null,
        string? description = null,
        Stage stage = Stage.Applied,
        DateTimeOffset? appliedAt = null,
        DateTimeOffset? postedAt = null)
    {
        var now = DateTimeOffset.UtcNow;
        return new JobApplication
        {
            Id = new JobApplicationId(Guid.NewGuid()),
            Title = title,
            Company = company,
            Location = location,
            Url = url,
            Description = description,
            Stage = stage,
            AppliedAt = appliedAt,
            PostedAt = postedAt,
            CreatedAt = now,
            UpdatedAt = now
        };
    }
}
