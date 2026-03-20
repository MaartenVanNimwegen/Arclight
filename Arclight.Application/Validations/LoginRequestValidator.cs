using Arclight.Application.DTOs;
using FluentValidation;

namespace Arclight.Application.Validations
{
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator() 
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email cannot be empty.")
                .MaximumLength(100).WithMessage("Email may contain a maximum of 100 characters.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password cannot be empty.")
                .MaximumLength(100).WithMessage("Password may contain a maximum of 100 characters.");
        }
    }
}
