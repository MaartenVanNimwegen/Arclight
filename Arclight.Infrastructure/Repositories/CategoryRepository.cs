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
    }
}
