using FluentValidation;

namespace JobTracker.Application.Activities.CreateActivity;

public sealed class CreateActivityCommandValidator : AbstractValidator<CreateActivityCommand>
{
    public CreateActivityCommandValidator()
    {
        RuleFor(x => x.JobApplicationId)
            .NotEmpty();

        RuleFor(x => x.OccurredAt)
            .NotEmpty();

        RuleFor(x => x.ContactName)
            .MaximumLength(200)
            .When(x => x.ContactName is not null);

        RuleFor(x => x.ContactEmail)
            .MaximumLength(320)
            .When(x => x.ContactEmail is not null);

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .When(x => x.Notes is not null);
    }
}
