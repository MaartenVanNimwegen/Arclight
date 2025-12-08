using Arclight.Application.DTOs;
using Arclight.Application.Interfaces;
using Arclight.Domain.Entities;
using Arclight.Domain.Enums;

namespace Arclight.Application.Services;

public interface IUserService
{
    Task<Guid> CreateUserAsync(string email, string firstName, string lastName, string password, UserRole role);
    Task<User?> GetUserAsync(Guid id);
    Task<string?> LoginAsync(LoginRequest request);
}

public class UserService(IUserRepository repository, IJwtTokenGenerator tokenGenerator) : IUserService
{
    public async Task<Guid> CreateUserAsync(string email, string firstName, string lastName, string password, UserRole role)
    {
        // Hash the password using BCrypt
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        // Create the user entity
        var user = new User(email, firstName, lastName, passwordHash, role);

        // Save the user to the repository
        await repository.AddAsync(user);
        await repository.SaveChangesAsync();

        // Return the newly created user's Id
        return user.Id;
    }

    public async Task<User?> GetUserAsync(Guid id)
    {
        return await repository.GetByIdAsync(id);
    }

    public async Task<string?> LoginAsync(LoginRequest request)
    {
        var user = await repository.GetByEmailAsync(request.Email);

        // Check 1: Does the user exist?
        if (user is null)
        {
            return null;
        }

        // Check 2: Is the password correct?
        bool isValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

        if (!isValid)
        {
            return null;
        }

        // Check 3: Everything is valid, generate a token
        return tokenGenerator.GenerateToken(user);
    }
}