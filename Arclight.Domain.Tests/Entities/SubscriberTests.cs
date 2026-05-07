using Arclight.Domain.Entities;
using FluentAssertions;
using System;
using Xunit;

namespace Arclight.Domain.Tests.Entities;

public class SubscriberTests
{
    private const string ValidEmail = "test@arclight.nl";

    // --- Constructor Tests ---

    [Fact]
    public void Constructor_ShouldSetProperties_WhenAnonymousSubscriberIsCreated()
    {
        // Act
        var subscriber = new Subscriber(ValidEmail);

        // Assert
        subscriber.Email.Should().Be(ValidEmail);
        subscriber.IsActive.Should().BeTrue();
        subscriber.UserId.Should().BeNull();
        subscriber.SubscribedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
        subscriber.UnsubscribedAt.Should().BeNull();
    }

    [Fact]
    public void Constructor_ShouldSetUserId_WhenLinkedSubscriberIsCreated()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var subscriber = new Subscriber(ValidEmail, userId);

        // Assert
        subscriber.UserId.Should().Be(userId);
        subscriber.Email.Should().Be(ValidEmail);
    }

    [Fact]
    public void Constructor_ShouldLowercaseAndTrimEmail()
    {
        // Arrange
        var messyEmail = "  uPpeRcaSe@TesT.nl  ";

        // Act
        var subscriber = new Subscriber(messyEmail);

        // Assert
        subscriber.Email.Should().Be("uppercase@test.nl");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_ShouldThrowException_WhenEmailIsEmpty(string invalidEmail)
    {
        // Act
        Action act = () => new Subscriber(invalidEmail);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("Email address is required.");
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenEmailFormatIsInvalid()
    {
        // Act
        Action act = () => new Subscriber("geen-at-teken.nl");

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("Invalid email address format.");
    }

    [Fact]
    public void Constructor_WithUserId_ShouldThrowException_WhenUserIdIsEmpty()
    {
        // Act
        Action act = () => new Subscriber(ValidEmail, Guid.Empty);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("UserId must not be empty.");
    }

    // --- Behavior Tests ---

    [Fact]
    public void ChangeEmail_ShouldUpdateEmail_WhenValidEmailProvided()
    {
        // Arrange
        var subscriber = new Subscriber(ValidEmail);
        var newEmail = "new@arclight.nl";

        // Act
        subscriber.ChangeEmail(newEmail);

        // Assert
        subscriber.Email.Should().Be(newEmail);
    }

    [Fact]
    public void ChangeEmail_ShouldDoNothing_WhenEmailIsSame()
    {
        // Arrange
        var subscriber = new Subscriber(ValidEmail);

        // Act
        subscriber.ChangeEmail(ValidEmail.ToUpper());

        // Assert
        subscriber.Email.Should().Be(ValidEmail);
    }

    [Fact]
    public void Unsubscribe_ShouldSetIsActiveToFalseAndSetDate()
    {
        // Arrange
        var subscriber = new Subscriber(ValidEmail);

        // Act
        subscriber.Unsubscribe();

        // Assert
        subscriber.IsActive.Should().BeFalse();
        subscriber.UnsubscribedAt.Should().NotBeNull();
        subscriber.UnsubscribedAt.Value.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Unsubscribe_ShouldDoNothing_WhenAlreadyInactive()
    {
        // Arrange
        var subscriber = new Subscriber(ValidEmail);
        subscriber.Unsubscribe();
        var originalUnsubDate = subscriber.UnsubscribedAt;

        // Act
        subscriber.Unsubscribe();

        // Assert
        subscriber.UnsubscribedAt.Should().Be(originalUnsubDate);
    }

    [Fact]
    public void Resubscribe_ShouldSetIsActiveToTrueAndClearDate()
    {
        // Arrange
        var subscriber = new Subscriber(ValidEmail);
        subscriber.Unsubscribe();

        // Act
        subscriber.Resubscribe();

        // Assert
        subscriber.IsActive.Should().BeTrue();
        subscriber.UnsubscribedAt.Should().BeNull();
    }

    [Fact]
    public void Resubscribe_ShouldDoNothing_WhenAlreadyActive()
    {
        // Arrange
        var subscriber = new Subscriber(ValidEmail);

        // Act
        subscriber.Resubscribe();

        // Assert
        subscriber.IsActive.Should().BeTrue();
    }

    [Fact]
    public void LinkToUser_ShouldSetUserId_WhenValidGuidProvided()
    {
        // Arrange
        var subscriber = new Subscriber(ValidEmail);
        var userId = Guid.NewGuid();

        // Act
        subscriber.LinkToUser(userId);

        // Assert
        subscriber.UserId.Should().Be(userId);
    }

    [Fact]
    public void LinkToUser_ShouldThrowException_WhenGuidIsEmpty()
    {
        // Arrange
        var subscriber = new Subscriber(ValidEmail);

        // Act
        Action act = () => subscriber.LinkToUser(Guid.Empty);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("UserId must not be empty.");
    }
}