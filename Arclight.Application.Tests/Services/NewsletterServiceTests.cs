using Arclight.Application.Interfaces;
using Arclight.Application.Services;
using Arclight.Domain.Entities;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Arclight.Application.Tests.Services;

public class NewsletterServiceTests
{
    private readonly Mock<INewsletterRepository> _newsletterRepoMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly NewsletterService _sut;

    public NewsletterServiceTests()
    {
        _newsletterRepoMock = new Mock<INewsletterRepository>();
        _emailServiceMock = new Mock<IEmailService>();

        _sut = new NewsletterService(_newsletterRepoMock.Object, _emailServiceMock.Object);
    }

    #region SubscribeAsync Tests

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    [InlineData("ongeldig-email")]
    public async Task SubscribeAsync_ShouldThrowArgumentException_WhenEmailIsInvalid(string invalidEmail)
    {
        // Act
        var act = () => _sut.SubscribeAsync(invalidEmail, null);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Ongeldig e-mailadres.");

        _newsletterRepoMock.Verify(r => r.AddAsync(It.IsAny<Subscriber>()), Times.Never);
    }

    [Fact]
    public async Task SubscribeAsync_ShouldAddAnonymousSubscriber_WhenEmailIsNew()
    {
        // Arrange
        var email = "new@test.nl";
        _newsletterRepoMock.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync((Subscriber?)null);

        // Act
        var result = await _sut.SubscribeAsync(email, null);

        // Assert
        result.Should().Be("Bedankt voor je inschrijving!");
        _newsletterRepoMock.Verify(r => r.AddAsync(It.Is<Subscriber>(s =>
            s.Email == email && s.UserId == null)), Times.Once);
    }

    [Fact]
    public async Task SubscribeAsync_ShouldAddLoggedInSubscriber_WhenEmailIsNew()
    {
        // Arrange
        var email = "user@test.nl";
        var userId = Guid.NewGuid();
        _newsletterRepoMock.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync((Subscriber?)null);

        // Act
        var result = await _sut.SubscribeAsync(email, userId);

        // Assert
        result.Should().Be("Bedankt voor je inschrijving!");
        _newsletterRepoMock.Verify(r => r.AddAsync(It.Is<Subscriber>(s =>
            s.Email == email && s.UserId == userId)), Times.Once);
    }

    [Fact]
    public async Task SubscribeAsync_ShouldResubscribe_WhenExistingSubscriberIsInactive()
    {
        // Arrange
        var email = "returning@test.nl";
        var subscriber = new Subscriber(email);
        subscriber.Unsubscribe();

        _newsletterRepoMock.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync(subscriber);

        // Act
        var result = await _sut.SubscribeAsync(email, null);

        // Assert
        result.Should().Be("Welkom terug! Je bent weer ingeschreven.");
        subscriber.IsActive.Should().BeTrue();
        _newsletterRepoMock.Verify(r => r.UpdateAsync(subscriber), Times.Once);
    }

    [Fact]
    public async Task SubscribeAsync_ShouldLinkToUserAndResubscribe_WhenInactiveSubscriberLogsIn()
    {
        // Arrange
        var email = "returning@test.nl";
        var userId = Guid.NewGuid();
        var subscriber = new Subscriber(email);
        subscriber.Unsubscribe();

        _newsletterRepoMock.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync(subscriber);

        // Act
        var result = await _sut.SubscribeAsync(email, userId);

        // Assert
        subscriber.UserId.Should().Be(userId);
        subscriber.IsActive.Should().BeTrue();
        _newsletterRepoMock.Verify(r => r.UpdateAsync(subscriber), Times.Once);
    }

    [Fact]
    public async Task SubscribeAsync_ShouldThrowInvalidOperationException_WhenSubscriberIsAlreadyActive()
    {
        // Arrange
        var email = "active@test.nl";
        var subscriber = new Subscriber(email);

        _newsletterRepoMock.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync(subscriber);

        // Act
        var act = () => _sut.SubscribeAsync(email, null);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Dit e-mailadres is al ingeschreven.");
    }

    #endregion

    #region SendNewsletterAsync Tests

    [Fact]
    public async Task SendNewsletterAsync_ShouldThrowInvalidOperationException_WhenNoActiveSubscribers()
    {
        // Arrange
        _newsletterRepoMock.Setup(r => r.GetAllActiveEmailsAsync())
            .ReturnsAsync(new List<string>());

        // Act
        var act = () => _sut.SendNewsletterAsync("Subject", "Content");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Er zijn geen actieve abonnees om de nieuwsbrief naar te verzenden.");

        _emailServiceMock.Verify(e => e.SendEmailAsync(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SendNewsletterAsync_ShouldCallEmailService_WhenActiveSubscribersExist()
    {
        // Arrange
        var emails = new List<string> { "sub1@test.nl", "sub2@test.nl" };
        var subject = "Weekly Update";
        var content = "Check out our new articles!";

        _newsletterRepoMock.Setup(r => r.GetAllActiveEmailsAsync()).ReturnsAsync(emails);

        // Act
        await _sut.SendNewsletterAsync(subject, content);

        // Assert
        _emailServiceMock.Verify(e => e.SendEmailAsync(emails, subject, content), Times.Once);
    }

    #endregion
}