namespace Arclight.Application.DTOs;

public record RegisterRequest(
    string Email, 
    string FirstName, 
    string LastName, 
    string Password
);