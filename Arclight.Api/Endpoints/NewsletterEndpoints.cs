using Arclight.Api.Extensions;
using Arclight.Api.Filters;
using Arclight.Application.DTOs;
using Arclight.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Arclight.Api.Endpoints;

public static class NewsletterEndpoints
{
    public static IEndpointConventionBuilder MapNewsletterEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/newsletter")
            .AddEndpointFilter<UserActionLogFilter>();

        group.MapPost("/subscribe", Subscribe)
            .AddEndpointFilter<ValidationFilter<SubscribeRequest>>();

        group.MapPost("/send", SendNewsletter)
            .RequireAuthorization("RequireContentManager")
            .AddEndpointFilter<ValidationFilter<SendNewsletterRequest>>();

        return group;
    }

    static async Task<IResult> Subscribe(
        [FromBody] SubscribeRequest request,
        INewsletterService service,
        ClaimsPrincipal user)
    {
        try
        {
            Guid? loggedInUserId = null;
            if (user.Identity?.IsAuthenticated == true)
            {
                loggedInUserId = user.GetUserId();
            }

            var successMessage = await service.SubscribeAsync(request.Email, loggedInUserId);

            return Results.Ok(new { message = successMessage });
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Unauthorized();
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Results.Conflict(new { error = ex.Message });
        }
        catch (Exception)
        {
            return Results.Problem(detail: "An unexpected error occurred.", statusCode: 500);
        }
    }

    static async Task<IResult> SendNewsletter(
        SendNewsletterRequest request,
        INewsletterService service)
    {
        try
        {
            await service.SendNewsletterAsync(request.Subject, request.Body);

            return Results.Ok(new { message = "The newsletter has been sent successfully." });
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (Exception)
        {
            return Results.Problem(
                detail: "An error occurred while sending the newsletter.",
                statusCode: 500);
        }
    }
}