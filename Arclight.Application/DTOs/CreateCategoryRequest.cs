using System;

namespace Arclight.Application.DTOs;

public record CreateCategoryRequest(
    string Name,
    string Slug,
    string Description
);