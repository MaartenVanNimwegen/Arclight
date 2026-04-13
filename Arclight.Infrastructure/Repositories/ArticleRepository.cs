using Arclight.Application.Interfaces;
using Arclight.Domain.Entities;
using Arclight.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Arclight.Infrastructure.Repositories;

public class ArticleRepository(AppDbContext context) : IArticleRepository
{
    public async Task<IEnumerable<Article>> GetAllPublishedAsync()
    {
        return await context.Articles
            .AsNoTracking()
            .Include(a => a.Author)      // Retrieve the Author data
            .Include(a => a.Category)    // Retrieve the Category data
            .Where(a => a.IsPublished)   // Retrieve only published articles
            .OrderByDescending(a => a.PublishedAt) // Sort by published date, newest first
            .ToListAsync();
    }

    public async Task<Article?> GetBySlugAsync(string slug)
    {
        return await context.Articles
            .AsNoTracking()
            .Include(a => a.Author)
            .Include(a => a.Category)
            .FirstOrDefaultAsync(a => a.Slug == slug);
    }

    public async Task<Article?> GetByIdAsync(Guid id)
    {
        return await context.Articles
            .Include(a => a.Author)
            .Include(a => a.Category)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task AddAsync(Article article)
    {
        await context.Articles.AddAsync(article);
    }

    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }

    public async Task<bool> SlugExistsAsync(string slug)
    {
        return await context.Articles.AnyAsync(a => a.Slug == slug);
    }

    public void Delete(Article article)
    {
            context.Articles.Remove(article);
    }

    public async Task<bool> HasArticlesInCategoryAsync(Guid categoryId)
    {
        return await context.Articles.AnyAsync(a => a.CategoryId == categoryId);
    }

    public async Task<List<string>> GetExistingSlugsAsync(string baseSlug)
    {
        return await context.Articles
            .Where(a => a.Slug == baseSlug || a.Slug.StartsWith(baseSlug + "-"))
            .Select(a => a.Slug)
            .ToListAsync();
    }

    public Task<bool> ExistsAsync(Guid articleId)
    {
        return context.Articles.AnyAsync(article => article.Id == articleId);
    }
}