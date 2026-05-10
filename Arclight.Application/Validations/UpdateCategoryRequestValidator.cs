using Arclight.Application.DTOs;
using FluentValidation;

namespace Arclight.Application.Validations;

public class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(50).WithMessage("Name is too long.");

        RuleFor(x => x.Description)
            .MaximumLength(250).WithMessage("Description is too long.");
    }
}