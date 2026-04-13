using Arclight.Application.DTOs;
using Arclight.Domain.Entities;
using Arclight.Domain.Enums;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Arclight.Api.IntegrationTests.Endpoints;

public class CommentEndpointsTests(CustomWebApplicationFactory factory) : BaseIntegrationTest(factory)
{
    private readonly Guid _testUserId = Guid.Parse(TestAuthHandler.TestUserId);

    [Fact]
    public async Task CreateComment_ShouldReturnCreated_WhenUserIsLoggedInAndArticleExists()
    {
        // Arrange
        var articleId = await SeedArticleAsync();
        var client = CreateClientWithRoles("Reader");
        var request = new CreateCommentRequest("Test");

        // Act
        var response = await client.PostAsJsonAsync($"/articles/{articleId}/comments", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateComment_ShouldReturnBadRequest_WhenArticleDoesNotExist()
    {
        // Arrange
        var client = CreateClientWithRoles("Reader");
        var request = new CreateCommentRequest("Test");

        // Act
        var response = await client.PostAsJsonAsync($"/articles/{Guid.NewGuid()}/comments", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetComments_ShouldReturnOk_WithCommentsList()
    {
        // Arrange
        var articleId = await SeedArticleAsync();
        await ExecuteDbContextAsync(async (context) =>
        {
            context.Comments.Add(new Comment("C1", articleId, _testUserId));
            await context.SaveChangesAsync();
        });

        // Act
        var response = await Client.GetAsync($"/articles/{articleId}/comments");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteComment_ShouldReturnNoContent_WhenUserIsOwner()
    {
        // Arrange
        var articleId = await SeedArticleAsync();
        var commentId = Guid.NewGuid();

        await ExecuteDbContextAsync(async (context) =>
        {
            var comment = new Comment("Mijn comment", articleId, _testUserId);
            typeof(Comment).GetProperty("Id")?.SetValue(comment, commentId);
            context.Comments.Add(comment);
            await context.SaveChangesAsync();
        });

        var client = CreateClientWithRoles("Reader");

        // Act
        var response = await client.DeleteAsync($"/articles/{articleId}/comments/{commentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteComment_ShouldReturnForbidden_WhenUserIsNotOwnerOrStaff()
    {
        var articleId = await SeedArticleAsync();
        var otherUserId = Guid.NewGuid();
        var commentId = Guid.NewGuid();

        await ExecuteDbContextAsync(async (context) =>
        {
            var otherUser = new User("other@test.nl", "Other", "User", "hash", UserRole.User);
            typeof(User).GetProperty("Id")?.SetValue(otherUser, otherUserId);
            context.Users.Add(otherUser);

            var comment = new Comment("Niet van jou", articleId, otherUserId);
            typeof(Comment).GetProperty("Id")?.SetValue(comment, commentId);
            context.Comments.Add(comment);

            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
        });

        var client = CreateClientWithRoles("Reader");

        // Act
        var response = await client.DeleteAsync($"/articles/{articleId}/comments/{commentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteComment_ShouldReturnNotFound_WhenCommentDoesNotExist()
    {
        // Arrange
        var articleId = await SeedArticleAsync();
        var client = CreateClientWithRoles("Admin");

        // Act
        var response = await client.DeleteAsync($"/articles/{articleId}/comments/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteComment_ShouldReturnNotFound_WhenCommentBelongsToDifferentArticle()
    {
        // Arrange
        var articleId = await SeedArticleAsync();
        var otherArticleId = Guid.NewGuid();
        var commentId = Guid.NewGuid();

        await ExecuteDbContextAsync(async (context) =>
        {
            var comment = new Comment("Mijn comment", articleId, _testUserId);
            typeof(Comment).GetProperty("Id")?.SetValue(comment, commentId);
            context.Comments.Add(comment);
            await context.SaveChangesAsync();
        });

        var client = CreateClientWithRoles("Admin");

        // Act
        var response = await client.DeleteAsync($"/articles/{otherArticleId}/comments/{commentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateComment_ShouldReturnBadRequest_WhenTextIsEmpty()
    {
        // Arrange
        var articleId = await SeedArticleAsync();
        var client = CreateClientWithRoles("Reader");

        // Act
        var response = await client.PostAsJsonAsync($"/articles/{articleId}/comments", new CreateCommentRequest(""));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateComment_ShouldReturnUnauthorized_WhenUserIsNotLoggedIn()
    {
        // Arrange
        var client = CreateAnonymousClient();

        // Act
        var response = await client.PostAsJsonAsync($"/articles/{Guid.NewGuid()}/comments", new CreateCommentRequest("X"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task<Guid> SeedArticleAsync()
    {
        var testUserId = Guid.Parse(TestAuthHandler.TestUserId);
        var category = new Category("Test", "test", "test");

        var author = new User("tester@arclight.nl", "Test", "User", "hash", UserRole.User);
        typeof(User).GetProperty("Id")?.SetValue(author, testUserId);

        var articleId = Guid.NewGuid();
        var article = new Article("Titel", "slug", "Sum", "Cont", testUserId, category.Id);
        typeof(Article).GetProperty("Id")?.SetValue(article, articleId);

        await ExecuteDbContextAsync(async (context) =>
        {
            if (await context.Users.FindAsync(testUserId) == null)
            {
                context.Users.Add(author);
            }

            if (!context.Categories.Any(c => c.Slug == "test"))
            {
                context.Categories.Add(category);
            }

            await context.SaveChangesAsync();

            if (await context.Articles.FindAsync(articleId) == null)
            {
                context.Articles.Add(article);
            }

            await context.SaveChangesAsync();
        });

        return articleId;
    }
}
