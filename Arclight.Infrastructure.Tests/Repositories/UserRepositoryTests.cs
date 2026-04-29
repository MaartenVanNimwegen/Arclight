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

    [Fact]
    public async Task GetAllUsersAsync_ShouldReturnAllUsers()
    {
        // Arrange
        var context = GetDbContext();
        var repo = new UserRepository(context);

        var user1 = new User("user1@test.nl", "A", "B", "hash", UserRole.User);
        var user2 = new User("user2@test.nl", "C", "D", "hash", UserRole.Admin);

        context.Users.AddRange(user1, user2);
        await context.SaveChangesAsync();

        // Act
        var result = (await repo.GetAllUsersAsync()).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(u => u.Email == "user1@test.nl");
        result.Should().Contain(u => u.Email == "user2@test.nl");
    }

    [Fact]
    public async Task UpdateUserRoleAsync_ShouldUpdateRole_WhenUserExists()
    {
        // Arrange
        var context = GetDbContext();
        var repo = new UserRepository(context);

        var user = new User("test@test.nl", "Jan", "Jansen", "hash", UserRole.User);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();

        // Act
        await repo.UpdateUserRoleAsync(user.Id, UserRole.Admin);

        // Assert
        var updatedUser = await context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);

        updatedUser.Should().NotBeNull();
        updatedUser!.Role.Should().Be(UserRole.Admin);
    }

    [Fact]
    public async Task UpdateUserRoleAsync_ShouldThrowKeyNotFoundException_WhenUserDoesNotExist()
    {
        // Arrange
        var context = GetDbContext();
        var repo = new UserRepository(context);

        var randomId = Guid.NewGuid();

        // Act
        var act = async () => await repo.UpdateUserRoleAsync(randomId, UserRole.Admin);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();

        context.Users.Should().BeEmpty();
    }

    [Fact]
    public async Task Delete_ShouldRemoveUserFromContext()
    {
        // Arrange
        var context = GetDbContext();
        var repo = new UserRepository(context);

        var user = new User("delete@test.nl", "To", "Delete", "hash", UserRole.User);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        repo.Delete(user);

        await context.SaveChangesAsync();

        // Assert
        context.Users.Should().BeEmpty();
    }
}