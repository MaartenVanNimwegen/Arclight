using Arclight.Domain.Entities;
using FluentAssertions;
using System;
using Xunit;

namespace Arclight.Domain.Tests.Entities;

public class CategoryTests
{
    // Constructor tests

    [Fact]
    public void Constructor_ShouldSetProperties_WhenValidDataProvided()
    {
        // Arrange
        var name = "Technologie";
        var slug = "technologie";
        var description = "Alles over software en hardware";

        // Act
        var category = new Category(name, slug, description);

        // Assert
        category.Name.Should().Be(name);
        category.Slug.Should().Be(slug);
        category.Description.Should().Be(description);

        // The list of articles should be initialized (not null) and empty
        category.Articles.Should().NotBeNull();
        category.Articles.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_ShouldThrowArgumentException_WhenNameIsInvalid(string invalidName)
    {
        // Arrange & Act
        Action act = () => new Category(invalidName, "geldige-slug");

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("Name is required");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_ShouldThrowArgumentException_WhenSlugIsInvalid(string invalidSlug)
    {
        // Arrange & Act
        Action act = () => new Category("Geldige Naam", invalidSlug);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("Slug is required");
    }

    // Update tests

    [Fact]
    public void UpdateDetails_ShouldUpdateProperties_WhenValidDataProvided()
    {
        // Arrange
        var category = new Category("Oud", "oud", "Oude omschrijving");

        var newName = "Nieuw";
        var newSlug = "nieuw";
        var newDescription = "Nieuwe omschrijving";

        // Act
        category.UpdateDetails(newName, newSlug, newDescription);

        // Assert
        category.Name.Should().Be(newName);
        category.Slug.Should().Be(newSlug);
        category.Description.Should().Be(newDescription);
        category.UpdatedAt.Should().NotBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void UpdateDetails_ShouldThrowArgumentException_WhenNameIsInvalid(string invalidName)
    {
        // Arrange
        var category = new Category("Geldige Naam", "geldige-slug");

        // Act
        Action act = () => category.UpdateDetails(invalidName, "nieuwe-slug", "desc");

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("Name cannot be empty");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void UpdateDetails_ShouldThrowArgumentException_WhenSlugIsInvalid(string invalidSlug)
    {
        // Arrange
        var category = new Category("Geldige Naam", "geldige-slug");

        // Act
        Action act = () => category.UpdateDetails("Nieuwe Naam", invalidSlug, "desc");

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("Slug cannot be empty");
    }
}