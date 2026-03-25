using Arclight.Application.DTOs;
using Arclight.Application.Interfaces;
using Arclight.Domain.Entities;
using Arclight.Domain.Enums;

namespace Arclight.Application.Services;

public class CommentService(ICommentRepository commentRepository) : ICommentService
{
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

    public async Task<IEnumerable<CommentDto>> GetCommentsByArticleIdAsync(Guid articleId)
    {
        var comments = await commentRepository.GetByArticleIdAsync(articleId);
        return comments.Select(c => new CommentResponse
        {
            Id = c.Id,
            Text = c.Text,
            AuthorName = c.User?.FullName ?? "Unknown",
            CreatedAt = c.CreatedAt
        });
    }
}