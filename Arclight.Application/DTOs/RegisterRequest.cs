namespace Arclight.Application.DTOs;

public record RegisterRequest(string email, string firstName, string lastName, string password);