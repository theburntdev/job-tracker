using Bogus;
using JobTracker.Domain.Jobs;

namespace JobTracker.Testing.Common.Builders;

public sealed class JobApplicationBuilder
{
    private static readonly Faker _faker = new("en");

    private string _title        = _faker.Name.JobTitle();
    private string _company      = _faker.Company.CompanyName();
    private string? _location    = $"{_faker.Address.City()}, {_faker.Address.StateAbbr()}";
    private string? _url         = _faker.Internet.Url();
    private string? _description = _faker.Lorem.Paragraph();
    private Stage _stage         = _faker.PickRandom<Stage>();
    private DateTimeOffset? _appliedAt = DateTimeOffset.UtcNow;
    private DateTimeOffset? _postedAt  = DateTimeOffset.UtcNow.AddDays(-_faker.Random.Int(1, 60));

    public JobApplicationBuilder WithTitle(string title)           { _title = title;         return this; }
    public JobApplicationBuilder WithCompany(string company)       { _company = company;     return this; }
    public JobApplicationBuilder WithLocation(string? location)    { _location = location;   return this; }
    public JobApplicationBuilder WithUrl(string? url)              { _url = url;             return this; }
    public JobApplicationBuilder WithDescription(string? desc)     { _description = desc;    return this; }
    public JobApplicationBuilder WithStage(Stage stage)            { _stage = stage;         return this; }
    public JobApplicationBuilder WithAppliedAt(DateTimeOffset? dt) { _appliedAt = dt;        return this; }
    public JobApplicationBuilder WithPostedAt(DateTimeOffset? dt)  { _postedAt = dt;         return this; }
    public JobApplicationBuilder WithNoUrl()                        { _url = null;            return this; }
    public JobApplicationBuilder WithNoLocation()                   { _location = null;       return this; }

    public JobApplication Build() =>
        JobApplication.Create(_title, _company, _location, _url, _description, _stage, _appliedAt, _postedAt);

    public static JobApplicationBuilder Remote() =>
        new JobApplicationBuilder()
            .WithLocation("Remote")
            .WithPostedAt(DateTimeOffset.UtcNow.AddDays(-_faker.Random.Int(1, 7)));

    public static JobApplicationBuilder NoLink() =>
        new JobApplicationBuilder()
            .WithNoUrl()
            .WithPostedAt(DateTimeOffset.UtcNow.AddDays(-_faker.Random.Int(30, 90)));

    public static JobApplicationBuilder Minimal() =>
        new JobApplicationBuilder()
            .WithNoUrl()
            .WithNoLocation()
            .WithDescription(null)
            .WithPostedAt(null)
            .WithAppliedAt(null);
}
