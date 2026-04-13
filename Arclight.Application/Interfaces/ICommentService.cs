using Arclight.Application.DTOs;
using Arclight.Domain.Enums;

namespace Arclight.Application.Interfaces
{
    public interface ICommentService
    {
        Task<bool> DeleteCommentAsync(Guid articleId, Guid commentId, Guid currentUserId, UserRole currentUserRole);
        Task<IEnumerable<CommentResponse>> GetCommentsByArticleIdAsync(Guid articleId);
        Task<CommentResponse> AddCommentAsync(Guid articleId, Guid userId, CreateCommentRequest request);
    }
}
