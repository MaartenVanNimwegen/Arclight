using Arclight.Api.Endpoints;
using Arclight.Application.Interfaces;
using Arclight.Domain.Entities;
using Arclight.Domain.Enums;
using Arclight.Infrastructure.Persistence;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;

namespace Arclight.Api.IntegrationTests.Endpoints;

public class NewsletterEndpointsTests : BaseIntegrationTest
{
    public NewsletterEndpointsTests(CustomWebApplicationFactory factory) : base(factory) { }

    #region Subscribe Tests

    [Fact]
    public async Task Subscribe_ShouldReturnOk_WhenAnonymousUserSubscribes()
    {
        // Arrange
        var email = "new-anonymous@test.nl";
        var request = new SubscribeRequest(email);

        // Act
        var response = await Client.PostAsJsonAsync("/newsletter/subscribe", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // FIX 1: Gebruik Dictionary in plaats van dynamic om de 'message' te lezen
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        result!["message"].Should().Be("Bedankt voor je inschrijving!");
    }

    [Fact]
    public async Task Subscribe_ShouldReturnOk_WhenLoggedInUserSubscribes()
    {
        // Arrange
        var testUserId = Guid.Parse(TestAuthHandler.TestUserId);
        var loggedInClient = CreateClientWithRoles("User");
        var email = "logged-in-user@test.nl";
        var request = new SubscribeRequest(email);

        // Act
        var response = await loggedInClient.PostAsJsonAsync("/newsletter/subscribe", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await ExecuteDbContextAsync(async (context) =>
        {
            var subscriber = await context.Subscribers
                .FirstOrDefaultAsync(s => s.Email == email.ToLower());

            subscriber.Should().NotBeNull();
            subscriber!.UserId.Should().Be(testUserId);
        });
    }

    [Fact]
    public async Task Subscribe_ShouldReturnBadRequest_WhenEmailIsInvalid()
    {
        // Arrange
        var request = new SubscribeRequest("geen-email-formaat");

        // Act
        var response = await Client.PostAsJsonAsync("/newsletter/subscribe", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Subscribe_ShouldReturnConflict_WhenAlreadySubscribed()
    {
        // Arrange
        var email = "already@exists.com";
        var existingSub = new Subscriber(email);

        await ExecuteDbContextAsync(async (context) =>
        {
            context.Subscribers.Add(existingSub);
            await context.SaveChangesAsync();
        });

        var request = new SubscribeRequest(email);

        // Act
        var response = await Client.PostAsJsonAsync("/newsletter/subscribe", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Subscribe_ShouldReturnOk_WhenUnsubscribedUserResubscribes()
    {
        // Arrange
        var email = "returning@user.com";
        var sub = new Subscriber(email);
        sub.Unsubscribe();

        await ExecuteDbContextAsync(async (context) =>
        {
            context.Subscribers.Add(sub);
            await context.SaveChangesAsync();
        });

        var request = new SubscribeRequest(email);

        // Act
        var response = await Client.PostAsJsonAsync("/newsletter/subscribe", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        result!["message"].Should().Contain("Welkom terug");
    }

    #endregion

    #region SendNewsletter Tests

    [Fact]
    public async Task SendNewsletter_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        var anonymousClient = CreateAnonymousClient();
        var request = new SendNewsletterRequest("Subject", "Body");

        // Act
        var response = await anonymousClient.PostAsJsonAsync("/newsletter/send", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SendNewsletter_ShouldReturnForbidden_WhenUserIsNotContentManager()
    {
        // Arrange
        var normalUserClient = CreateClientWithRoles("User");
        var request = new SendNewsletterRequest("Subject", "Body");

        // Act
        var response = await normalUserClient.PostAsJsonAsync("/newsletter/send", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SendNewsletter_ShouldReturnOk_WhenAuthorizedAndSubscribersExist()
    {
        // Arrange
        var adminClient = CreateClientWithRoles("Admin");

        await ExecuteDbContextAsync(async (context) =>
        {
            if (!context.Subscribers.Any(s => s.IsActive))
            {
                context.Subscribers.Add(new Subscriber("active@subscriber.com"));
                await context.SaveChangesAsync();
            }
        });

        var request = new SendNewsletterRequest("Test Subject", "Test Body Content");

        // Act
        var response = await adminClient.PostAsJsonAsync("/newsletter/send", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SendNewsletter_ShouldReturnBadRequest_WhenNoActiveSubscribers()
    {
        // Arrange
        var adminClient = CreateClientWithRoles("Admin");

        await ExecuteDbContextAsync(async (context) =>
        {
            var all = context.Subscribers.ToList();
            context.Subscribers.RemoveRange(all);
            await context.SaveChangesAsync();
        });

        var request = new SendNewsletterRequest("Subject", "Body");

        // Act
        var response = await adminClient.PostAsJsonAsync("/newsletter/send", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SendNewsletter_ShouldReturnBadRequest_WhenSubjectIsEmpty()
    {
        // Arrange
        var adminClient = CreateClientWithRoles("Admin");
        var request = new SendNewsletterRequest("", "Body content");

        // Act
        var response = await adminClient.PostAsJsonAsync("/newsletter/send", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion
}