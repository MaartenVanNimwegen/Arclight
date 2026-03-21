using Arclight.Application.Interfaces;
using Arclight.Domain.Enums;
using System.Text.RegularExpressions;
using System.Text;
using System.Globalization;

namespace Arclight.Application.Services;

public class SlugService(IArticleRepository articleRepository, ICategoryRepository categoryRepository) : ISlugService
{
    public async Task<string> GenerateUniqueSlugAsync(string before, SlugType type)
    {
        if (string.IsNullOrWhiteSpace(before))
            throw new ArgumentException("Input cannot be empty.");

        string baseSlug = PrepareSlug(before);

        if (string.IsNullOrEmpty(baseSlug))
            throw new ArgumentException("Input resulted in an empty slug.");

        var slugList = type switch
        {
            SlugType.Article => await articleRepository.GetExistingSlugsAsync(baseSlug),
            SlugType.Category => await categoryRepository.GetExistingSlugsAsync(baseSlug),
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };

        var existingSlugs = new HashSet<string>(slugList, StringComparer.Ordinal);

        if (!existingSlugs.Contains(baseSlug))
        {
            return baseSlug;
        }

        int counter = 1;
        string candidateSlug;

        do
        {
            candidateSlug = $"{baseSlug}-{counter}";
            counter++;
        }
        while (existingSlugs.Contains(candidateSlug));

        return candidateSlug;
    }

    private static string PrepareSlug(string input)
    {
        string normalized = input.ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        string slug = stringBuilder.ToString();

        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"-+", "-");

        return slug.Trim('-');
    }
}