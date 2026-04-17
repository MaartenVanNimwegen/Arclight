using Arclight.Application.Interfaces;
using Arclight.Application.Services;
using Arclight.Domain.Enums;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Arclight.Application.Tests.Services;

public class SlugServiceTests
{
    private readonly Mock<IArticleRepository> _articleRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly SlugService _sut;

    public SlugServiceTests()
    {
        _articleRepositoryMock = new Mock<IArticleRepository>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _sut = new SlugService(_articleRepositoryMock.Object, _categoryRepositoryMock.Object);
    }

    [Fact]
    public async Task GenerateUniqueSlugAsync_ShouldReturnBaseSlug_WhenNoCollisionsExist()
    {
        // Arrange
        var title = "Mijn Nieuwe Blog!";
        var expectedBaseSlug = "mijn-nieuwe-blog";

        _articleRepositoryMock.Setup(repo => repo.GetExistingSlugsAsync(expectedBaseSlug))
                       .ReturnsAsync(new List<string>());

        // Act
        var result = await _sut.GenerateUniqueSlugAsync(title, SlugType.Article);

        // Assert
        result.Should().Be(expectedBaseSlug);
    }

    [Fact]
    public async Task GenerateUniqueSlugAsync_ShouldAppendNumber_WhenSlugAlreadyExists()
    {
        // Arrange
        var title = "Test Artikel";
        var baseSlug = "test-artikel";

        _articleRepositoryMock.Setup(repo => repo.GetExistingSlugsAsync(baseSlug))
                       .ReturnsAsync(new List<string> { "test-artikel" });

        // Act
        var result = await _sut.GenerateUniqueSlugAsync(title, SlugType.Article);

        // Assert
        result.Should().Be("test-artikel-1");
    }

    [Fact]
    public async Task GenerateUniqueSlugAsync_ShouldFindNextAvailableNumber_WhenMultipleExist()
    {
        // Arrange
        var title = "Test Artikel";
        var baseSlug = "test-artikel";

        _articleRepositoryMock.Setup(repo => repo.GetExistingSlugsAsync(baseSlug))
                       .ReturnsAsync(new List<string> { "test-artikel", "test-artikel-1", "test-artikel-2" });

        // Act
        var result = await _sut.GenerateUniqueSlugAsync(title, SlugType.Article);

        // Assert
        result.Should().Be("test-artikel-3");
    }

    [Theory]
    [InlineData("Hallo   Wereld", "hallo-wereld")]
    [InlineData("---Test---", "test")]
    [InlineData("Wat is C#?", "wat-is-c")]
    [InlineData("Café & Restaurant", "cafe-restaurant")]
    public async Task GenerateUniqueSlugAsync_ShouldNormalizeCorrectly(string title, string expected)
    {
        // Arrange
        _articleRepositoryMock.Setup(repo => repo.GetExistingSlugsAsync(It.IsAny<string>()))
                       .ReturnsAsync(new List<string>());

        // Act
        var result = await _sut.GenerateUniqueSlugAsync(title, SlugType.Article);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task GenerateUniqueSlugAsync_ShouldThrowArgumentException_WhenTitleIsInvalid(string invalidTitle)
    {
        Func<Task> act = async () => await _sut.GenerateUniqueSlugAsync(invalidTitle, SlugType.Article);

        await act.Should().ThrowAsync<ArgumentException>()
                 .WithMessage("Input cannot be empty.");
    }

    [Fact]
    public async Task GenerateUniqueSlugAsync_ShouldUseCategoryRepository_WhenTypeIsCategory()
    {
        // Arrange
        var title = "Nieuwe Categorie";
        var expectedBase = "nieuwe-categorie";

        _categoryRepositoryMock.Setup(repo => repo.GetExistingSlugsAsync(expectedBase))
                       .ReturnsAsync(new List<string>());

        // Act
        var result = await _sut.GenerateUniqueSlugAsync(title, SlugType.Category);

        // Assert
        result.Should().Be(expectedBase);
        _categoryRepositoryMock.Verify(repo => repo.GetExistingSlugsAsync(expectedBase), Times.Once);
        _articleRepositoryMock.Verify(repo => repo.GetExistingSlugsAsync(It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData("!!!")]
    [InlineData("  @  ")]
    public async Task GenerateUniqueSlugAsync_ShouldThrowArgumentException_WhenResultingSlugIsEmpty(string invalidTitle)
    {
        // Act
        Func<Task> act = async () => await _sut.GenerateUniqueSlugAsync(invalidTitle, SlugType.Article);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
                 .WithMessage("Input resulted in an empty slug.");
    }

    [Fact]
    public async Task GenerateUniqueSlugAsync_ShouldThrowException_WhenSlugTypeIsInvalid()
    {
        // Arrange
        var invalidType = (SlugType)999;

        // Act
        Func<Task> act = async () => await _sut.GenerateUniqueSlugAsync("test", invalidType);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData("!!!")]
    [InlineData("  @  ")]
    [InlineData("😊😊😊")]
    [InlineData("- - -")]
    public async Task GenerateUniqueSlugAsync_ShouldThrowArgumentException_WhenNormalizationResultsInEmptySlug(string invalidTitle)
    {
        // Arrange & Act
        Func<Task> act = async () => await _sut.GenerateUniqueSlugAsync(invalidTitle, SlugType.Article);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
                 .WithMessage("Input resulted in an empty slug.");
    }
}