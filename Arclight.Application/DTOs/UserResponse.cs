namespace Arclight.Application.DTOs;

public record UserResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    string Status
);
