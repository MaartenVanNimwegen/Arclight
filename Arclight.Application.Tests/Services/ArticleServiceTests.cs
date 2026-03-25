using Arclight.Application.DTOs;
using Arclight.Application.Interfaces;
using Arclight.Application.Services;
using Arclight.Domain.Entities;
using Arclight.Domain.Enums;
using FluentAssertions;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Arclight.Application.Tests.Services;

public class ArticleServiceTests
{
    private readonly Mock<IArticleRepository> _articleRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<ISlugService> _slugServiceMock;
    private readonly ArticleService _sut;

    public ArticleServiceTests()
    {
        _articleRepoMock = new Mock<IArticleRepository>();
        _userRepoMock = new Mock<IUserRepository>();
        _slugServiceMock = new Mock<ISlugService>();

        _sut = new ArticleService(_articleRepoMock.Object, _userRepoMock.Object, _slugServiceMock.Object);
    }

    [Fact]
    public async Task CreateArticleAsync_ShouldReturnGuid_AndCallRepository_WhenAuthorExists()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var author = new User("test@test.nl", "John", "Doe", "hash", UserRole.ContentCreator);
        var request = new CreateArticleRequest("Mijn Titel", "Korte samenvatting", "Inhoud", categoryId, false);

        _userRepoMock.Setup(repo => repo.GetByIdAsync(authorId))
                     .ReturnsAsync(author); 

        _slugServiceMock
            .Setup(s => s.GenerateUniqueSlugAsync(request.Title, SlugType.Article))
            .ReturnsAsync("mijn-titel");

        // Act
        var resultId = await _sut.CreateArticleAsync(request, authorId);

        // Assert
        resultId.Should().NotBeEmpty();
        _articleRepoMock.Verify(repo => repo.AddAsync(It.Is<Article>(a =>
                    a.Title == request.Title &&
                    a.Slug == "mijn-titel" &&
                    a.AuthorId == authorId &&
                    a.CategoryId == categoryId
                )), Times.Once);
        _articleRepoMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateArticleAsync_ShouldThrowUnauthorizedAccessException_WhenAuthorDoesNotExist()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var request = new CreateArticleRequest("Titel", "Sum", "Content", Guid.NewGuid(), false);

        _userRepoMock.Setup(repo => repo.GetByIdAsync(authorId))
                     .ReturnsAsync((User?)null);

        // Act
        var act = () => _sut.CreateArticleAsync(request, authorId);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("The given author is not found.");

        _articleRepoMock.Verify(repo => repo.AddAsync(It.IsAny<Article>()), Times.Never);
        _articleRepoMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateArticleAsync_ShouldPublishImmediately_WhenPublishNowIsTrue()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var author = new User("test@test.nl", "John", "Doe", "hash", UserRole.ContentCreator);
        var request = new CreateArticleRequest("Titel", "Sum", "Content", Guid.NewGuid(), true);

        _userRepoMock.Setup(repo => repo.GetByIdAsync(authorId)).ReturnsAsync(author);
        _slugServiceMock.Setup(s => s.GenerateUniqueSlugAsync(request.Title, SlugType.Article)).ReturnsAsync("titel");

        // Act
        await _sut.CreateArticleAsync(request, authorId);

