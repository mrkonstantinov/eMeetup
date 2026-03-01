using eMeetup.Common.Domain;
using eMeetup.Modules.Users.Domain.Errors;
using eMeetup.Modules.Users.Domain.Helpers;
using eMeetup.Modules.Users.Domain.Interfaces.Repositories;
using eMeetup.Modules.Users.Domain.Interfaces.Services;
using eMeetup.Modules.Users.Domain.Tags;
using eMeetup.Modules.Users.Domain.UserInterests;
using eMeetup.Modules.Users.Domain.Users;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;

namespace eMeetup.Modules.Users.Infrastructure.Services;
public class TagService : ITagService
{
    private readonly ITagRepository _tagRepository;
    private readonly ILogger<TagService> _logger;

    public TagService(ITagRepository tagRepository, ILogger<TagService> logger)
    {
        _tagRepository = tagRepository;
        _logger = logger;
    }

    // Parse tag string and validate each tag exists
    public async Task<Result<string[]>> ParseAndValidateTagsAsync(
        string tagString,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(tagString))
                return Result.Success(Array.Empty<string>());

            var tags = TagHelper.SplitAndNormalizeTags(tagString);

            if (!tags.Any())
                return Result.Success(Array.Empty<string>());

            // Validate each tag exists in the database
            foreach (var tag in tags)
            {
                var exists = await _tagRepository.TagExistsAsync(tag, cancellationToken);
                if (!exists)
                {
                    _logger.LogWarning("Tag '{Tag}' does not exist", tag);
                    return Result.Failure<string[]>(TagErrors.TagNotFound(tag));
                }
            }

            _logger.LogDebug("Successfully parsed {Count} tags", tags.Length);
            return Result.Success(tags);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing and validating tags: {TagString}", tagString);
            return Result.Failure<string[]>(TagErrors.TagParseFailed);
        }
    }

    // Get existing tags from a string, ignoring non-existent ones
    public async Task<IEnumerable<Tag>> GetExistingTagsAsync(string tagString, CancellationToken cancellationToken = default)
    {
        try
        {
            var allTags = TagHelper.SplitAndNormalizeTags(tagString);
            if (!allTags.Any())
                return Enumerable.Empty<Tag>();

            var existingTags = await _tagRepository.GetByTagsAsync(allTags, cancellationToken);
            var tags = existingTags.AsEnumerable();

            // Log any ignored tags (using ToArray() just for the check)
            var foundTags = existingTags.Select(t => t.Name).ToArray();
            var ignoredTags = allTags.Except(foundTags, StringComparer.OrdinalIgnoreCase).ToList();
            if (ignoredTags.Any())
            {
                _logger.LogInformation("Ignored {Count} non-existent tags: {Tags}",
                    ignoredTags.Count, string.Join(", ", ignoredTags));
            }

            return tags;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting existing tags from: {TagString}", tagString);
            return Enumerable.Empty<Tag>();
        }
    }

    // Convert array of tags back to a string
    public string CombineTags(IEnumerable<string> tags, bool includeSpaces = true)
    {
        return TagHelper.CombineTags(tags, includeSpaces);
    }

    public string CombineTags(IEnumerable<Tag> tags, bool includeSpaces = true)
    {
        return TagHelper.CombineTags(tags.Select(t => t.Name), includeSpaces);
    }

    // Update user interests from a tag string
    public async Task<Result> SyncUserInterestsFromTagStringAsync(
    User user,
    string tagString,
    IUserInterestService userInterestService,
    CancellationToken cancellationToken = default)
    {
        try
        {
            var existingTags = await GetExistingTagsAsync(tagString, cancellationToken);
            var tags = existingTags.Select(t => t.Name).ToList();

            // Get current user's tags (now using Tag entity directly)
            var currentTags = user.Interests
                .Where(ui => ui.Tag.IsActive)
                .Select(ui => ui.Tag)
                .ToList();

            var currentSlugs = currentTags.Select(t => t.Name).ToList();

            // Find differences
            var tagsToAdd = existingTags
                .Where(t => !currentSlugs.Contains(t.Name, StringComparer.OrdinalIgnoreCase))
                .ToList();
            var tagsToRemove = currentSlugs
                .Except(tags, StringComparer.OrdinalIgnoreCase)
                .ToList();

            _logger.LogInformation("User {UserId} sync: Adding {AddCount} interests, Removing {RemoveCount} interests",
                user.Id, tagsToAdd.Count, tagsToRemove.Count);

            // Apply changes
            foreach (var tag in tagsToRemove)
            {
                await userInterestService.RemoveInterestFromUserAsync(user.Id, tag, cancellationToken);
            }

            foreach (var tag in tagsToAdd)
            {
                await userInterestService.AddInterestToUserAsync(user.Id, tag.Name, cancellationToken);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing user interests from tag string for user {UserId}", user.Id);
            return Result.Failure(UserInterestErrors.InterestSyncFailed);
        }
    }
}
