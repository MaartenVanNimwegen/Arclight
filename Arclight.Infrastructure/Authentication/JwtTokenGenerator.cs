using Arclight.Application.Interfaces;
using Arclight.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Arclight.Infrastructure.Authentication
{
    public class JwtTokenGenerator(IConfiguration configuration) : IJwtTokenGenerator
    {
        public string GenerateToken(User user)
        {
            // 1. Get the secret key
            var secretKey = configuration["JwtSettings:Secret"];

            // Small security check
            if (string.IsNullOrEmpty(secretKey))
            {
                throw new Exception("JwtSettings:Secret is not configured in appsettings.json");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 2. Add Claims to token
            var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.GivenName, user.FullName),
            // Add the user role
            new("role", user.Role.ToString())
        };

            // 3. Build the token
            var token = new JwtSecurityToken(
                issuer: configuration["JwtSettings:Issuer"],
                audience: configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(60), // Token has a lifetime of 60 minutes
                signingCredentials: creds
            );

            // 4. Return the token as string
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