        // Assert
        _articleRepoMock.Verify(repo => repo.AddAsync(It.Is<Article>(a => a.IsPublished)), Times.Once);
    }

    [Fact]
    public async Task GetArticleBySlugAsync_ShouldReturnUnknowns_WhenNavigationPropertiesAreNull()
    {
        // Arrange
        var article = new Article("Titel", "slug", "sum", "content", Guid.NewGuid(), Guid.NewGuid());
        article.Publish();

        _articleRepoMock.Setup(repo => repo.GetBySlugAsync("slug"))
                        .ReturnsAsync(article);

        // Act
        var result = await _sut.GetArticleBySlugAsync("slug");

        // Assert
        result.Should().NotBeNull();
        result!.AuthorName.Should().Be("Unknown author");
        result!.CategoryName.Should().Be("No category");
    }

    [Fact]
    public async Task UpdateArticleAsync_ShouldReturnFalse_WhenArticleDoesNotExist()
    {
        // Arrange
        var articleId = Guid.NewGuid();
        var request = new UpdateArticleRequest("Nieuwe Titel", "Samenvatting", "Inhoud", Guid.NewGuid());

        _articleRepoMock.Setup(repo => repo.GetByIdAsync(articleId))
                        .ReturnsAsync((Article?)null);

        // Act
        var result = await _sut.UpdateArticleAsync(articleId, request);

        // Assert
        result.Should().BeFalse();

        _articleRepoMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateArticleAsync_ShouldReturnTrue_AndCallSave_WhenArticleExists()
    {
        // Arrange
        var articleId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var existingArticle = new Article("Oude Titel", "oude-slug", "Oude sum", "Oude content", Guid.NewGuid(), categoryId);

        var request = new UpdateArticleRequest("Nieuwe Titel", "Nieuwe sum", "Nieuwe content", categoryId);

        _articleRepoMock.Setup(repo => repo.GetByIdAsync(articleId))
                        .ReturnsAsync(existingArticle);

        // Act
        var result = await _sut.UpdateArticleAsync(articleId, request);

        // Assert
        result.Should().BeTrue();

        _articleRepoMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);

        existingArticle.Title.Should().Be("Nieuwe Titel");
    }

    // Delete tests

    [Fact]
    public async Task DeleteArticleAsync_ShouldReturnFalse_WhenArticleDoesNotExist()
    {
        // Arrange
        var articleId = Guid.NewGuid();
        _articleRepoMock.Setup(repo => repo.GetByIdAsync(articleId))
                        .ReturnsAsync((Article?)null);

        // Act
        var result = await _sut.DeleteArticleAsync(articleId);

        // Assert
        result.Should().BeFalse();

        _articleRepoMock.Verify(repo => repo.Delete(It.IsAny<Article>()), Times.Never);
        _articleRepoMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteArticleAsync_ShouldReturnTrue_AndCallDelete_WhenArticleExists()
    {
        // Arrange
        var articleId = Guid.NewGuid();
        var existingArticle = new Article("Te verwijderen", "slug", "sum", "content", Guid.NewGuid(), Guid.NewGuid());

        _articleRepoMock.Setup(repo => repo.GetByIdAsync(articleId))
                        .ReturnsAsync(existingArticle);

        // Act
        var result = await _sut.DeleteArticleAsync(articleId);

        // Assert
        result.Should().BeTrue();

        _articleRepoMock.Verify(repo => repo.Delete(existingArticle), Times.Once);

        _articleRepoMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    // Get tests

    [Fact]
    public async Task GetAllPublishedArticlesAsync_ShouldReturnMappedResponses()
    {
        // Arrange
        var author = new User("test@test.nl", "John", "Doe", "hash", Arclight.Domain.Enums.UserRole.ContentCreator);
        var category = new Category("Tech", "tech", "desc");

        var article1 = new Article("Titel 1", "titel-1", "sum", "content", author.Id, category.Id);
        article1.Publish();

        var article2 = new Article("Titel 2", "titel-2", "sum", "content", author.Id, category.Id);
        article2.Publish();

        var articles = new List<Article> { article1, article2 };

        _articleRepoMock.Setup(repo => repo.GetAllPublishedAsync())
                        .ReturnsAsync(articles);

        // Act
        var result = await _sut.GetAllPublishedArticlesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.First().Title.Should().Be("Titel 1");
    }

    [Fact]
    public async Task GetArticleBySlugAsync_ShouldReturnNull_WhenArticleDoesNotExist()
    {
        // Arrange
        _articleRepoMock.Setup(repo => repo.GetBySlugAsync("bestaat-niet"))
                        .ReturnsAsync((Article?)null);

        // Act
        var result = await _sut.GetArticleBySlugAsync("bestaat-niet");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetArticleBySlugAsync_ShouldReturnNull_WhenArticleIsNotPublished()
    {
        // Arrange
        var unpublishedArticle = new Article("Titel", "slug", "sum", "content", Guid.NewGuid(), Guid.NewGuid());

        _articleRepoMock.Setup(repo => repo.GetBySlugAsync("slug"))
                        .ReturnsAsync(unpublishedArticle);

        // Act
        var result = await _sut.GetArticleBySlugAsync("slug");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetArticleBySlugAsync_ShouldReturnResponse_WhenArticleIsPublished()
    {
        // Arrange
        var publishedArticle = new Article("Titel", "slug", "sum", "content", Guid.NewGuid(), Guid.NewGuid());
        publishedArticle.Publish();

        _articleRepoMock.Setup(repo => repo.GetBySlugAsync("slug"))
                        .ReturnsAsync(publishedArticle);

        // Act
        var result = await _sut.GetArticleBySlugAsync("slug");

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Titel");
    }
}