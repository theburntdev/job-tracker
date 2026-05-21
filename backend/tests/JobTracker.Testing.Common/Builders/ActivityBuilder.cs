using Bogus;
using JobTracker.Domain.Common;
using JobTracker.Domain.Jobs;

namespace JobTracker.Testing.Common.Builders;

public sealed class ActivityBuilder
{
    private static readonly Faker _faker = new("en");

    private JobApplicationId _jobApplicationId = new(Guid.NewGuid());
    private ActivityType _activityType        = _faker.PickRandom<ActivityType>();
    private DateTimeOffset _occurredAt        = DateTimeOffset.UtcNow.AddDays(-_faker.Random.Int(1, 30));
    private string? _contactName              = null;
    private string? _contactEmail             = null;
    private string? _notes                    = null;

    public ActivityBuilder WithJobApplicationId(JobApplicationId id) { _jobApplicationId = id; return this; }
    public ActivityBuilder WithActivityType(ActivityType type)       { _activityType = type;   return this; }
    public ActivityBuilder WithOccurredAt(DateTimeOffset dt)         { _occurredAt = dt;       return this; }
    public ActivityBuilder WithContactName(string? name)             { _contactName = name;    return this; }
    public ActivityBuilder WithContactEmail(string? email)           { _contactEmail = email;  return this; }
    public ActivityBuilder WithNotes(string? notes)                  { _notes = notes;         return this; }

    public Activity Build() =>
        Activity.Create(_jobApplicationId, _activityType, _occurredAt, _contactName, _contactEmail, _notes);

    public static ActivityBuilder Applied(JobApplicationId jobApplicationId) =>
        new ActivityBuilder()
            .WithJobApplicationId(jobApplicationId)
            .WithActivityType(ActivityType.Applied)
            .WithOccurredAt(DateTimeOffset.UtcNow);

    public static ActivityBuilder Interview(JobApplicationId jobApplicationId) =>
        new ActivityBuilder()
            .WithJobApplicationId(jobApplicationId)
            .WithActivityType(ActivityType.Interview)
            .WithContactName(_faker.Name.FullName())
            .WithContactEmail(_faker.Internet.Email());
}
