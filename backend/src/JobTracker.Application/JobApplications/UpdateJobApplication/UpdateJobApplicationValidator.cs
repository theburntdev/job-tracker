using FluentValidation;
using JobTracker.Domain.Jobs;

namespace JobTracker.Application.JobApplications.UpdateJobApplication;

public sealed class UpdateJobApplicationValidator : AbstractValidator<UpdateJobApplicationCommand>
{
    public UpdateJobApplicationValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(JobApplicationConstraints.TitleMaxLength);

        RuleFor(x => x.Company)
            .NotEmpty()
            .MaximumLength(JobApplicationConstraints.CompanyMaxLength);

        RuleFor(x => x.Location)
            .MaximumLength(JobApplicationConstraints.LocationMaxLength)
            .When(x => x.Location is not null);

        RuleFor(x => x.Url)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out Uri? result)
                         && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps))
            .WithMessage("'Url' must be a valid HTTP or HTTPS URL.")
            .When(x => x.Url is not null);

        RuleFor(x => x.Description)
            .MaximumLength(JobApplicationConstraints.DescriptionMaxLength)
            .When(x => x.Description is not null);
    }
}
