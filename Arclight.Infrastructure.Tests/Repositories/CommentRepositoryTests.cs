using Arclight.Domain.Entities;
using Arclight.Domain.Enums;
using Arclight.Infrastructure.Persistence;
using Arclight.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Arclight.Infrastructure.Tests.Repositories;

public class CommentRepositoryTests
{
    private AppDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnComment_WithUserIncluded()
    {
        // Arrange
        var context = GetDbContext();
        var repo = new CommentRepository(context);

        var user = new User("test@test.nl", "Jan", "Jansen", "hash", UserRole.User);
        var commentId = Guid.NewGuid();
        var comment = new Comment("Test tekst", Guid.NewGuid(), user.Id);
        typeof(Comment).GetProperty("Id")?.SetValue(comment, commentId);

        context.Users.Add(user);
        context.Comments.Add(comment);
        await context.SaveChangesAsync();

        // Act
        var result = await repo.GetByIdAsync(commentId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(commentId);
        result.User.Should().NotBeNull();
        result.User!.FullName.Should().Be("Jan Jansen");
    }

    [Fact]
    public async Task GetByArticleIdAsync_ShouldReturnComments_SortedByNewestFirst()
    {
        // Arrange
        var context = GetDbContext();
        var repo = new CommentRepository(context);

        var user = new User("t@t.nl", "J", "D", "h", UserRole.User);
        var category = new Category("T", "t", "d");
        var article = new Article("T", "s", "s", "c", user.Id, category.Id);

        context.Users.Add(user);
        context.Categories.Add(category);
        context.Articles.Add(article);
        await context.SaveChangesAsync();

        var oldComment = new Comment("Oud", article.Id, user.Id);
        var newComment = new Comment("Nieuw", article.Id, user.Id);

        context.Comments.AddRange(oldComment, newComment);
        await context.SaveChangesAsync();

        var oldDate = DateTimeOffset.UtcNow.AddHours(-1);
        var newDate = DateTimeOffset.UtcNow;

        typeof(Comment).GetProperty("CreatedAt")?.SetValue(oldComment, oldDate);
        typeof(Comment).GetProperty("CreatedAt")?.SetValue(newComment, newDate);

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        var result = (await repo.GetByArticleIdAsync(article.Id)).ToList();

        // Assert
        result.Should().NotBeEmpty("er zouden comments in de database moeten zitten");
        result.Should().HaveCount(2);
        result.First().Text.Should().Be("Nieuw");
        result.Last().Text.Should().Be("Oud");
    }

    [Fact]
    public async Task AddAsync_ShouldAddCommentToContext()
    {
        // Arrange
        var context = GetDbContext();
        var repo = new CommentRepository(context);
        var comment = new Comment("Nieuw commentaar", Guid.NewGuid(), Guid.NewGuid());

        // Act
        await repo.AddAsync(comment);
        await repo.SaveChangesAsync();

        // Assert
        context.Comments.Any(c => c.Text == "Nieuw commentaar").Should().BeTrue();
    }

    [Fact]
    public async Task Delete_ShouldRemoveCommentFromContext()
    {
        // Arrange
        var context = GetDbContext();
        var repo = new CommentRepository(context);
        var comment = new Comment("Te verwijderen", Guid.NewGuid(), Guid.NewGuid());
        context.Comments.Add(comment);
        await context.SaveChangesAsync();

        // Act
        repo.Delete(comment);
        await repo.SaveChangesAsync();

        // Assert
        context.Comments.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldPersistChanges()
    {
        // Arrange
        var context = GetDbContext();
        var repo = new CommentRepository(context);
        var comment = new Comment("Initial text", Guid.NewGuid(), Guid.NewGuid());
        context.Comments.Add(comment);
        await context.SaveChangesAsync();

        // Act
        comment.UpdateText("Updated text");
        await repo.SaveChangesAsync();

        // Assert
        var dbComment = await context.Comments.FirstAsync();
        dbComment.Text.Should().Be("Updated text");
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnComments_FilteredByUser_SortedByNewestFirst_AndIncludeUser()
    {
        // Arrange
        var context = GetDbContext();
        var repo = new CommentRepository(context);

        var userA = new User("usera@test.nl", "Alice", "A", "hash", UserRole.User);
        var userB = new User("userb@test.nl", "Bob", "B", "hash", UserRole.User);
        var category = new Category("Test", "test", "desc");
        var article = new Article("Titel", "slug", "sum", "content", userA.Id, category.Id);

        context.Users.AddRange(userA, userB);
        context.Categories.Add(category);
        context.Articles.Add(article);
        await context.SaveChangesAsync();

        var oldCommentUserA = new Comment("Oud A", article.Id, userA.Id);
        var newCommentUserA = new Comment("Nieuw A", article.Id, userA.Id);
        var commentUserB = new Comment("Reactie B", article.Id, userB.Id);

        context.Comments.AddRange(oldCommentUserA, newCommentUserA, commentUserB);
        await context.SaveChangesAsync();

        var oldDate = DateTimeOffset.UtcNow.AddHours(-1);
        var newDate = DateTimeOffset.UtcNow;

        typeof(Comment).GetProperty("CreatedAt")?.SetValue(oldCommentUserA, (DateTimeOffset?)oldDate);
        typeof(Comment).GetProperty("CreatedAt")?.SetValue(newCommentUserA, (DateTimeOffset?)newDate);

        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();

        // Act
        var result = (await repo.GetByUserIdAsync(userA.Id)).ToList();

        // Assert
        result.Should().HaveCount(2, "omdat we filteren op User A en User B moeten negeren");

        result.First().Text.Should().Be("Nieuw A");
        result.Last().Text.Should().Be("Oud A");

        result.First().User.Should().NotBeNull();
        result.First().User!.FullName.Should().Be("Alice A");
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnEmptyList_WhenUserHasNoComments()
    {
        // Arrange
        var context = GetDbContext();
        var repo = new CommentRepository(context);
        var userId = Guid.NewGuid(); 

        // Act
        var result = await repo.GetByUserIdAsync(userId);

        // Assert
        result.Should().BeEmpty();
    }
}