using Arclight.Domain.Entities;

namespace Arclight.Application.Interfaces;

public interface IArticleRepository
{
    Task<IEnumerable<Article>> GetAllPublishedAsync();
    Task<Article?> GetBySlugAsync(string slug);
    Task<Article?> GetByIdAsync(Guid id);
    Task AddAsync(Article article);
    Task SaveChangesAsync();
    Task<bool> SlugExistsAsync(string slug);
    void Delete(Article article);
    Task<bool> HasArticlesInCategoryAsync(Guid categoryId);
    Task<List<string>> GetExistingSlugsAsync(string baseSlug);
    Task<bool> ExistsAsync(Guid articleId);
}