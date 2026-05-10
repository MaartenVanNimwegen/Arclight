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
    private readonly Mock<ICommentRepository> _commentRepoMock;
    private readonly Mock<IArticleRepository> _articleRepoMock;
    private readonly Mock<IJwtTokenGenerator> _tokenGeneratorMock;
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _commentRepoMock = new Mock<ICommentRepository>();
        _articleRepoMock = new Mock<IArticleRepository>();
        _tokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        _sut = new UserService(_userRepoMock.Object, _articleRepoMock.Object, _commentRepoMock.Object, _tokenGeneratorMock.Object);
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


    [Fact]
    public async Task GetAllUsersAsync_ShouldReturnUsersFromRepository()
    {
        // Arrange
        var users = new List<User>
        {
            new User("user1@test.nl", "A", "B", "hash", UserRole.User),
            new User("user2@test.nl", "C", "D", "hash", UserRole.Admin)
        };

        _userRepoMock.Setup(repo => repo.GetAllUsersAsync())
                     .ReturnsAsync(users);

        // Act
        var result = (await _sut.GetAllUsersAsync()).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(r => r.Email == "user1@test.nl" && r.Role == "User");
        result.Should().Contain(r => r.Email == "user2@test.nl" && r.Role == "Admin");
        _userRepoMock.Verify(repo => repo.GetAllUsersAsync(), Times.Once);
    }


    [Fact]
    public async Task UpdateUserRoleAsync_ShouldCallRepositoryMethod()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var newRole = UserRole.ContentCreator;

        // Act
        await _sut.UpdateUserRoleAsync(userId, newRole);

        // Assert
        _userRepoMock.Verify(repo => repo.UpdateUserRoleAsync(userId, newRole), Times.Once);
        _userRepoMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }


    [Fact]
    public async Task DeleteUserAsync_ShouldThrowKeyNotFoundException_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepoMock.Setup(repo => repo.GetByIdAsync(userId))
                     .ReturnsAsync((User?)null);

        // Act
        var act = () => _sut.DeleteUserAsync(userId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Gebruiker niet gevonden.");

        _articleRepoMock.Verify(repo => repo.Delete(It.IsAny<Article>()), Times.Never);
        _commentRepoMock.Verify(repo => repo.Delete(It.IsAny<Comment>()), Times.Never);
        _userRepoMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteUserAsync_ShouldDeleteUserAndAllRelatedContent_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var user = new User("test@test.nl", "Delete", "Me", "hash", UserRole.ContentCreator);

        var article1 = new Article("Titel 1", "slug1", "sum", "content", userId, categoryId);
        var article2 = new Article("Titel 2", "slug2", "sum", "content", userId, categoryId);
        var comment = new Comment("Reactie tekst", article1.Id, userId);

        var articles = new List<Article> { article1, article2 };
        var comments = new List<Comment> { comment };

        _userRepoMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
        _articleRepoMock.Setup(repo => repo.GetByAuthorIdAsync(userId)).ReturnsAsync(articles);
        _commentRepoMock.Setup(repo => repo.GetByUserIdAsync(userId)).ReturnsAsync(comments);

        // Act
        await _sut.DeleteUserAsync(userId);

        // Assert
        _articleRepoMock.Verify(repo => repo.Delete(article1), Times.Once);
        _articleRepoMock.Verify(repo => repo.Delete(article2), Times.Once);

        // Assert
        _commentRepoMock.Verify(repo => repo.Delete(comment), Times.Once);

        // Assert
        _userRepoMock.Verify(repo => repo.Delete(user), Times.Once);
        _userRepoMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_ShouldSuccessfullyDeleteUser_WhenUserHasNoContent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User("test@test.nl", "Delete", "Me", "hash", UserRole.User);

        _userRepoMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);

        _articleRepoMock.Setup(repo => repo.GetByAuthorIdAsync(userId)).ReturnsAsync(new List<Article>());
        _commentRepoMock.Setup(repo => repo.GetByUserIdAsync(userId)).ReturnsAsync(new List<Comment>());

        // Act
        await _sut.DeleteUserAsync(userId);

        // Assert
        _articleRepoMock.Verify(repo => repo.Delete(It.IsAny<Article>()), Times.Never);
        _commentRepoMock.Verify(repo => repo.Delete(It.IsAny<Comment>()), Times.Never);

        // Assert
        _userRepoMock.Verify(repo => repo.Delete(user), Times.Once);
        _userRepoMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }
}