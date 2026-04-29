using Arclight.Domain.Entities;

namespace Arclight.Application.Interfaces
{
    public interface ICommentRepository
    {
        Task<IEnumerable<Comment>> GetByArticleIdAsync(Guid articleId);
        Task<Comment?> GetByIdAsync(Guid commentId);
        Task AddAsync(Comment comment);
        void Delete(Comment comment);
        Task SaveChangesAsync();
        Task<IEnumerable<Comment>> GetByUserIdAsync(Guid userId);
    }
}
