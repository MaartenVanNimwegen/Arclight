using Arclight.Application.DTOs;
using Arclight.Application.Interfaces;
using Arclight.Domain.Entities;

namespace Arclight.Application.Services
{
    public class CategoryService(ICategoryRepository repository, ISlugService slugService) : ICategoryService
    {
        public async Task<IEnumerable<CategoryResponse>> GetAllCategoriesAsync()
        {
            var categories = await repository.GetAllAsync();
            return categories.Select(c => new CategoryResponse(c.Id, c.Name, c.Slug, c.Description));
        }

        public async Task<Guid> CreateCategoryAsync(CreateCategoryRequest request)
        {
            // Check of de naam al bestaat om dubbele slugs te voorkomen
            string slug = await slugService.GenerateUniqueSlugAsync(request.Name);

            var category = new Category(request.Name, slug, request.Description);

            await repository.AddAsync(category);
            await repository.SaveChangesAsync();

            return category.Id;
        }

        public async Task<bool> DeleteCategoryAsync(Guid id)
        {
            var category = await repository.GetByIdAsync(id);
            if (category is null) return false;

            // TODO: Check hier later of er nog artikelen in deze categorie zitten!
            repository.Delete(category);
            await repository.SaveChangesAsync();
            return true;
        }
    }
}
