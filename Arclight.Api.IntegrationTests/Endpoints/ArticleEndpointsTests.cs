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
        var category = new Category("Test Tech", "test-tech", "Description");

        await ExecuteDbContextAsync(async (context) =>
        {
            context.Categories.Add(category);
            await context.SaveChangesAsync();
        });

        var request = new CreateArticleRequest(
            Title: "Mijn Geweldige Integratietest",
            Summary: "Korte samenvatting",
            Content: "Volledige tekst",
            CategoryId: category.Id,
            true
        );

        // Act
        var response = await Client.PostAsJsonAsync("/articles", request);

        // Assert
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Crash details: {error}");
        }

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain("/articles/");
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
}