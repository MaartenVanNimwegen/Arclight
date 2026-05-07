using FluentValidation;

namespace Arclight.Application.Validators;

public class SendNewsletterRequestValidator : AbstractValidator<SendNewsletterRequest>
{
    public SendNewsletterRequestValidator()
    {
        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Subject is required.")
            .MaximumLength(200).WithMessage("Subject cannot be longer than 200 characters.");

        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("The content of the newsletter cannot be empty.");
    }
}