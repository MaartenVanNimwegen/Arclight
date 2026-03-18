using System;

namespace Arclight.Application.DTOs;

public record CreateArticleRequest(
    string Title,
    string Summary,
    string Content,
    Guid CategoryId,
    bool PublishNow
);