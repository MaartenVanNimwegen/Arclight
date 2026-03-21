using Arclight.Application.DTOs;

namespace Arclight.Application.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryResponse>> GetAllCategoriesAsync();
        Task<Guid> CreateCategoryAsync(CreateCategoryRequest request);
        Task<bool> DeleteCategoryAsync(Guid id);
        Task<bool> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request);
    }
}