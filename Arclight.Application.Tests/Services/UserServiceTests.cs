using Arclight.Application.DTOs;
using Arclight.Application.Interfaces;
using Arclight.Application.Services;
using Arclight.Domain.Entities;
using Arclight.Domain.Enums;
using FluentAssertions;
using Moq;

namespace Arclight.Application.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IJwtTokenGenerator> _tokenGeneratorMock;
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _tokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        _sut = new UserService(_userRepoMock.Object, _tokenGeneratorMock.Object);
    }

    // Create tests

    [Fact]
    public async Task CreateUserAsync_ShouldThrowInvalidOperationException_WhenEmailAlreadyExists()
    {
        // Arrange
        var email = "bestaat@al.nl";
        var existingUser = new User(email, "F", "L", "H", UserRole.User);

        _userRepoMock.Setup(repo => repo.GetByEmailAsync(email))
                     .ReturnsAsync(existingUser);

        // Act
        Func<Task> act = async () => await _sut.CreateUserAsync(email, "Jan", "Jansen", "Wachtwoord123!", UserRole.User);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("Email address is already in use.");

        _userRepoMock.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Never);
        _userRepoMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateUserAsync_ShouldHashPasswordAndSave_WhenDataIsValid()
    {
        // Arrange
        var email = "nieuw@test.nl";
        var plainPassword = "SuperGeheimWachtwoord123!";

        _userRepoMock.Setup(repo => repo.GetByEmailAsync(email))
                     .ReturnsAsync((User?)null); // Email is vrij!

        // Act
        var resultId = await _sut.CreateUserAsync(email, "Jan", "Jansen", plainPassword, UserRole.Admin);

        // Assert
        resultId.Should().NotBeEmpty();

        _userRepoMock.Verify(repo => repo.AddAsync(It.Is<User>(u =>
            u.Email == email &&
            u.PasswordHash != plainPassword &&
            u.PasswordHash.StartsWith("$2")
        )), Times.Once);

        _userRepoMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    // Get tests

    [Fact]
    public async Task GetUserAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        _userRepoMock.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>()))
                     .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.GetUserAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User("test@test.nl", "F", "L", "H", UserRole.Admin);
        _userRepoMock.Setup(repo => repo.GetByIdAsync(userId))
                     .ReturnsAsync(user);

        // Act
        var result = await _sut.GetUserAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("test@test.nl");
    }

    // Login tests

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenEmailDoesNotExist()
    {
        // Arrange
        var request = new LoginRequest("fout@test.nl", "wachtwoord");
        _userRepoMock.Setup(repo => repo.GetByEmailAsync(request.Email))
                     .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenPasswordIsIncorrect()
    {
        // Arrange
        var request = new LoginRequest("goed@test.nl", "FoutWachtwoord!");

        var correctHash = BCrypt.Net.BCrypt.HashPassword("GoedWachtwoord!");
        var user = new User(request.Email, "F", "L", correctHash, UserRole.Admin);

        _userRepoMock.Setup(repo => repo.GetByEmailAsync(request.Email))
                     .ReturnsAsync(user);

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreCorrect()
    {
        // Arrange
        var plainPassword = "GoedWachtwoord!";
        var request = new LoginRequest("goed@test.nl", plainPassword);

        var correctHash = BCrypt.Net.BCrypt.HashPassword(plainPassword);
        var user = new User(request.Email, "F", "L", correctHash, UserRole.Admin);

        _userRepoMock.Setup(repo => repo.GetByEmailAsync(request.Email))
                     .ReturnsAsync(user);

        var expectedToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...";
        _tokenGeneratorMock.Setup(tg => tg.GenerateToken(user))
                           .Returns(expectedToken);

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        result.Should().Be(expectedToken);
    }
}