using Arclight.Api.Extensions;
using Arclight.Api.Filters;
using Arclight.Application.DTOs;
using Arclight.Application.Interfaces;
using Arclight.Domain.Enums;
using System.Security.Claims;

namespace Arclight.Api.Endpoints;
public static class CommentEndpoints
{
    public static IEndpointConventionBuilder MapCommentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/articles/{articleId:guid}/comments");

        group.MapPost("", CreateComment)
            .RequireAuthorization()
            .AddEndpointFilter<ValidationFilter<CreateCommentRequest>>();

        group.MapGet("", GetCommentsByArticleId)
            .AllowAnonymous();

        group.MapDelete("{commentId:guid}", DeleteCommentById)
            .RequireAuthorization();

        var adminGroup = app.MapGroup("/admin/comments");

        adminGroup.MapGet("", GetAllComments)
            .RequireAuthorization("RequireContentManager");

        adminGroup.MapDelete("{commentId:guid}", DeleteCommentByIdAdmin)
            .RequireAuthorization("RequireContentManager");

        return group;
    }

    static async Task<IResult> GetCommentsByArticleId(Guid articleId, ICommentService service)
    {
        var comments = await service.GetCommentsByArticleIdAsync(articleId);
        return Results.Ok(comments);
    }

    static async Task<IResult> GetAllComments(ICommentService service)
    {
        var comments = await service.GetAllCommentsAsync();
        return Results.Ok(comments);
    }

    static Task<IResult> DeleteCommentByIdAdmin(Guid commentId, ICommentService service, ClaimsPrincipal user)
        => ExecuteDeleteAsync(user, (userId, role) => service.DeleteCommentAsync(commentId, userId, role));

    static Task<IResult> DeleteCommentById(
    Guid articleId,
    Guid commentId,
    ICommentService service,
    ClaimsPrincipal user)
        => ExecuteDeleteAsync(user, (userId, role) => service.DeleteCommentAsync(articleId, commentId, userId, role));

    static async Task<IResult> ExecuteDeleteAsync(ClaimsPrincipal user, Func<Guid, UserRole, Task<bool>> deleteAction)
    {
        Guid userId;
        try
        {
            userId = user.GetUserId();
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Unauthorized();
        }

        var role = user.GetUserRole();
        try
        {
            var success = await deleteAction(userId, role);
            return success ? Results.Ok() : Results.NotFound();
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
        Guid userId;
        try
        {
            userId = user.GetUserId();
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Unauthorized();
        }

        try
        {
            var response = await service.AddCommentAsync(articleId, userId, request);
            return Results.Created($"/articles/{articleId}/comments/{response.Id}", response.Id);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Forbid();
        }
    }
}
