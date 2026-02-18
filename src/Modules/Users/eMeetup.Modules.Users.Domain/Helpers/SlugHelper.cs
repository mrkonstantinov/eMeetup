using System;
using System.Collections.Generic;
using System.Text;
using eMeetup.Modules.Users.Domain.Interfaces.Repositories;
using eMeetup.Modules.Users.Domain.Tags;

namespace eMeetup.Modules.Users.Domain.Helpers;
public static class SlugHelper
{
    private const string SlugSeparator = ",";
    private const string SlugSeparatorWithSpace = ", ";

    // Split a string containing slugs into an array
    public static string[] SplitSlugs(string slugString)
    {
        if (string.IsNullOrWhiteSpace(slugString))
            return Array.Empty<string>();

        return slugString
            .Split(new[] { SlugSeparator, SlugSeparatorWithSpace }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToArray();
    }

    // Split and normalize slugs (convert to lowercase, remove duplicates)
    public static string[] SplitAndNormalizeSlugs(string slugString, bool uniqueOnly = true)
    {
        var slugs = SplitSlugs(slugString)
            .Select(NormalizeSlug)
            .Where(s => !string.IsNullOrWhiteSpace(s));

        return uniqueOnly
            ? slugs.Distinct(StringComparer.OrdinalIgnoreCase).ToArray()
            : slugs.ToArray();
    }

    // Validate if a slug string contains only valid slugs
    public static bool IsValidSlugString(string slugString, Func<string, bool> slugValidator)
    {
        var slugs = SplitSlugs(slugString);
        return slugs.All(slugValidator);
    }

    // Combine an array of slugs into a string
    public static string CombineSlugs(IEnumerable<string> slugs, bool includeSpaces = true)
    {
        if (slugs == null)
            return string.Empty;

        var separator = includeSpaces ? SlugSeparatorWithSpace : SlugSeparator;
        return string.Join(separator, slugs.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()));
    }

    public static string CombineSlugs(IEnumerable<Tag> tags, bool includeSpaces = true)
    {
        if (tags == null) return string.Empty;
        return CombineSlugs(tags.Select(t => t?.Slug), includeSpaces);
    }

    // Normalize a single slug
    public static string NormalizeSlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return string.Empty;

        return slug.Trim().ToLowerInvariant();
    }

    // Filter out non-existing slugs using a repository
    public static async Task<string[]> GetExistingSlugsAsync(
        string slugString,
        ITagRepository tagRepository,
        CancellationToken cancellationToken = default)
    {
        var allSlugs = SplitAndNormalizeSlugs(slugString);
        if (!allSlugs.Any())
            return Array.Empty<string>();

        var existingTags = await tagRepository.GetBySlugsAsync(allSlugs, cancellationToken);
        return existingTags.Select(t => t.Slug).ToArray();
    }

    // Parse slugs and return only existing ones as a combined string
    public static async Task<string> ParseAndCombineExistingSlugsAsync(
        string slugString,
        ITagRepository tagRepository,
        CancellationToken cancellationToken = default,
        bool includeSpaces = true)
    {
        var existingSlugs = await GetExistingSlugsAsync(slugString, tagRepository, cancellationToken);
        return CombineSlugs(existingSlugs, includeSpaces);
    }

    // Extension method for string
    public static string[] ToSlugArray(this string slugString) => SplitSlugs(slugString);

    // Extension method for IEnumerable<string>
    public static string ToSlugString(this IEnumerable<string> slugs, bool includeSpaces = true)
        => CombineSlugs(slugs, includeSpaces);
}
