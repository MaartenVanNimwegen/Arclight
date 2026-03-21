using Arclight.Application.Interfaces;
using Arclight.Application.Services;
using FluentAssertions;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Arclight.Application.Tests.Services;

public class SlugServiceTests
{
    private readonly Mock<IArticleRepository> _repositoryMock;
    private readonly SlugService _sut;

    public SlugServiceTests()
    {
        _repositoryMock = new Mock<IArticleRepository>();
        _sut = new SlugService(_repositoryMock.Object);
    }

    [Fact]
    public async Task GenerateUniqueSlugAsync_ShouldReturnBaseSlug_WhenSlugDoesNotExist()
    {
        // Arrange
        var title = "Mijn Nieuwe Blog!";
        var expectedBaseSlug = "mijn-nieuwe-blog";

        _repositoryMock.Setup(repo => repo.SlugExistsAsync(expectedBaseSlug))
                       .ReturnsAsync(false);

        // Act
        var result = await _sut.GenerateUniqueSlugAsync(title);

        // Assert
        result.Should().Be(expectedBaseSlug);
    }

    [Fact]
    public async Task GenerateUniqueSlugAsync_ShouldAppendNumber_WhenSlugAlreadyExists()
    {
        // Arrange
        var title = "Test Artikel";
        var baseSlug = "test-artikel";

        _repositoryMock.Setup(repo => repo.SlugExistsAsync(baseSlug))
                       .ReturnsAsync(true);

        _repositoryMock.Setup(repo => repo.SlugExistsAsync($"{baseSlug}-1"))
                       .ReturnsAsync(false);

        // Act
        var result = await _sut.GenerateUniqueSlugAsync(title);

        // Assert
        result.Should().Be($"{baseSlug}-1");

        _repositoryMock.Verify(repo => repo.SlugExistsAsync(It.IsAny<string>()), Times.Exactly(2));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task GenerateUniqueSlugAsync_ShouldThrowArgumentException_WhenTitleIsInvalid(string invalidTitle)
    {
        // Arrange
        // Act
        Func<Task> act = async () => await _sut.GenerateUniqueSlugAsync(invalidTitle);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
                 .WithMessage("Titel mag niet leeg zijn.");
    }

    [Theory]
    [InlineData("!!!")]
    [InlineData("@@@")]
    public async Task GenerateUniqueSlugAsync_ShouldThrowArgumentException_WhenTitleResultsInEmptySlug(string invalidTitle)
    {
        // Act
        Func<Task> act = async () => await _sut.GenerateUniqueSlugAsync(invalidTitle);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
                 .WithMessage("Titel resulteert in een ongeldige slug na normalisatie.");
    }

    [Theory]
    [InlineData("Hallo   Wereld", "hallo-wereld")]
    [InlineData("---Test---", "test")]
    [InlineData("Wat is C#?", "wat-is-c")]
    public async Task GenerateUniqueSlugAsync_ShouldNormalizeCorrectly(string title, string expected)
    {
        // Arrange
        _repositoryMock.Setup(repo => repo.SlugExistsAsync(It.IsAny<string>()))
                       .ReturnsAsync(false);

        // Act
        var result = await _sut.GenerateUniqueSlugAsync(title);

        // Assert
        result.Should().Be(expected);
    }
}