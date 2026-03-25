using Arclight.Domain.Entities;
using Arclight.Domain.Enums;
using Arclight.Infrastructure.Persistence;
using Arclight.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Arclight.Infrastructure.Tests.Repositories;

public class UserRepositoryTests
{
    private AppDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnUser_WhenEmailExists()
    {
        // Arrange
        var context = GetDbContext();
        var repo = new UserRepository(context);

        var user = new User("test@domain.com", "John", "Doe", "hash", UserRole.User);

        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        var result = await repo.GetByEmailAsync("test@domain.com");

        // Assert
        result.Should().NotBeNull();
        result!.FirstName.Should().Be("John");
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnNull_WhenEmailDoesNotExist()
    {
        // Arrange
        var context = GetDbContext();
        var repo = new UserRepository(context);

        // Act
        var result = await repo.GetByEmailAsync("bestaat-niet@domain.com");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_ShouldAddUserToDatabase()
    {
        // Arrange
        var context = GetDbContext();
        var repo = new UserRepository(context);
        var user = new User("new@domain.com", "Jane", "Doe", "hash", UserRole.Admin);

        // Act
        await repo.AddAsync(user);
        await context.SaveChangesAsync();

        // Assert
        var savedUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "new@domain.com");
        savedUser.Should().NotBeNull();
        savedUser!.Id.Should().Be(user.Id);
    }
}