using Arclight.Application.DTOs;
using Arclight.Application.Interfaces;
using Arclight.Domain.Entities;
using Arclight.Domain.Enums;

namespace Arclight.Application.Services;

public class UserService(IUserRepository repository, IJwtTokenGenerator tokenGenerator) : IUserService
{
    public async Task<Guid> CreateUserAsync(string email, string firstName, string lastName, string password, UserRole role)
    {
        // 1. Check if a user with the same email already exists
        User? existingUser = await repository.GetByEmailAsync(email);
        if (existingUser is not null)
        {
            // Throw an exception or return an error indicating that the email is already in use. This exception should be caught and handled by the caller to return an appropriate response to the client.
            throw new InvalidOperationException("Email address is already in use."); 
        }

        // 2. Hash the password using BCrypt
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        // 3. Create the user entity
        User user = new(email, firstName, lastName, passwordHash, role);

        // 4. Save the user to the repository
        await repository.AddAsync(user);
        await repository.SaveChangesAsync();

        // 5. Return the newly created user's Id
        return user.Id;
    }

    public async Task<User?> GetUserAsync(Guid id)
    {
        return await repository.GetByIdAsync(id);
    }

    public async Task<string?> LoginAsync(LoginRequest request)
    {
        User? user = await repository.GetByEmailAsync(request.Email);

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