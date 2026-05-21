using JobTracker.Domain.Common;

namespace JobTracker.Domain.Jobs;

public sealed class Activity
{
    public ActivityId Id { get; private set; }
    public JobApplicationId JobApplicationId { get; private set; }
    public ActivityType ActivityType { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }
    public string? ContactName { get; private set; }
    public string? ContactEmail { get; private set; }
    public string? Notes { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public JobApplication? JobApplication { get; private set; }

    private Activity() { }

    public static Activity Create(
        JobApplicationId jobApplicationId,
        ActivityType activityType,
        DateTimeOffset occurredAt,
        string? contactName = null,
        string? contactEmail = null,
        string? notes = null)
    {
        return new Activity
        {
            Id = new ActivityId(Guid.NewGuid()),
            JobApplicationId = jobApplicationId,
            ActivityType = activityType,
            OccurredAt = occurredAt,
            ContactName = contactName,
            ContactEmail = contactEmail,
            Notes = notes,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
