using Arclight.Application.Interfaces;
using System.Text.RegularExpressions;

namespace Arclight.Application.Services;

public class SlugService(IArticleRepository repository) : ISlugService
{
    public async Task<string> GenerateUniqueSlugAsync(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Titel mag niet leeg zijn.");
        }

        // Make a base slug, for example: "Arclight is cool" will be "arclight-is-cool"
        string baseSlug = title.ToLowerInvariant().Replace(" ", "-");

        // Removes all characters that are not letters, numbers, or hyphens
        baseSlug = Regex.Replace(baseSlug, @"[^a-z0-9\-]", "");

        // If slug exists, we add a number at the end until we find a unique one
        string currentSlug = baseSlug;
        int counter = 1;

        while (await repository.SlugExistsAsync(currentSlug))
        {
            // If "arclight-is-cool" exists, we try "arclight-is-cool-1", then "arclight-is-cool-2", etc.
            currentSlug = $"{baseSlug}-{counter}";
            counter++;
        }

        return currentSlug;
    }
}