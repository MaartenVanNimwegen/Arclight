namespace Arclight.Application.DTOs;

public record ArticleResponse(
    Guid Id,
    string Title,
    string Slug,
    string Summary,
    string Content,
    DateTimeOffset? PublishedAt,
    string AuthorName,
    string CategoryName
);