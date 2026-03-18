using Arclight.Domain.Entities;
using FluentAssertions;
using System;
using Xunit;

namespace Arclight.Domain.Tests.Entities;

public class ArticleTests
{
    private readonly Guid _authorId = Guid.NewGuid();
    private readonly Guid _categoryId = Guid.NewGuid();

    // Constructor Tests

    [Fact]
    public void Constructor_ShouldSetProperties_WhenValidDataProvided()
    {
        var article = new Article("Titel", "slug", "Summary", "Content", _authorId, _categoryId);

        article.Title.Should().Be("Titel");
        article.Slug.Should().Be("slug");
        article.Summary.Should().Be("Summary");
        article.Content.Should().Be("Content");
        article.AuthorId.Should().Be(_authorId);
        article.CategoryId.Should().Be(_categoryId);
        article.IsPublished.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_ShouldThrowException_WhenStringsAreInvalid(string invalidStr)
    {
        Action actTitle = () => new Article(invalidStr, "slug", "sum", "content", _authorId, _categoryId);
        actTitle.Should().Throw<ArgumentException>().WithMessage("Title is required");

        Action actSlug = () => new Article("Title", invalidStr, "sum", "content", _authorId, _categoryId);
        actSlug.Should().Throw<ArgumentException>().WithMessage("Slug is required");

        Action actContent = () => new Article("Title", "slug", "sum", invalidStr, _authorId, _categoryId);
        actContent.Should().Throw<ArgumentException>().WithMessage("Content is required");
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenGuidsAreEmpty()
    {
        Action actAuthor = () => new Article("T", "s", "s", "c", Guid.Empty, _categoryId);
        actAuthor.Should().Throw<ArgumentException>().WithMessage("AuthorId cannot be empty");

        Action actCategory = () => new Article("T", "s", "s", "c", _authorId, Guid.Empty);
        actCategory.Should().Throw<ArgumentException>().WithMessage("CategoryId cannot be empty");
    }

    // Behavior Tests

    [Fact]
    public void UpdateContent_ShouldUpdateProperties_WhenValidDataProvided()
    {
        var article = new Article("T", "s", "s", "c", _authorId, _categoryId);

        article.UpdateContent("NewTitle", "new-slug", "NewSum", "NewContent");

        article.Title.Should().Be("NewTitle");
        article.Slug.Should().Be("new-slug");
        article.Summary.Should().Be("NewSum");
        article.Content.Should().Be("NewContent");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void UpdateContent_ShouldThrowException_WhenDataIsInvalid(string invalid)
    {
        var article = new Article("T", "s", "s", "c", _authorId, _categoryId);

        Action actTitle = () => article.UpdateContent(invalid, "s", "s", "c");
        actTitle.Should().Throw<ArgumentException>().WithMessage("Title cannot be empty");

        Action actSlug = () => article.UpdateContent("T", invalid, "s", "c");
        actSlug.Should().Throw<ArgumentException>().WithMessage("Slug cannot be empty");

        Action actSum = () => article.UpdateContent("T", "s", invalid, "c");
        actSum.Should().Throw<ArgumentException>().WithMessage("Summary cannot be empty");

        Action actContent = () => article.UpdateContent("T", "s", "s", invalid);
        actContent.Should().Throw<ArgumentException>().WithMessage("Content cannot be empty");
    }

    [Fact]
    public void ChangeCategory_ShouldUpdateCategory_WhenValidGuidProvided()
    {
        var article = new Article("T", "s", "s", "c", _authorId, _categoryId);
        var newCatId = Guid.NewGuid();

        article.ChangeCategory(newCatId);

        article.CategoryId.Should().Be(newCatId);
    }

    [Fact]
    public void ChangeCategory_ShouldThrowException_WhenGuidIsEmpty()
    {
        var article = new Article("T", "s", "s", "c", _authorId, _categoryId);
        Action act = () => article.ChangeCategory(Guid.Empty);
        act.Should().Throw<ArgumentException>().WithMessage("CategoryId cannot be empty");
    }

    [Fact]
    public void PublishAndUnpublish_ShouldChangeStateCorrectly()
    {
        var article = new Article("T", "s", "s", "c", _authorId, _categoryId);

        // Act & Assert Publish
        article.Publish();
        article.IsPublished.Should().BeTrue();
        article.PublishedAt.Should().NotBeNull();
        article.PublishedAt.Value.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));

        // Act & Assert Unpublish
        article.Unpublish();
        article.IsPublished.Should().BeFalse();
        article.PublishedAt.Should().BeNull();
    }
}