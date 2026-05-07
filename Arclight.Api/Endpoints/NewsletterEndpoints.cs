using Arclight.Api.Extensions;
using Arclight.Domain.Entities;
using Arclight.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;

namespace Arclight.Api.Endpoints;

public static class NewsletterEndpoints
{
    public static void MapNewsletterEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/newsletter")

        group.MapPost("/subscribe", Subscribe);
    }

    static async Task<IResult> Subscribe(
        [FromBody] SubscribeRequest request,
        AppDbContext context,
        ClaimsPrincipal user)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@'))
            {
                return Results.BadRequest(new { message = "Ongeldig e-mailadres." });
            }

            var emailToSave = request.Email.ToLowerInvariant().Trim();

            // Check of de gebruiker is ingelogd (en gebruik jouw eigen GetUserId() extensie!)
            Guid? loggedInUserId = null;
            if (user.Identity?.IsAuthenticated == true)
            {
                loggedInUserId = user.GetUserId();
            }

            // Controleer of de abonnee al in de database staat
            var existingSubscriber = await context.Subscribers.FirstOrDefaultAsync(s => s.Email == emailToSave);

            if (existingSubscriber != null)
            {
                if (!existingSubscriber.IsActive)
                {
                    existingSubscriber.Resubscribe();

                    // Koppel alsnog aan het User account als ze nu wél zijn ingelogd
                    if (loggedInUserId.HasValue)
                    {
                        existingSubscriber.LinkToUser(loggedInUserId.Value);
                    }

                    await context.SaveChangesAsync();
                    return Results.Ok(new { message = "Welkom terug! Je bent weer ingeschreven." });
                }

                return Results.Conflict(new { message = "Dit e-mailadres is al ingeschreven." });
            }

            // Nieuwe abonnee aanmaken
            Subscriber newSubscriber = loggedInUserId.HasValue
                ? new Subscriber(emailToSave, loggedInUserId.Value)
                : new Subscriber(emailToSave);

            context.Subscribers.Add(newSubscriber);
            await context.SaveChangesAsync();

            return Results.Ok(new { message = "Bedankt voor je inschrijving!" });
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            // Vang onverwachte fouten op (bijv. database issues)
            return Results.Problem(detail: ex.Message, statusCode: 500);
        }
    }
}

// DTO voor het opvangen van de payload (deze kun je ook verplaatsen naar je DTOs mapje)
public record SubscribeRequest(string Email);