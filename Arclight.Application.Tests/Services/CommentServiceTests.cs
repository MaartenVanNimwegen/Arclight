using Arclight.Application.DTOs;
using Arclight.Application.Interfaces;
using Arclight.Application.Services;
using Arclight.Domain.Entities;
using Arclight.Domain.Enums;
using FluentAssertions;
using Moq;

namespace Arclight.Application.Tests.Services;

public class CommentServiceTests
{
    private readonly Mock<ICommentRepository> _commentRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IArticleRepository> _articleRepoMock;
    private readonly CommentService _sut;

    public CommentServiceTests()
    {
        _commentRepoMock = new Mock<ICommentRepository>();
        _userRepoMock = new Mock<IUserRepository>();
        _articleRepoMock = new Mock<IArticleRepository>();

        _sut = new CommentService(
            _commentRepoMock.Object,
            _userRepoMock.Object,
            _articleRepoMock.Object
        );
    }


    [Fact]
    public async Task AddCommentAsync_ShouldReturnResponse_WhenValidRequest()
    {
        // Arrange
        var articleId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new CreateCommentRequest("Test commentaar");
        var user = new User("test@test.nl", "Jan", "Jansen", "hash", UserRole.User);

        _articleRepoMock.Setup(repo => repo.ExistsAsync(articleId))
                        .ReturnsAsync(true);

        _userRepoMock.Setup(repo => repo.GetByIdAsync(userId))
                     .ReturnsAsync(user);

        // Act
        var result = await _sut.AddCommentAsync(articleId, userId, request);

        // Assert
        result.Should().NotBeNull();
        result.Text.Should().Be(request.Text);
        result.AuthorName.Should().Be("Jan Jansen");

        _commentRepoMock.Verify(repo => repo.AddAsync(It.Is<Comment>(c =>
            c.Text == request.Text &&
            c.ArticleId == articleId &&
            c.UserId == userId
        )), Times.Once);

        _commentRepoMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AddCommentAsync_ShouldThrowArgumentException_WhenArticleDoesNotExist()
    {
        // Arrange
        var articleId = Guid.NewGuid();
        _articleRepoMock.Setup(repo => repo.ExistsAsync(articleId))
                        .ReturnsAsync(false);

        // Act
        var act = () => _sut.AddCommentAsync(articleId, Guid.NewGuid(), new CreateCommentRequest("X"));

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Article not found.");

        _commentRepoMock.Verify(repo => repo.AddAsync(It.IsAny<Comment>()), Times.Never);
    }

    [Fact]
    public async Task AddCommentAsync_ShouldThrowUnauthorizedAccessException_WhenUserDoesNotExist()
    {
        // Arrange
        var articleId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _articleRepoMock.Setup(repo => repo.ExistsAsync(articleId))
                        .ReturnsAsync(true);

        _userRepoMock.Setup(repo => repo.GetByIdAsync(userId))
                     .ReturnsAsync((User?)null);

        // Act
        var act = () => _sut.AddCommentAsync(articleId, userId, new CreateCommentRequest("X"));

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("User not found.");
    }


    [Fact]
    public async Task GetCommentsByArticleIdAsync_ShouldReturnUnknown_WhenUserNavigationPropertyIsNull()
    {
        // Arrange
        var articleId = Guid.NewGuid();

        var comments = new List<Comment>
    {
        new Comment("Test tekst", articleId, Guid.NewGuid())
    };

        _commentRepoMock.Setup(repo => repo.GetByArticleIdAsync(articleId))
                        .ReturnsAsync(comments);

        // Act
        var result = await _sut.GetCommentsByArticleIdAsync(articleId);

        // Assert
        var resultList = result.ToList();
        resultList.Should().HaveCount(1);

        resultList[0].AuthorName.Should().Be("Unknown");
    }


    [Fact]
    public async Task DeleteCommentAsync_ShouldReturnTrue_WhenUserIsOwner()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var articleId = Guid.NewGuid();
        var comment = new Comment("X", articleId, userId);

        _commentRepoMock.Setup(repo => repo.GetByIdAsync(commentId))
                        .ReturnsAsync(comment);

        // Act
        var result = await _sut.DeleteCommentAsync(articleId, commentId, userId, UserRole.User);

        // Assert
        result.Should().BeTrue();
        _commentRepoMock.Verify(repo => repo.Delete(comment), Times.Once);
        _commentRepoMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteCommentAsync_ShouldReturnTrue_WhenUserIsAdminButNotOwner()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var articleId = Guid.NewGuid();
        var comment = new Comment("X", articleId, ownerId);

        _commentRepoMock.Setup(repo => repo.GetByIdAsync(commentId))
                        .ReturnsAsync(comment);

        // Act
        var result = await _sut.DeleteCommentAsync(articleId, commentId, adminId, UserRole.Admin);

        // Assert
        result.Should().BeTrue();
        _commentRepoMock.Verify(repo => repo.Delete(comment), Times.Once);
    }

    [Fact]
    public async Task DeleteCommentAsync_ShouldReturnFalse_WhenCommentDoesNotExist()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        _commentRepoMock.Setup(repo => repo.GetByIdAsync(commentId))
                        .ReturnsAsync((Comment?)null);

        // Act
        var result = await _sut.DeleteCommentAsync(Guid.NewGuid(), commentId, Guid.NewGuid(), UserRole.Admin);

        // Assert
        result.Should().BeFalse();
        _commentRepoMock.Verify(repo => repo.Delete(It.IsAny<Comment>()), Times.Never);
    }

    [Fact]
    public async Task DeleteCommentAsync_ShouldReturnFalse_WhenCommentDoesNotBelongToArticle()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var articleId = Guid.NewGuid();
        var differentArticleId = Guid.NewGuid();
        var comment = new Comment("X", articleId, Guid.NewGuid());

        _commentRepoMock.Setup(repo => repo.GetByIdAsync(commentId))
                        .ReturnsAsync(comment);

        // Act
        var result = await _sut.DeleteCommentAsync(differentArticleId, commentId, Guid.NewGuid(), UserRole.Admin);

        // Assert
        result.Should().BeFalse();
        _commentRepoMock.Verify(repo => repo.Delete(It.IsAny<Comment>()), Times.Never);
        _commentRepoMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteCommentAsync_ShouldThrowUnauthorizedAccessException_WhenNotOwnerAndNotStaff()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var articleId = Guid.NewGuid();
        var comment = new Comment("X", articleId, ownerId);

        _commentRepoMock.Setup(repo => repo.GetByIdAsync(commentId))
                        .ReturnsAsync(comment);

        // Act
        var act = () => _sut.DeleteCommentAsync(articleId, commentId, otherUserId, UserRole.User);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("You don't have permission to delete this comment.");

        _commentRepoMock.Verify(repo => repo.Delete(It.IsAny<Comment>()), Times.Never);
    }
}
