using Arclight.Application.DTOs;
using FluentValidation;

namespace Arclight.Application.Validations;

public class CreateCommentRequestValidator : AbstractValidator<CreateCommentRequest>
{
    public CreateCommentRequestValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Comment text cannot be empty.");
    }
}
