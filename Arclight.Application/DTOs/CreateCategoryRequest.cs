namespace Arclight.Application.DTOs;

public record CreateCategoryRequest(
    string Name,
    string Description
);