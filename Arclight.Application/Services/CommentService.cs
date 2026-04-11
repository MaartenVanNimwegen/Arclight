using Arclight.Application.Interfaces;
using Arclight.Domain.Entities;
using Arclight.Domain.Enums;

public class CommentService(
    ICommentRepository commentRepository,
    IUserRepository userRepository,
    IArticleRepository articleRepository
    ) : ICommentService
{
    public async Task<CommentResponse> AddCommentAsync(Guid articleId, Guid userId, CreateCommentRequest request)
    {
        var articleExists = await articleRepository.ExistsAsync(articleId);
        if (!articleExists) throw new ArgumentException("Article not found.");

        var user = await userRepository.GetByIdAsync(userId);
        if (user is null) throw new UnauthorizedAccessException("User not found.");

        Comment comment = new(
            request.Text,
            articleId,
            userId
        );

        await commentRepository.AddAsync(comment);
        await commentRepository.SaveChangesAsync();

        return new CommentResponse(
            comment.Id,
            comment.Text,
            user.FullName,
            comment.CreatedAt,
            comment.UserId
        );
    }

    public async Task<IEnumerable<CommentResponse>> GetCommentsByArticleIdAsync(Guid articleId)
    {
        var comments = await commentRepository.GetByArticleIdAsync(articleId);

        return comments.Select(c => new CommentResponse(
            c.Id,
            c.Text,
            c.User?.FullName ?? "Unknown",
            c.CreatedAt,
            c.UserId
        ));
    }

    public async Task<bool> DeleteCommentAsync(Guid commentId, Guid currentUserId, UserRole currentUserRole)
    {
        var comment = await commentRepository.GetByIdAsync(commentId);

        if (comment is null) return false;

        bool isOwner = comment.UserId == currentUserId;
        bool isStaff = currentUserRole == UserRole.Admin || currentUserRole == UserRole.ContentCreator;

        if (!isOwner && !isStaff)
        {
            throw new UnauthorizedAccessException("You don't have permission to delete this comment.");
        }

        commentRepository.Delete(comment);
        await commentRepository.SaveChangesAsync();
        return true;
    }
}