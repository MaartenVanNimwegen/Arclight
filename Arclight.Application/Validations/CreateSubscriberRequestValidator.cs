using FluentValidation;

namespace Arclight.Application.Validators;

public class CreateSubscriberRequestValidator : AbstractValidator<SubscribeRequest>
{
    public CreateSubscriberRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email address.");
    }
}