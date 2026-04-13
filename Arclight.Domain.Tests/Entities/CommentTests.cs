using Arclight.Domain.Entities;
using FluentAssertions;

namespace Arclight.Domain.Tests.Entities;

public class CommentTests
{
    [Fact]
    public void Constructor_ShouldCreateComment_WhenValidParameters()
    {
        // Arrange
        var text = "Dit is een valide commentaar";
        var articleId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Act
        var comment = new Comment(text, articleId, userId);

        // Assert
        comment.Text.Should().Be(text);
        comment.ArticleId.Should().Be(articleId);
        comment.UserId.Should().Be(userId);
        comment.Id.Should().NotBeEmpty();
        comment.CreatedAt.Should().BeBefore(DateTimeOffset.UtcNow.AddSeconds(1));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_ShouldThrowArgumentException_WhenTextIsInvalid(string invalidText)
    {
        // Act
        Action act = () => new Comment(invalidText, Guid.NewGuid(), Guid.NewGuid());

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("Comment text cannot be empty");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenArticleIdIsEmpty()
    {
        // Act
        Action act = () => new Comment("Valid text", Guid.Empty, Guid.NewGuid());

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("ArticleId is required");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenUserIdIsEmpty()
    {
        // Act
        Action act = () => new Comment("Valid text", Guid.NewGuid(), Guid.Empty);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("UserId is required");
    }

    [Fact]
    public void UpdateText_ShouldUpdateTextAndSetUpdatedDate_WhenValidText()
    {
        // Arrange
        var comment = new Comment("Oude tekst", Guid.NewGuid(), Guid.NewGuid());

        var voorUpdate = DateTimeOffset.UtcNow;
        var nieuweTekst = "Nieuwe, verbeterde tekst";

        // Act
        comment.UpdateText(nieuweTekst);

        // Assert
        comment.Text.Should().Be(nieuweTekst);

        comment.UpdatedAt.Should().NotBeNull();
        comment.UpdatedAt.Value.Should().BeOnOrAfter(voorUpdate);
        comment.UpdatedAt.Value.Should().BeBefore(DateTimeOffset.UtcNow.AddSeconds(1));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void UpdateText_ShouldThrowArgumentException_WhenNewTextIsInvalid(string invalidText)
    {
        // Arrange
        var comment = new Comment("Valid text", Guid.NewGuid(), Guid.NewGuid());

        // Act
        Action act = () => comment.UpdateText(invalidText);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("Text cannot be empty");
    }
}