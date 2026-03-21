using Arclight.Domain.Entities;

namespace Arclight.Application.Interfaces;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id);
    Task<IEnumerable<Category>> GetAllAsync();
    Task<Category?> GetBySlugAsync(string slug);
    void Update(Category category);
    void Delete(Category category);
    Task<bool> SlugExistsAsync(string slug);
    Task AddAsync(Category category);
    Task SaveChangesAsync();
}
