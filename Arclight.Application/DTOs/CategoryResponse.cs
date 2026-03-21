namespace Arclight.Application.DTOs;

public record CategoryResponse(
    Guid Id,
    string Name,
    string Slug,
    string? Description
);