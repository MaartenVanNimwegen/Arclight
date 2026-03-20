using Arclight.Application.DTOs;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace Arclight.Api.IntegrationTests.Endpoints;

public class ArticleEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ArticleEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAllArticles_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/articles");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetArticleBySlug_ShouldReturnNotFound_WhenArticleDoesNotExist()
    {
        // Act
        var response = await _client.GetAsync("/articles/deze-slug-bestaat-helemaal-niet");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateArticle_ShouldReturnCreated_WhenDataIsValid()
    {
        // Arrange
        var request = new CreateArticleRequest(
            Title: "Mijn Geweldige Integratietest",
            Summary: "Korte samenvatting",
            Content: "Volledige tekst",
            CategoryId: Guid.NewGuid(),
            true
        );

        // Act
        var response = await _client.PostAsJsonAsync("/articles", request);

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
    public async Task UpdateArticle_ShouldReturnNotFound_WhenArticleDoesNotExist()
    {
        // Arrange
        var randomId = Guid.NewGuid();
        var request = new UpdateArticleRequest("Nieuw", "Sum", "Content", Guid.NewGuid());

        // Act
        var response = await _client.PutAsJsonAsync($"/articles/{randomId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteArticle_ShouldReturnNotFound_WhenArticleDoesNotExist()
    {
        // Arrange
        var randomId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/articles/{randomId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateArticle_ShouldReturnBadRequest_WhenTitleIsEmpty()
    {
        // Arrange
        var request = new CreateArticleRequest("", "S", "C", Guid.NewGuid(), true);

        // Act
        var response = await _client.PostAsJsonAsync("/articles", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateArticle_ShouldReturnBadRequest_WhenTitleIsEmpty()
    {
        // Arrange
        var randomId = Guid.NewGuid();
        var request = new UpdateArticleRequest("", "Summary", "Content", Guid.NewGuid());

        // Act
        var response = await _client.PutAsJsonAsync($"/articles/{randomId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}