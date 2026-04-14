using Arclight.Application.DTOs;
using Arclight.Application.Interfaces;
using Arclight.Domain.Entities;
using Arclight.Domain.Enums;

namespace Arclight.Application.Services;

public class ArticleService(IArticleRepository articleRepository, IUserRepository userRepository, ISlugService slugService) : IArticleService
{
    public async Task<IEnumerable<ArticleResponse>> GetAllPublishedArticlesAsync()
    {
        IEnumerable<Article> articles = await articleRepository.GetAllPublishedAsync();

        return articles.Select(a => new ArticleResponse(
            a.Id,
            a.Title,
            a.Slug,
            a.Summary,
            a.Content,
            a.PublishedAt,
            a.Author?.FullName ?? "Unknown author",
            a.Category?.Name ?? "No category"
        ));
    }

    public async Task<ArticleResponse?> GetArticleBySlugAsync(string slug)
    {
        Article? article = await articleRepository.GetBySlugAsync(slug);

        if (article is null || !article.IsPublished)
        {
            return null;
        }

        return new ArticleResponse(
            article.Id,
            article.Title,
            article.Slug,
            article.Summary,
            article.Content,
            article.PublishedAt,
            article.Author?.FullName ?? "Unknown author",
            article.Category?.Name ?? "No category"
        );
    }

    public async Task<Guid> CreateArticleAsync(CreateArticleRequest request, Guid authorId)
    {
        if (await userRepository.GetByIdAsync(authorId) is null)    
        {
            throw new KeyNotFoundException("The given author is not found.");
        }

        string uniqueSlug = await slugService.GenerateUniqueSlugAsync(request.Title, SlugType.Article);

        var article = new Article(
            request.Title,
            uniqueSlug,
            request.Summary,
            request.Content,
            authorId,
            request.CategoryId
        );

        if (request.PublishNow)
        {
            article.Publish();
        }

        await articleRepository.AddAsync(article);
        await articleRepository.SaveChangesAsync();

        return article.Id;
    }

    public async Task<bool> UpdateArticleAsync(Guid id, UpdateArticleRequest request)
    {
        var article = await articleRepository.GetByIdAsync(id);

        if (article is null)
        {
            return false;
        }

        article.UpdateContent(request.Title, article.Slug, request.Summary, request.Content, request.CategoryId);

        await articleRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> PublishArticleAsync(Guid id)
    {
        var article = await articleRepository.GetByIdAsync(id);

        if (article is null)
        {
            return false;
        }

        article.Publish();

        await articleRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteArticleAsync(Guid id)
    {
        var article = await articleRepository.GetByIdAsync(id);

        if (article is null)
        {
            return false;
        }

        articleRepository.Delete(article);
        await articleRepository.SaveChangesAsync();
        return true;
    }
}
