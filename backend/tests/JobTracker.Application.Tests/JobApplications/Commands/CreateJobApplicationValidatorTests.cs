using JobTracker.Application.JobApplications.CreateJobApplication;
using JobTracker.Domain.Jobs;

namespace JobTracker.Application.Tests.JobApplications.Commands;

public sealed class CreateJobApplicationValidatorTests
{
    private readonly CreateJobApplicationValidator _validator = new();

    private static CreateJobApplicationCommand ValidCommand() =>
        new("Software Engineer", "Acme Corp", null, null, null, Stage.Applied, null, null);

    [Fact]
    public async Task Validate_GivenValidCommand_ThenIsValid()
    {
        var result = await _validator.ValidateAsync(ValidCommand());

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validate_GivenEmptyTitle_ThenIsInvalid(string title)
    {
        var cmd = ValidCommand() with { Title = title };

        var result = await _validator.ValidateAsync(cmd);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Title");
    }

    [Fact]
    public async Task Validate_GivenTitleExceeds200Chars_ThenIsInvalid()
    {
        var cmd = ValidCommand() with { Title = new string('x', 201) };

        var result = await _validator.ValidateAsync(cmd);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Title");
    }

    [Fact]
    public async Task Validate_GivenTitleExactly200Chars_ThenIsValid()
    {
        var cmd = ValidCommand() with { Title = new string('x', 200) };

        var result = await _validator.ValidateAsync(cmd);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validate_GivenEmptyCompany_ThenIsInvalid(string company)
    {
        var cmd = ValidCommand() with { Company = company };

        var result = await _validator.ValidateAsync(cmd);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Company");
    }

    [Fact]
    public async Task Validate_GivenCompanyExceeds200Chars_ThenIsInvalid()
    {
        var cmd = ValidCommand() with { Company = new string('x', 201) };

        var result = await _validator.ValidateAsync(cmd);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Company");
    }

    [Fact]
    public async Task Validate_GivenLocationNull_ThenIsValid()
    {
        var cmd = ValidCommand() with { Location = null };

        var result = await _validator.ValidateAsync(cmd);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_GivenLocationExceeds200Chars_ThenIsInvalid()
    {
        var cmd = ValidCommand() with { Location = new string('x', 201) };

        var result = await _validator.ValidateAsync(cmd);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Location");
    }

    [Fact]
    public async Task Validate_GivenUrlNull_ThenIsValid()
    {
        var cmd = ValidCommand() with { Url = null };

        var result = await _validator.ValidateAsync(cmd);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("https://example.com")]
    [InlineData("http://jobs.acme.io/posting/123")]
    public async Task Validate_GivenValidUrl_ThenIsValid(string url)
    {
        var cmd = ValidCommand() with { Url = url };

        var result = await _validator.ValidateAsync(cmd);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("ftp://files.example.com")]
    [InlineData("example.com")]
    public async Task Validate_GivenInvalidUrl_ThenIsInvalid(string url)
    {
        var cmd = ValidCommand() with { Url = url };

        var result = await _validator.ValidateAsync(cmd);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Url");
    }

    [Fact]
    public async Task Validate_GivenDescriptionNull_ThenIsValid()
    {
        var cmd = ValidCommand() with { Description = null };

        var result = await _validator.ValidateAsync(cmd);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_GivenDescriptionExceeds5000Chars_ThenIsInvalid()
    {
        var cmd = ValidCommand() with { Description = new string('x', 5001) };

        var result = await _validator.ValidateAsync(cmd);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Description");
    }

    [Fact]
    public async Task Validate_GivenDescriptionExactly5000Chars_ThenIsValid()
    {
        var cmd = ValidCommand() with { Description = new string('x', 5000) };

        var result = await _validator.ValidateAsync(cmd);

        Assert.True(result.IsValid);
    }
}
