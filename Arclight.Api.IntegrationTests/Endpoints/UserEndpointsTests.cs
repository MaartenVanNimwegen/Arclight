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

    // Login tests

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
}