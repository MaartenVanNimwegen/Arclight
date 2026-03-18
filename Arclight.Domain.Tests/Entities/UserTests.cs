using Arclight.Domain.Entities;
using Arclight.Domain.Enums;
using FluentAssertions;
using System;
using Xunit;

namespace Arclight.Domain.Tests.Entities;

public class UserTests
{
    // Constructor Tests

    [Fact]
    public void Constructor_ShouldSetProperties_WhenValidDataProvided()
    {
        var user = new User("test@test.nl", "John", "Doe", "hash123", UserRole.ContentCreator);

        user.Email.Should().Be("test@test.nl");
        user.FirstName.Should().Be("John");
        user.LastName.Should().Be("Doe");
        user.PasswordHash.Should().Be("hash123");
        user.Role.Should().Be(UserRole.ContentCreator);
        user.Status.Should().Be(UserStatus.Active);
        user.FullName.Should().Be("John, Doe");
        user.Articles.Should().NotBeNull();
    }

    [Fact]
    public void SeedConstructor_ShouldSetAllProperties()
    {
        var id = Guid.NewGuid();
        var user = new User(id, "test@test.nl", "J", "D", "hash", UserRole.Admin, UserStatus.Inactive);

        user.Id.Should().Be(id);
        user.Status.Should().Be(UserStatus.Inactive);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_ShouldThrowException_WhenStringsAreInvalid(string invalid)
    {
        Action actEmail = () => new User(invalid, "F", "L", "H", UserRole.Admin);
        actEmail.Should().Throw<ArgumentException>().WithMessage("Email is required");

        Action actFirst = () => new User("E", invalid, "L", "H", UserRole.Admin);
        actFirst.Should().Throw<ArgumentException>().WithMessage("First name is required");

        Action actLast = () => new User("E", "F", invalid, "H", UserRole.Admin);
        actLast.Should().Throw<ArgumentException>().WithMessage("Last name is required");

        Action actHash = () => new User("E", "F", "L", invalid, UserRole.Admin);
        actHash.Should().Throw<ArgumentException>().WithMessage("Password hash is required");
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenRoleIsInvalid()
    {
        Action act = () => new User("E", "F", "L", "H", (UserRole)999);
        act.Should().Throw<ArgumentException>().WithMessage("Invalid user role");
    }

    // Behavior Tests

    [Fact]
    public void UpdateName_ShouldUpdateProperties()
    {
        var user = new User("E", "F", "L", "H", UserRole.Admin);
        user.UpdateName("NewFirst", "NewLast");

        user.FirstName.Should().Be("NewFirst");
        user.LastName.Should().Be("NewLast");
        user.FullName.Should().Be("NewFirst, NewLast");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void UpdateName_ShouldThrowException_WhenInvalid(string invalid)
    {
        var user = new User("E", "F", "L", "H", UserRole.Admin);

        Action actFirst = () => user.UpdateName(invalid, "L");
        actFirst.Should().Throw<ArgumentException>().WithMessage("First name cannot be empty");

        Action actLast = () => user.UpdateName("F", invalid);
        actLast.Should().Throw<ArgumentException>().WithMessage("Last name cannot be empty");
    }

    [Fact]
    public void UpdateEmailAndChangePassword_ShouldUpdateProperties()
    {
        var user = new User("E", "F", "L", "H", UserRole.Admin);

        user.UpdateEmail("new@test.nl");
        user.Email.Should().Be("new@test.nl");

        user.ChangePassword("newHash");
        user.PasswordHash.Should().Be("newHash");
    }

    [Fact]
    public void ChangeRole_ShouldUpdateRole()
    {
        var user = new User("E", "F", "L", "H", UserRole.Admin);
        user.ChangeRole(UserRole.User);
        user.Role.Should().Be(UserRole.User);
    }

    [Fact]
    public void RecordLogin_ShouldSetLastLoggedinDate()
    {
        var user = new User("E", "F", "L", "H", UserRole.Admin);
        user.RecordLogin();

        user.LastLoggedinDate.Should().NotBeNull();
        user.LastLoggedinDate.Value.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ActivateAndDeactivate_ShouldChangeStatus()
    {
        var user = new User(Guid.NewGuid(), "E", "F", "L", "H", UserRole.Admin, UserStatus.Active);

        user.Deactivate();
        user.Status.Should().Be(UserStatus.Inactive);

        user.Activate();
        user.Status.Should().Be(UserStatus.Active);
    }
}