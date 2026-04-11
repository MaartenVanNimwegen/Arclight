using Arclight.Api.Extensions;
using Arclight.Application.Interfaces;
using System.Security.Claims;

public static class CommentEndpoints
{
    public static void MapCommentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/articles/{articleId:guid}/comments");

        group.MapPost("", CreateComment)
            .RequireAuthorization();

        group.MapGet("", GetCommentsByArticleId)
            .AllowAnonymous();

        group.MapDelete("{commentId:guid}", DeleteCommentById)
            .RequireAuthorization();
    }

    static async Task<IResult> GetCommentsByArticleId(Guid articleId, ICommentService service)
    {
        var comments = await service.GetCommentsByArticleIdAsync(articleId);
        return Results.Ok(comments);
    }

    static async Task<IResult> DeleteCommentById(
    Guid articleId,
    Guid commentId,
    ICommentService service,
    ClaimsPrincipal user)
    {
        var userId = user.GetUserId();
        var role = user.GetUserRole();

        try
        {
            var success = await service.DeleteCommentAsync(commentId, userId, role);
            return success ? Results.Ok() : Results.BadRequest();
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Forbid();
        }
    }

    static async Task<IResult> CreateComment(
    Guid articleId,
    CreateCommentRequest request,
    ICommentService service,
    ClaimsPrincipal user)
    {
        var userId = user.GetUserId();
        try
        {
            var response = await service.AddCommentAsync(articleId, userId, request);
            return Results.Created($"/articles/{articleId}/comments/{response.Id}", response.Id);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }
}