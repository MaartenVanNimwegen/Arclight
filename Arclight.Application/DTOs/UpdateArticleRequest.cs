using System;

namespace Arclight.Application.DTOs;

public record UpdateArticleRequest(
    string Title,
    string Summary,
    string Content,
    Guid CategoryId
);