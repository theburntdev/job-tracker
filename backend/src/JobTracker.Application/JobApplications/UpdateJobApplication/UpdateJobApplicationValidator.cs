using FluentValidation;

namespace JobTracker.Application.JobApplications.UpdateJobApplication;

public sealed class UpdateJobApplicationValidator : AbstractValidator<UpdateJobApplicationCommand>
{
    public UpdateJobApplicationValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Company)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Location)
            .MaximumLength(200)
            .When(x => x.Location is not null);

        RuleFor(x => x.Url)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out Uri? result)
                         && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps))
            .WithMessage("'Url' must be a valid HTTP or HTTPS URL.")
            .When(x => x.Url is not null);

        RuleFor(x => x.Description)
            .MaximumLength(5000)
            .When(x => x.Description is not null);
    }
}
