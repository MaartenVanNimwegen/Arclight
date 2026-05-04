using Arclight.Application.Interfaces;
using Arclight.Domain.Entities;
using Arclight.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Arclight.Infrastructure.Repositories;

public class CommentRepository(AppDbContext context) : ICommentRepository
{
    public async Task<Comment?> GetByIdAsync(Guid commentId)
    {
        return await context.Comments
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == commentId);
    }

    public async Task<IEnumerable<Comment>> GetByArticleIdAsync(Guid articleId)
    {
        return await context.Comments
            .Include(c => c.User)
            .Where(c => c.ArticleId == articleId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(Comment comment)
    {
        await context.Comments.AddAsync(comment);
    }

    public void Delete(Comment comment)
    {
        context.Comments.Remove(comment);
    }

    public async Task SaveChangesAsync()
    {
       await context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Comment>> GetByUserIdAsync(Guid userId)
    {
        return await context.Comments
            .Include(c => c.User)
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Comment>> GetAllAsync()
    {
        return await context.Comments
            .Include(c => c.User)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }
}