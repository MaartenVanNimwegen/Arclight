using Arclight.Application.Interfaces;
using System.Text.RegularExpressions;

namespace Arclight.Application.Services;

public class SlugService(IArticleRepository articleRepository, ICategoryRepository categoryRepository) : ISlugService
{
    public async Task<string> GenerateUniqueArticleSlugAsync(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title cannot be empty.");
        }

        // Make a base slug, for example: "Arclight is cool" will be "arclight-is-cool"
        string baseSlug = title.ToLowerInvariant().Replace(" ", "-");

        // Removes all characters that are not letters, numbers, or hyphens
        baseSlug = Regex.Replace(baseSlug, @"[^a-z0-9\-]", "");

        if (string.IsNullOrEmpty(baseSlug))
        {
            throw new ArgumentException("Title resulted in an invalid slug after normalisation.");
        }

        // If slug exists, we add a number at the end until we find a unique one
        string currentSlug = baseSlug;
        int counter = 1;

        while (await articleRepository.SlugExistsAsync(currentSlug))
        {
            // If "arclight-is-cool" exists, we try "arclight-is-cool-1", then "arclight-is-cool-2", etc.
            currentSlug = $"{baseSlug}-{counter}";
            counter++;
        }

        return currentSlug;
    }

    public async Task<string> GenerateUniqueCategorySlugAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be empty.");
        }

        // Make a base slug, for example: "Arclight is cool" will be "arclight-is-cool"
        string baseSlug = name.ToLowerInvariant().Replace(" ", "-");

        // Removes all characters that are not letters, numbers, or hyphens
        baseSlug = Regex.Replace(baseSlug, @"[^a-z0-9\-]", "");

        if (string.IsNullOrEmpty(baseSlug))
        {
            throw new ArgumentException("Name resulted in an invalid slug after normalisation.");
        }

        // If slug exists, we add a number at the end until we find a unique one
        string currentSlug = baseSlug;
        int counter = 1;

        while (await categoryRepository.SlugExistsAsync(currentSlug))
        {
            // If "arclight-is-cool" exists, we try "arclight-is-cool-1", then "arclight-is-cool-2", etc.
            currentSlug = $"{baseSlug}-{counter}";
            counter++;
        }

        return currentSlug;
    }
}