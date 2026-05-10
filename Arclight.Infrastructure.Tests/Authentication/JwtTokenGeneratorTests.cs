using Arclight.Domain.Entities;
using Arclight.Domain.Enums;
using Arclight.Infrastructure.Authentication;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System;
using Xunit;

namespace Arclight.Infrastructure.Tests.Authentication;

public class JwtTokenGeneratorTests
{
    [Fact]
    public void GenerateToken_ShouldReturnValidJwtToken_WithCorrectClaims()
    {
        // Arrange
        const int configuredExpiryMinutes = 60;
        var inMemorySettings = new Dictionary<string, string?>
        {
            {"JwtSettings:Secret", "SuperGeheimeSleutelDieEchtMinimaal32KaraktersLangMoetZijn!"},
            {"JwtSettings:Issuer", "TestIssuer"},
            {"JwtSettings:Audience", "TestAudience"},
            {"JwtSettings:ExpiryMinutes", configuredExpiryMinutes.ToString()}
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var generator = new JwtTokenGenerator(configuration);
        var user = new User("token@test.nl", "Token", "Test", "hash", UserRole.ContentCreator);

        // Act
        var token = generator.GenerateToken(user);

        // Assert
        token.Should().NotBeNullOrWhiteSpace();

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var subClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == JwtRegisteredClaimNames.Sub)?.Value;
        var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

        subClaim.Should().Be(user.Id.ToString());
        roleClaim.Should().Be(UserRole.ContentCreator.ToString());

        jwtToken.ValidTo.Should().BeAfter(DateTime.UtcNow.AddMinutes(configuredExpiryMinutes - 5));
        jwtToken.ValidTo.Should().BeBefore(DateTime.UtcNow.AddMinutes(configuredExpiryMinutes + 5));
    }
}
