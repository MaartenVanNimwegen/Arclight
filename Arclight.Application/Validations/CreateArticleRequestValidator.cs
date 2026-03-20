using Arclight.Application.DTOs;
using Arclight.Application.Interfaces;
using FluentValidation;

namespace Arclight.Application.Validations;

public class CreateArticleRequestValidator : AbstractValidator<CreateArticleRequest>
{
    private readonly ICategoryRepository _categoryRepository;

    public CreateArticleRequestValidator(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title cannot be empty.")
            .MaximumLength(100).WithMessage("Title may contain a maximum of 100 characters.");

        RuleFor(x => x.Summary)
            .NotEmpty().WithMessage("Summary cannot be empty.")
            .MaximumLength(500).WithMessage("Summary is too long. Use a maximum of 500 characters.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content cannot be empty.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category is required.")
            .MustAsync(CategoryMustExist).WithMessage("The selected category does not exist.");

    }

    private async Task<bool> CategoryMustExist(Guid categoryId)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId);
        return category is not null;
    }
}