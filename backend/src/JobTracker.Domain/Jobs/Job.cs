using JobTracker.Domain.Common;

namespace JobTracker.Domain.Jobs;

public sealed class Job
{
    public JobId Id { get; private set; }
    public string Title { get; private set; } = null!;
    public string Company { get; private set; } = null!;
    public string? Location { get; private set; }
    public string? Url { get; private set; }
    public string? Description { get; private set; }
    public DateTimeOffset? PostedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private Job() { }

    public static Job Create(
        string title,
        string company,
        string? location = null,
        string? url = null,
        string? description = null,
        DateTimeOffset? postedAt = null)
    {
        var now = DateTimeOffset.UtcNow;
        return new Job
        {
            Id = new JobId(Guid.NewGuid()),
            Title = title,
            Company = company,
            Location = location,
            Url = url,
            Description = description,
            PostedAt = postedAt,
            CreatedAt = now,
            UpdatedAt = now
        };
    }
}
