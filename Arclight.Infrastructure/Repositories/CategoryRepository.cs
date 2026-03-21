using Arclight.Application.Interfaces;
using Arclight.Domain.Entities;
using Arclight.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Arclight.Infrastructure.Repositories
{
    public class CategoryRepository(AppDbContext context) : ICategoryRepository
    {
        public async Task<Category?> GetByIdAsync(Guid id)
        {
            return await context.Categories
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await context.Categories.OrderBy(c => c.Name).ToListAsync();
        }

        public async Task<Category?> GetBySlugAsync(string slug)
        {
            return await context.Categories.FirstOrDefaultAsync(c => c.Slug == slug);
        }

        public void Update(Category category)
        {
            context.Categories.Update(category);
        }

        public void Delete(Category category) 
        { 
            context.Categories.Remove(category); 
        }

        public async Task<bool> SlugExistsAsync(string slug)
        { 
            return await context.Categories.AnyAsync(c => c.Slug == slug);
        }

        public async Task AddAsync(Category category)
        {
            await context.Categories.AddAsync(category);
        }

        public async Task SaveChangesAsync()
        {
            await context.SaveChangesAsync();
        }

        public async Task<List<string>> GetExistingSlugsAsync(string baseSlug)
        {
            return await context.Categories
                .Where(a => a.Slug == baseSlug || a.Slug.StartsWith(baseSlug + "-"))
                .Select(a => a.Slug)
                .ToListAsync();
        }
    }
}
