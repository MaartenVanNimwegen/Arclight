using Arclight.Api.Extensions;
using Arclight.Api.Filters;
using Arclight.Application.DTOs;
using Arclight.Application.Interfaces;
using Arclight.Domain.Entities;
using System;
using System.Security.Claims;

namespace Arclight.Api.Endpoints;

public static class ArticleEndpoints
{
    public static void MapArticleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/articles");

        group.MapGet("/", GetAllArticles);
        group.MapGet("/{slug}", GetArticleBySlug);

        // Secured endpoints
        group.MapPost("/", CreateArticle)
            .RequireAuthorization("RequireContentManager")
            .AddEndpointFilter<ValidationFilter<CreateArticleRequest>>();

        group.MapPut("/{id:guid}", UpdateArticle)
            .RequireAuthorization("RequireContentManager")
            .AddEndpointFilter<ValidationFilter<UpdateArticleRequest>>();
        group.MapDelete("/{id:guid}", DeleteArticle).RequireAuthorization("RequireContentManager");
    }

    static async Task<IResult> GetAllArticles(IArticleService service)
    {
        // Returns all the published articles
        IEnumerable<ArticleResponse> articles = await service.GetAllPublishedArticlesAsync();
        return Results.Ok(articles);
    }

    static async Task<IResult> GetArticleBySlug(string slug, IArticleService service)
    {
        // Searches for an article with the specified slug
        ArticleResponse? article = await service.GetArticleBySlugAsync(slug);

        return article is not null
            ? Results.Ok(article)
            : Results.NotFound(new { message = "Article not found." });
    }

    static async Task<IResult> CreateArticle(
    CreateArticleRequest request,
    IArticleService service,
    ClaimsPrincipal user)
    { 
        try
        {
            var authorId = user.GetUserId();

            var id = await service.CreateArticleAsync(request, authorId);
            return Results.Created($"/articles/{id}", id);
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Unauthorized();
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    static async Task<IResult> UpdateArticle(Guid id, UpdateArticleRequest request, IArticleService service)
    {
        var success = await service.UpdateArticleAsync(id, request);

        return success
            ? Results.NoContent()
            : Results.NotFound(new { message = "Article not found." });
    }

    static async Task<IResult> DeleteArticle(Guid id, IArticleService service)
    {
        var success = await service.DeleteArticleAsync(id);

        return success
            ? Results.NoContent()
            : Results.NotFound(new { message = "Article not found." });
    }
}