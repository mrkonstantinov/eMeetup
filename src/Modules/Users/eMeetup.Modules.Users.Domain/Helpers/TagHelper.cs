using eMeetup.Modules.Users.Domain.Interfaces.Repositories;
using eMeetup.Modules.Users.Domain.Tags;

namespace eMeetup.Modules.Users.Domain.Helpers;
public static class TagHelper
{
    private const string TagSeparator = ",";
    private const string TagSeparatorWithSpace = ", ";

    // Split a string containing tags into an array
    public static string[] SplitTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            return Array.Empty<string>();

        return tag
            .Split(new[] { TagSeparator, TagSeparatorWithSpace }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToArray();
    }

    // Split and normalize tags (convert to lowercase, remove duplicates)
    public static string[] SplitAndNormalizeTags(string tag, bool uniqueOnly = true)
    {
        var tags = SplitTag(tag)
            .Select(NormalizeTag)
            .Where(s => !string.IsNullOrWhiteSpace(s));

        return uniqueOnly
            ? tags.Distinct(StringComparer.OrdinalIgnoreCase).ToArray()
            : tags.ToArray();
    }

    // Validate if a tag string contains only valid tags
    public static bool IsValidTagString(string tag, Func<string, bool> tagValidator)
    {
        var tags = SplitTag(tag);
        return tags.All(tagValidator);
    }

    // Combine an array of tags into a string
    public static string CombineTags(IEnumerable<string> tags, bool includeSpaces = true)
    {
        if (tags == null)
            return string.Empty;

        var separator = includeSpaces ? TagSeparatorWithSpace : TagSeparator;
        return string.Join(separator, tags.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()));
    }

    public static string CombineTags(IEnumerable<Tag> tags, bool includeSpaces = true)
    {
        if (tags == null) return string.Empty;
        return CombineTags(tags.Select(t => t?.Name), includeSpaces);
    }

    // Normalize a single tag
    public static string NormalizeTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            return string.Empty;

        return tag.Trim().ToLowerInvariant();
    }

    // Filter out non-existing tags using a repository
    public static async Task<string[]> GetExistingTagsAsync(
        string tag,
        ITagRepository tagRepository,
        CancellationToken cancellationToken = default)
    {
        var allTags = SplitAndNormalizeTags(tag);
        if (!allTags.Any())
            return Array.Empty<string>();

        var existingTags = await tagRepository.GetByTagsAsync(allTags, cancellationToken);
        return existingTags.Select(t => t.Name).ToArray();
    }

    // Parse tags and return only existing ones as a combined string
    public static async Task<string> ParseAndCombineExistingTagsAsync(
        string tag,
        ITagRepository tagRepository,
        CancellationToken cancellationToken = default,
        bool includeSpaces = true)
    {
        var existingTags = await GetExistingTagsAsync(tag, tagRepository, cancellationToken);
        return CombineTags(existingTags, includeSpaces);
    }

    // Extension method for string
    public static string[] ToTagArray(this string tagString) => SplitTag(tagString);

    // Extension method for IEnumerable<string>
    public static string ToTagString(this IEnumerable<string> tags, bool includeSpaces = true)
        => CombineTags(tags, includeSpaces);
}
