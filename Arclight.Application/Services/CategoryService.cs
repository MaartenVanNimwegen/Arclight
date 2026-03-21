using Arclight.Application.DTOs;
using Arclight.Application.Interfaces;
using Arclight.Domain.Entities;

namespace Arclight.Application.Services
{
    public class CategoryService(ICategoryRepository repository, IArticleRepository articleRepository, ISlugService slugService) : ICategoryService
    {
        public async Task<IEnumerable<CategoryResponse>> GetAllCategoriesAsync()
        {
            var categories = await repository.GetAllAsync();
            return categories.Select(c => new CategoryResponse(c.Id, c.Name, c.Slug, c.Description));
        }

        public async Task<Guid> CreateCategoryAsync(CreateCategoryRequest request)
        {
            string slug = await slugService.GenerateUniqueCategorySlugAsync(request.Name);

            var category = new Category(request.Name, slug, request.Description);

            await repository.AddAsync(category);
            await repository.SaveChangesAsync();

            return category.Id;
        }

        public async Task<bool> DeleteCategoryAsync(Guid id)
        {
            var category = await repository.GetByIdAsync(id);
            if (category is null) return false;

            bool hasArticles = await articleRepository.HasArticlesInCategoryAsync(id);

            if (hasArticles)
            {
                throw new InvalidOperationException("Cannot delete category because it still contains articles.");
            }

            repository.Delete(category);
            await repository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request)
        {
            var category = await repository.GetByIdAsync(id);
            if (category is null) return false;

            category.Update(request.Name, request.Description);

            repository.Update(category);
            await repository.SaveChangesAsync();
            return true;
        }
    }
}
