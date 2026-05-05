using Bogus;
using JobTracker.Domain.Jobs;

namespace JobTracker.Testing.Common.Builders;

public sealed class JobBuilder
{
    private static readonly Faker _faker = new("en");

    private string _title        = _faker.Name.JobTitle();
    private string _company      = _faker.Company.CompanyName();
    private string? _location    = $"{_faker.Address.City()}, {_faker.Address.StateAbbr()}";
    private string? _url         = _faker.Internet.Url();
    private string? _description = _faker.Lorem.Paragraph();
    private DateTimeOffset? _postedAt = DateTimeOffset.UtcNow.AddDays(-_faker.Random.Int(1, 60));

    public JobBuilder WithTitle(string title)           { _title = title;        return this; }
    public JobBuilder WithCompany(string company)       { _company = company;    return this; }
    public JobBuilder WithLocation(string? location)   { _location = location;  return this; }
    public JobBuilder WithUrl(string? url)              { _url = url;            return this; }
    public JobBuilder WithDescription(string? desc)    { _description = desc;   return this; }
    public JobBuilder WithPostedAt(DateTimeOffset? dt) { _postedAt = dt;        return this; }
    public JobBuilder WithNoUrl()                       { _url = null;           return this; }
    public JobBuilder WithNoLocation()                  { _location = null;      return this; }

    public Job Build() =>
        Job.Create(_title, _company, _location, _url, _description, _postedAt);

    public static JobBuilder Remote() =>
        new JobBuilder()
            .WithLocation("Remote")
            .WithPostedAt(DateTimeOffset.UtcNow.AddDays(-_faker.Random.Int(1, 7)));

    public static JobBuilder NoLink() =>
        new JobBuilder()
            .WithNoUrl()
            .WithPostedAt(DateTimeOffset.UtcNow.AddDays(-_faker.Random.Int(30, 90)));

    public static JobBuilder Minimal() =>
        new JobBuilder()
            .WithNoUrl()
            .WithNoLocation()
            .WithDescription(null)
            .WithPostedAt(null);
}
