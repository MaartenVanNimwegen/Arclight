using Arclight.Application.DTOs;
using Arclight.Application.Interfaces;
using Arclight.Application.Services;
using Arclight.Domain.Entities;
using Arclight.Domain.Enums;
using Arclight.Infrastructure.Persistence;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace Arclight.Api.IntegrationTests.Endpoints;

public class ArticleEndpointsTests : BaseIntegrationTest
{
    public ArticleEndpointsTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetAllArticles_ShouldReturnOk()
    {
        // Act
        var response = await Client.GetAsync("/articles");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetArticleBySlug_ShouldReturnNotFound_WhenArticleDoesNotExist()
    {
        // Act
        var response = await Client.GetAsync("/articles/deze-slug-bestaat-helemaal-niet");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetArticleBySlug_ShouldReturnOk_WhenArticleExists()
    {
        // Arrange
        var category = new Category("News", "news", "desc");
        var author = new User("test@arclight.nl", "Test", "Author", "hash", UserRole.ContentCreator);

        var article = new Article("Unique Title", "unique-title", "Sum", "Content", author.Id, category.Id);
        article.Publish();

        await ExecuteDbContextAsync(async (context) =>
        {
            context.Users.Add(author);
            context.Categories.Add(category);
            context.Articles.Add(article);
            await context.SaveChangesAsync();
        });

        // Act
        var response = await Client.GetAsync($"/articles/{article.Slug}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ArticleResponse>();
        result!.Title.Should().Be("Unique Title");
    }

    [Fact]
    public async Task UpdateArticle_ShouldReturnBadRequest_WhenTitleIsEmpty()
    {
        // Arrange
        var randomId = Guid.NewGuid();
        var request = new UpdateArticleRequest("", "Summary", "Content", Guid.NewGuid());

        // Act
        var response = await Client.PutAsJsonAsync($"/articles/{randomId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }


    [Fact]
    public async Task UpdateArticle_ShouldReturnNotFound_WhenArticleDoesNotExist()
    {
        // Arrange
        var randomId = Guid.NewGuid();
        var request = new UpdateArticleRequest("Nieuw", "Sum", "Content", Guid.NewGuid());

        // Act
        var response = await Client.PutAsJsonAsync($"/articles/{randomId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateArticle_ShouldReturnNoContent_WhenValid()
    {
        // Arrange
        var category = new Category("Tech", "tech", "desc");
        var author = new User("update@test.nl", "U", "P", "h", UserRole.ContentCreator);
        var article = new Article("Old Title", "old-slug", "Sum", "Content", author.Id, category.Id);

        await ExecuteDbContextAsync(async (context) =>
        {
            context.Users.Add(author);
            context.Categories.Add(category);
            context.Articles.Add(article);
            await context.SaveChangesAsync();
        });

        var request = new UpdateArticleRequest("New Title", "Summary", "Content", category.Id);

        // Act
        var response = await Client.PutAsJsonAsync($"/articles/{article.Id}", request);

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.NoContent, because: $"API returned: {content}");
    }

    [Fact]
    public async Task DeleteArticle_ShouldReturnNotFound_WhenArticleDoesNotExist()
    {
        // Arrange
        var randomId = Guid.NewGuid();

        // Act
        var response = await Client.DeleteAsync($"/articles/{randomId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteArticle_ShouldReturnNoContent_WhenSuccessful()
    {
        // Arrange
        var category = new Category("Delete", "del", "d");
        var author = new User("delete@test.nl", "D", "E", "h", UserRole.ContentCreator);
        var article = new Article("To Delete", "delete-me", "S", "C", author.Id, category.Id);

        await ExecuteDbContextAsync(async (context) =>
        {
            context.Users.Add(author);
            context.Categories.Add(category);
            context.Articles.Add(article);
            await context.SaveChangesAsync();
        });

        // Act
        var response = await Client.DeleteAsync($"/articles/{article.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var verifyResponse = await Client.GetAsync($"/articles/{article.Slug}");
        verifyResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateArticle_ShouldReturnBadRequest_WhenTitleIsEmpty()
    {
        // Arrange
        var request = new CreateArticleRequest("", "S", "C", Guid.NewGuid(), true);

        // Act
        var response = await Client.PostAsJsonAsync("/articles", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateArticle_ShouldReturnCreated_WhenDataIsValid()
    {
        // Arrange
        var testUserId = Guid.Parse(TestAuthHandler.TestUserId);

        var contentCreatorClient = CreateClientWithRoles("ContentCreator");

        var category = new Category("Test Tech", "test-tech", "Description");

        var author = new User(testUserId, "admin@arclight.nl", "Admin", "User", "hash", UserRole.Admin, UserStatus.Active);

        await ExecuteDbContextAsync(async (context) =>
        {
            context.Categories.Add(category);

            if (await context.Users.FindAsync(testUserId) == null)
            {
                context.Users.Add(author);
            }
            await context.SaveChangesAsync();
        });

        var request = new CreateArticleRequest(
            Title: "Mijn Geweldige Integratietest",
            Summary: "Korte samenvatting",
            Content: "Volledige tekst",
            CategoryId: category.Id,
            PublishNow: true
        );

        // Act
        var response = await contentCreatorClient.PostAsJsonAsync("/articles", request);

        // Assert
        if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.Unauthorized)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Auth error: {response.StatusCode}. Details: {error}. Check if policy 'RequireContentManager' accepts 'ContentManager' or 'Admin'.");
        }

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateArticle_ShouldReturnBadRequest_WhenCategoryDoesNotExist()
    {
        // Arrange
        var request = new CreateArticleRequest("Title", "Summary", "Content", Guid.NewGuid(), true);

        // Act
        var response = await Client.PostAsJsonAsync("/articles", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadAsStringAsync();
        error.Should().Contain("The selected category does not exist");
    }

    [Fact]
    public async Task PublishArticle_ShouldReturnNotFound_WhenArticleDoesNotExist()
    {
        // Arrange
        var randomId = Guid.NewGuid();
        // Act
        var response = await Client.PatchAsync($"/articles/{randomId}/publish", null);
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    [Fact]
    public async Task PublishArticle_ShouldReturnNoContent_AndMakeArticlePublic_WhenDraftExists()
    {
        // Arrange
        var category = new Category("Draft", "draft", "desc");
        var author = new User("draft@test.nl", "Draft", "Author", "h", UserRole.ContentCreator);
        var article = new Article("Draft Title", "draft-title", "Sum", "Content", author.Id, category.Id);
        await ExecuteDbContextAsync(async (context) =>
        {
            context.Users.Add(author);
            context.Categories.Add(category);
            context.Articles.Add(article);
            await context.SaveChangesAsync();
        });
        var beforePublish = await Client.GetAsync($"/articles/{article.Slug}");
        beforePublish.StatusCode.Should().Be(HttpStatusCode.NotFound);
        // Act
        var publishResponse = await Client.PatchAsync($"/articles/{article.Id}/publish", null);
        // Assert
        publishResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var afterPublish = await Client.GetAsync($"/articles/{article.Slug}");
        afterPublish.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetDrafts_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        var anonymousClient = CreateAnonymousClient();

        // Act
        var response = await anonymousClient.GetAsync("/articles/drafts");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetDrafts_ShouldReturnOk_WithOwnDrafts_WhenContentCreator()
    {
        // Arrange
        var testUserId = Guid.Parse(TestAuthHandler.TestUserId);
        var category = new Category("CC Drafts Cat", "cc-drafts-cat", "desc");
        var author = new User(testUserId, "cc-drafts@test.nl", "CC", "Drafts", "h", UserRole.ContentCreator, UserStatus.Active);
        var ownDraft = new Article("CC Own Draft", "cc-own-draft", "sum", "content", testUserId, category.Id);

        await ExecuteDbContextAsync(async (context) =>
        {
            context.Categories.Add(category);
            if (await context.Users.FindAsync(testUserId) == null)
                context.Users.Add(author);
            context.Articles.Add(ownDraft);
            await context.SaveChangesAsync();
        });

        // Act
        var response = await Client.GetAsync("/articles/drafts");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var articles = await response.Content.ReadFromJsonAsync<IEnumerable<ArticleResponse>>();
        articles.Should().Contain(a => a.Title == "CC Own Draft");
    }

    [Fact]
    public async Task GetDrafts_ShouldNotIncludeOtherUsersDrafts_WhenContentCreator()
    {
        // Arrange
        var testUserId = Guid.Parse(TestAuthHandler.TestUserId);
        var otherUserId = Guid.NewGuid();
        var category = new Category("Filter Drafts Cat", "filter-drafts-cat", "desc");
        var otherAuthor = new User(otherUserId, "other-drafts@test.nl", "Other", "Author", "h", UserRole.ContentCreator, UserStatus.Active);
        var ownDraft = new Article("Filter Own Draft", "filter-own-draft", "sum", "content", testUserId, category.Id);
        var otherDraft = new Article("Filter Other Draft", "filter-other-draft", "sum", "content", otherUserId, category.Id);

        await ExecuteDbContextAsync(async (context) =>
        {
            context.Categories.Add(category);
            if (await context.Users.FindAsync(testUserId) == null)
            {
                var testAuthor = new User(testUserId, "filter-cc@test.nl", "Filter", "CC", "h", UserRole.ContentCreator, UserStatus.Active);
                context.Users.Add(testAuthor);
            }
            context.Users.Add(otherAuthor);
            context.Articles.Add(ownDraft);
            context.Articles.Add(otherDraft);
            await context.SaveChangesAsync();
        });

        // Act
        var response = await Client.GetAsync("/articles/drafts");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var articles = await response.Content.ReadFromJsonAsync<IEnumerable<ArticleResponse>>();
        articles.Should().Contain(a => a.Title == "Filter Own Draft");
        articles.Should().NotContain(a => a.Title == "Filter Other Draft");
    }

    [Fact]
    public async Task GetDrafts_ShouldReturnAllDrafts_WhenAdmin()
    {
        // Arrange
        var testUserId = Guid.Parse(TestAuthHandler.TestUserId);
        var otherUserId = Guid.NewGuid();
        var category = new Category("Admin Drafts Cat", "admin-drafts-cat", "desc");
        var otherAuthor = new User(otherUserId, "admin-other@test.nl", "Other", "Admin", "h", UserRole.ContentCreator, UserStatus.Active);
        var testUserDraft = new Article("Admin Draft Own", "admin-draft-own", "sum", "content", testUserId, category.Id);
        var otherUserDraft = new Article("Admin Draft Other", "admin-draft-other", "sum", "content", otherUserId, category.Id);

        await ExecuteDbContextAsync(async (context) =>
        {
            context.Categories.Add(category);
            if (await context.Users.FindAsync(testUserId) == null)
            {
                var testAuthor = new User(testUserId, "admin-test@test.nl", "Admin", "Test", "h", UserRole.Admin, UserStatus.Active);
                context.Users.Add(testAuthor);
            }
            context.Users.Add(otherAuthor);
            context.Articles.Add(testUserDraft);
            context.Articles.Add(otherUserDraft);
            await context.SaveChangesAsync();
        });

        var adminClient = CreateClientWithRoles("Admin");

        // Act
        var response = await adminClient.GetAsync("/articles/drafts");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var articles = await response.Content.ReadFromJsonAsync<IEnumerable<ArticleResponse>>();
        articles.Should().Contain(a => a.Title == "Admin Draft Own");
        articles.Should().Contain(a => a.Title == "Admin Draft Other");
    }

    [Fact]
    public async Task GetDrafts_ShouldNotReturnPublishedArticles()
    {
        // Arrange
        var testUserId = Guid.Parse(TestAuthHandler.TestUserId);
        var category = new Category("Published Check Cat", "published-check-cat", "desc");
        var draft = new Article("Drafts Only Draft", "drafts-only-draft", "sum", "content", testUserId, category.Id);
        var published = new Article("Drafts Only Published", "drafts-only-published", "sum", "content", testUserId, category.Id);
        published.Publish();

        await ExecuteDbContextAsync(async (context) =>
        {
            context.Categories.Add(category);
            if (await context.Users.FindAsync(testUserId) == null)
            {
                var testAuthor = new User(testUserId, "published-check@test.nl", "Published", "Check", "h", UserRole.ContentCreator, UserStatus.Active);
                context.Users.Add(testAuthor);
            }
            context.Articles.Add(draft);
            context.Articles.Add(published);
            await context.SaveChangesAsync();
        });

        // Act
        var response = await Client.GetAsync("/articles/drafts");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var articles = await response.Content.ReadFromJsonAsync<IEnumerable<ArticleResponse>>();
        articles.Should().Contain(a => a.Title == "Drafts Only Draft");
        articles.Should().NotContain(a => a.Title == "Drafts Only Published");
    }

    [Fact]
    public async Task GetAllArticles_ShouldReturnServiceUnavailable_WhenRateLimitExceeded()
    {
        // Arrange
        int requestCount = 15;
        var tasks = new List<Task<HttpResponseMessage>>();

        // Act
        for (int i = 0; i < requestCount; i++)
        {
            tasks.Add(Client.GetAsync("/articles"));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert
        var hasRateLimitResponse = responses.Any(r =>
            r.StatusCode == HttpStatusCode.ServiceUnavailable ||
            r.StatusCode == HttpStatusCode.TooManyRequests);

        hasRateLimitResponse.Should().BeTrue(because: "we hebben de limiet van 10 requests per 10 seconden ruimschoots overschreden");
    }
}