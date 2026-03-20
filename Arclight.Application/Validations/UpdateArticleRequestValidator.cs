using Arclight.Application.DTOs;
using FluentValidation;

namespace Arclight.Application.Validations;

public class UpdateArticleRequestValidator : AbstractValidator<UpdateArticleRequest>
{
    public UpdateArticleRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title cannot be empty.")
            .MaximumLength(100).WithMessage("Title may contain a maximal of 100 characters.");

        RuleFor(x => x.Summary)
            .NotEmpty().WithMessage("Summary cannot be empty.")
            .MaximumLength(500).WithMessage("Summary is to long. Use a maximal of 500 characters.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content cannot be empty.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Select a category.");
    }
}