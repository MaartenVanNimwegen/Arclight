using Arclight.Application.DTOs;
using System.Security.Claims;

namespace Arclight.Application.Interfaces;

public interface IArticleService
{
    Task<IEnumerable<ArticleResponse>> GetAllPublishedArticlesAsync();
    Task<ArticleResponse?> GetArticleBySlugAsync(string slug);
    Task<Guid> CreateArticleAsync(CreateArticleRequest request, Guid authorId);
    Task<bool> PublishArticleAsync(Guid id);
    Task<bool> UpdateArticleAsync(Guid id, UpdateArticleRequest request);
    Task<bool> DeleteArticleAsync(Guid id);
    Task<IEnumerable<ArticleResponse>> GetAllUnpublishedArticlesAsync(ClaimsPrincipal user);
}