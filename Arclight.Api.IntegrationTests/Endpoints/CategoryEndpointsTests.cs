using Arclight.Application.DTOs;
using Arclight.Domain.Entities;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace Arclight.Api.IntegrationTests.Endpoints;

public class CategoryEndpointsTests : BaseIntegrationTest
{
    public CategoryEndpointsTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetAllCategories_ShouldReturnOk_AndList()
    {
        // Arrange
        await ExecuteDbContextAsync(async (context) =>
        {
            context.Categories.Add(new Category("Tech", "tech", "Description"));
            await context.SaveChangesAsync();
        });

        // Act
        var response = await Client.GetAsync("/categories");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var categories = await response.Content.ReadFromJsonAsync<IEnumerable<CategoryResponse>>();
        categories.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateCategory_ShouldReturnCreated_WhenValid()
    {
        // Arrange
        var adminClient = CreateClientWithRoles("Admin");
        var request = new CreateCategoryRequest("Lifestyle", "Everything about life");

        // Act
        var response = await adminClient.PostAsJsonAsync("/categories", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var id = await response.Content.ReadFromJsonAsync<Guid>();
        id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateCategory_ShouldReturnBadRequest_WhenNameIsEmpty()
    {
        // Arrange
        var adminClient = CreateClientWithRoles("Admin");
        var request = new CreateCategoryRequest("", "Description");

        // Act
        var response = await adminClient.PostAsJsonAsync("/categories", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateCategory_ShouldReturnForbidden_WhenNotAdmin()
    {
        // Arrange
        var request = new CreateCategoryRequest("Lifestyle", "Everything about life");

        // Act
        var response = await Client.PostAsJsonAsync("/categories", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteCategory_ShouldReturnNoContent_WhenSuccessful()
    {
        // Arrange
        var adminClient = CreateClientWithRoles("Admin");
        var category = new Category("To Delete", "to-delete", "desc");
        await ExecuteDbContextAsync(async (context) =>
        {
            context.Categories.Add(category);
            await context.SaveChangesAsync();
        });

        // Act
        var response = await adminClient.DeleteAsync($"/categories/{category.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteCategory_ShouldReturnNotFound_WhenDoesNotExist()
    {
        // Arrange
        var adminClient = CreateClientWithRoles("Admin");
        var randomId = Guid.NewGuid();

        // Act
        var response = await adminClient.DeleteAsync($"/categories/{randomId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteCategory_ShouldReturnConflict_WhenCategoryHasArticles()
    {
        // Arrange
        var adminClient = CreateClientWithRoles("Admin");
        var category = new Category("Has Articles", "has-articles", "desc");
        var article = new Article("Title", "slug", "Sum", "Cont", Guid.NewGuid(), category.Id);

        await ExecuteDbContextAsync(async (context) =>
        {
            context.Categories.Add(category);
            context.Articles.Add(article);
            await context.SaveChangesAsync();
        });

        // Act
        var response = await adminClient.DeleteAsync($"/categories/{category.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var error = await response.Content.ReadAsStringAsync();
        error.Should().Contain("still contains articles");
    }

    [Fact]
    public async Task DeleteCategory_ShouldReturnForbidden_WhenNotAdmin()
    {
        // Arrange
        var randomId = Guid.NewGuid();

        // Act
        var response = await Client.DeleteAsync($"/categories/{randomId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateCategory_ShouldReturnNoContent_WhenValid()
    {
        // Arrange
        var adminClient = CreateClientWithRoles("Admin");
        var category = new Category("Old Name", "old-slug", "Old Desc");
        await ExecuteDbContextAsync(async (context) =>
        {
            context.Categories.Add(category);
            await context.SaveChangesAsync();
        });

        var request = new UpdateCategoryRequest("New Name", "New Description");

        // Act
        var response = await adminClient.PutAsJsonAsync($"/categories/{category.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateCategory_ShouldReturnNotFound_WhenDoesNotExist()
    {
        // Arrange
        var adminClient = CreateClientWithRoles("Admin");
        var request = new UpdateCategoryRequest("Name", "Desc");

        // Act
        var response = await adminClient.PutAsJsonAsync($"/categories/{Guid.NewGuid()}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateCategory_ShouldReturnForbidden_WhenNotAdmin()
    {
        // Arrange
        var request = new UpdateCategoryRequest("Name", "Desc");

        // Act
        var response = await Client.PutAsJsonAsync($"/categories/{Guid.NewGuid()}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}