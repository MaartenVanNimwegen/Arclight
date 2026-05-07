using Arclight.Domain.Entities;
using Arclight.Infrastructure.Persistence;
using Arclight.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Arclight.Infrastructure.Tests.Repositories;

public class NewsletterRepositoryTests
{
    private AppDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnSubscriber_WhenEmailExists()
    {
        // Arrange
        var context = GetDbContext();
        var repo = new NewsletterRepository(context);
        var email = "test@arclight.nl";
        var subscriber = new Subscriber(email);

        context.Subscribers.Add(subscriber);
        await context.SaveChangesAsync();

        // Act
        var result = await repo.GetByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnNull_WhenEmailDoesNotExist()
    {
        // Arrange
        var context = GetDbContext();
        var repo = new NewsletterRepository(context);

        // Act
        var result = await repo.GetByEmailAsync("niet-bestaand@test.nl");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_ShouldAddSubscriberToContext()
    {
        // Arrange
        var context = GetDbContext();
        var repo = new NewsletterRepository(context);
        var subscriber = new Subscriber("new@test.nl");

        // Act
        await repo.AddAsync(subscriber);

        // Assert
        var savedSubscriber = await context.Subscribers.FirstOrDefaultAsync(s => s.Email == "new@test.nl");
        savedSubscriber.Should().NotBeNull();
        savedSubscriber!.Email.Should().Be("new@test.nl");
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistChangesToDatabase()
    {
        // Arrange
        var context = GetDbContext();
        var repo = new NewsletterRepository(context);
        var subscriber = new Subscriber("update@test.nl");
        context.Subscribers.Add(subscriber);
        await context.SaveChangesAsync();

        // Act - Verander de status naar inactief via de domain behavior
        subscriber.Unsubscribe();
        await repo.UpdateAsync(subscriber);

        // Assert
        context.ChangeTracker.Clear(); // Forceer herladen uit de in-memory db
        var updatedSubscriber = await context.Subscribers.FirstOrDefaultAsync(s => s.Email == "update@test.nl");
        updatedSubscriber!.IsActive.Should().BeFalse();
        updatedSubscriber.UnsubscribedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllActiveEmailsAsync_ShouldReturnOnlyEmailsOfActiveSubscribers()
    {
        // Arrange
        var context = GetDbContext();
        var repo = new NewsletterRepository(context);

        var active1 = new Subscriber("active1@test.nl");
        var active2 = new Subscriber("active2@test.nl");
        var inactive = new Subscriber("inactive@test.nl");
        inactive.Unsubscribe();

        context.Subscribers.AddRange(active1, active2, inactive);
        await context.SaveChangesAsync();

        // Act
        var result = await repo.GetAllActiveEmailsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain("active1@test.nl");
        result.Should().Contain("active2@test.nl");
        result.Should().NotContain("inactive@test.nl");
    }

    [Fact]
    public async Task GetAllActiveEmailsAsync_ShouldReturnEmptyList_WhenNoSubscribersExist()
    {
        // Arrange
        var context = GetDbContext();
        var repo = new NewsletterRepository(context);

        // Act
        var result = await repo.GetAllActiveEmailsAsync();

        // Assert
        result.Should().BeEmpty();
    }
}