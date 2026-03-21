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

        string slug = title.ToLowerInvariant();

        slug = Regex.Replace(slug, @"\s+", "-");

        slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");

        slug = Regex.Replace(slug, @"-+", "-").Trim('-');

        if (string.IsNullOrEmpty(slug))
        {
            throw new ArgumentException("Titel resulteert in een ongeldige slug na normalisatie.");
        }

        string baseSlug = slug;
        string currentSlug = baseSlug;
        int counter = 1;

        while (await repository.SlugExistsAsync(currentSlug))
        {
            currentSlug = $"{baseSlug}-{counter}";
            counter++;
        }

        return currentSlug;
    }
}