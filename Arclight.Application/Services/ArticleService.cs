using Arclight.Application.DTOs;
using Arclight.Application.Interfaces;
using Arclight.Domain.Entities;

namespace Arclight.Application.Services;

public class ArticleService(IArticleRepository repository, ISlugService slugService) : IArticleService
{
    public async Task<IEnumerable<ArticleResponse>> GetAllPublishedArticlesAsync()
    {
        IEnumerable<Article> articles = await repository.GetAllPublishedAsync();

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
        Article? article = await repository.GetBySlugAsync(slug);

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
        // Make a unique and URL-friendly slug based on the title
        string uniqueSlug = await slugService.GenerateUniqueSlugAsync(request.Title);

        // Create the article entity
        var article = new Article(
            request.Title,
            uniqueSlug,
            request.Summary,
            request.Content,
            authorId,
            request.CategoryId
        );

        // if publishNow is true, publish the article immediately, else make it a draft
        if ( request.PublishNow)
        {
            article.Publish();
        }

        // Save the article to the database
        await repository.AddAsync(article);
        await repository.SaveChangesAsync();

        return article.Id;
    }

    public async Task<bool> UpdateArticleAsync(Guid id, UpdateArticleRequest request)
    {
        var article = await repository.GetByIdAsync(id);

        if (article is null)
        {
            return false;
        }

        article.UpdateContent(request.Title, article.Slug, request.Summary, request.Content, request.CategoryId);

        await repository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteArticleAsync(Guid id)
    {
        var article = await repository.GetByIdAsync(id);

        if (article is null)
        {
            return false;
        }

        repository.Delete(article);
        await repository.SaveChangesAsync();
        return true;
    }
}