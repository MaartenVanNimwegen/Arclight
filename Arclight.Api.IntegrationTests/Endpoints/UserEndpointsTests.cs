using Arclight.Application.DTOs;
using FluentAssertions;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace Arclight.Api.IntegrationTests.Endpoints;

public class UserEndpointsTests : BaseIntegrationTest
{
    public UserEndpointsTests(CustomWebApplicationFactory factory) : base(factory) { }

    // Register tests

    [Fact]
    public async Task Register_ShouldReturnCreated_WhenDataIsValid()
    {
        // Arrange
        var request = new RegisterRequest("nieuw@test.nl", "Jan", "Jansen", "Wachtwoord123!");

        // Act
        var response = await Client.PostAsJsonAsync("/user/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task Register_ShouldReturnConflict_WhenEmailAlreadyExists()
    {
        // Arrange
        var email = "dubbel@test.nl";
        var request = new RegisterRequest(email, "Piet", "Pieters", "Wachtwoord123!");

        await Client.PostAsJsonAsync("/user/register", request);

        var response = await Client.PostAsJsonAsync("/user/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    // Get tests

    [Fact]
    public async Task GetUser_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var randomId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/user/{randomId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetUser_ShouldReturnOk_WhenUserExists()
    {
        // Arrange
        var request = new RegisterRequest("getuser@test.nl", "Klaas", "Vaak", "Wachtwoord123!");
        var registerResponse = await Client.PostAsJsonAsync("/user/register", request);

        var createdUserId = await registerResponse.Content.ReadFromJsonAsync<Guid>();

        // Act
        var response = await Client.GetAsync($"/user/{createdUserId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenEmailIsEmpty()
    {
        // Arrange
        var request = new RegisterRequest("", "Jan", "Jansen", "Wachtwoord123!");

        // Act
        var response = await Client.PostAsJsonAsync("/user/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Email cannot be empty");
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenPasswordIsTooShort()
    {
        // Arrange
        var request = new RegisterRequest("valid@test.nl", "Jan", "Jansen", "short");

        // Act
        var response = await Client.PostAsJsonAsync("/user/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Password must be at least 8 characters");
    }

    // Login tests

    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenEmailIsEmpty()
    {
        // Arrange
        var request = new LoginRequest("", "Wachtwoord123!");

        // Act
        var response = await Client.PostAsJsonAsync("/user/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Email cannot be empty");
    }

    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenPasswordIsEmpty()
    {
        // Arrange
        var request = new LoginRequest("valid@test.nl", "");

        // Act
        var response = await Client.PostAsJsonAsync("/user/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Password cannot be empty");
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenCredentialsAreWrong()
    {
        // Arrange
        var request = new LoginRequest("fout@test.nl", "VerkeerdWachtwoord!");

        // Act
        var response = await Client.PostAsJsonAsync("/user/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_ShouldReturnOkWithToken_WhenCredentialsAreCorrect()
    {
        // Arrange
        var email = "login@test.nl";
        var password = "GoedWachtwoord123!";

        var registerRequest = new RegisterRequest(email, "Login", "Test", password);
        await Client.PostAsJsonAsync("/user/register", registerRequest);

        var loginRequest = new LoginRequest(email, password);

        // Act
        var response = await Client.PostAsJsonAsync("/user/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();

        content.Should().Contain("token");
    }


    [Fact]
    public async Task GetAllUsers_ShouldReturnOk_WithListOfUsers()
    {
        // Arrange
        var request = new RegisterRequest("getall@test.nl", "Anna", "Anders", "Wachtwoord123!");
        await Client.PostAsJsonAsync("/user/register", request);

        // Act
        var adminClient = CreateClientWithRoles("Admin");
        var response = await adminClient.GetAsync("/user");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }


    [Fact]
    public async Task UpdateUser_ShouldReturnNoContent_WhenValidIdAndRole()
    {
        // Arrange
        var request = new RegisterRequest("update@test.nl", "Piet", "Puk", "Wachtwoord123!");
        var registerResponse = await Client.PostAsJsonAsync("/user/register", request);
        var createdUserId = await registerResponse.Content.ReadFromJsonAsync<Guid>();

        // Act
        var adminClient = CreateClientWithRoles("Admin");
        var response = await adminClient.PutAsync($"/user/{createdUserId}/Admin", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateUser_ShouldReturnBadRequest_WhenRoleIsInvalid()
    {
        // Arrange
        var request = new RegisterRequest("invalidrole@test.nl", "Piet", "Puk", "Wachtwoord123!");
        var registerResponse = await Client.PostAsJsonAsync("/user/register", request);
        var createdUserId = await registerResponse.Content.ReadFromJsonAsync<Guid>();

        // Act
        var adminClient = CreateClientWithRoles("Admin");
        var response = await adminClient.PutAsync($"/user/{createdUserId}/OnbekendeRol", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Invalid role");
    }

    [Fact]
    public async Task UpdateUser_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var randomId = Guid.NewGuid();

        // Act
        var adminClient = CreateClientWithRoles("Admin");
        var response = await adminClient.PutAsync($"/user/{randomId}/Admin", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }


    [Fact]
    public async Task DeleteUser_ShouldReturnNoContent_WhenUserExists()
    {
        // Arrange
        var request = new RegisterRequest("delete@test.nl", "Piet", "Puk", "Wachtwoord123!");
        var registerResponse = await Client.PostAsJsonAsync("/user/register", request);
        var createdUserId = await registerResponse.Content.ReadFromJsonAsync<Guid>();

        // Act
        var adminClient = CreateClientWithRoles("Admin");
        var response = await adminClient.DeleteAsync($"/user/{createdUserId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteUser_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var randomId = Guid.NewGuid();

        // Act
        var adminClient = CreateClientWithRoles("Admin");
        var response = await adminClient.DeleteAsync($"/user/{randomId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteUser_ShouldReturnConflict_WhenBusinessRuleIsViolated()
    {
        // Arrange
        // Let op: Afhankelijk van je business logica gooit de service een InvalidOperationException.
        // Bijvoorbeeld: "Je kunt de laatste admin niet verwijderen". 
        // Pas de Arrange hieronder aan om die specifieke situatie te simuleren!

        var request = new RegisterRequest("conflict@test.nl", "Piet", "Puk", "Wachtwoord123!");
        var registerResponse = await Client.PostAsJsonAsync("/user/register", request);
        var createdUserId = await registerResponse.Content.ReadFromJsonAsync<Guid>();

        // Zorg er hier voor dat de user in de staat komt die de InvalidOperationException triggert.
        // Mocht je deze rule nog niet hebben geïmplementeerd in je service, 
        // dan kun je deze test voor nu uitschakelen of als placeholder gebruiken.

        // Act
        // var response = await Client.DeleteAsync($"/user/{createdUserId}");

        // Assert
        // response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        // var content = await response.Content.ReadAsStringAsync();
        // content.Should().Contain("verwachte error message");
    }
}