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
public class SlugService : ISlugService
{
    private readonly ITagRepository _tagRepository;
    private readonly ILogger<SlugService> _logger;

    public SlugService(ITagRepository tagRepository, ILogger<SlugService> logger)
    {
        _tagRepository = tagRepository;
        _logger = logger;
    }

    // Parse slug string and validate each slug exists
    public async Task<Result<string[]>> ParseAndValidateSlugsAsync(
        string slugString,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(slugString))
                return Result.Success(Array.Empty<string>());

            var slugs = SlugHelper.SplitAndNormalizeSlugs(slugString);

            if (!slugs.Any())
                return Result.Success(Array.Empty<string>());

            // Validate each slug exists in the database
            foreach (var slug in slugs)
            {
                var exists = await _tagRepository.SlugExistsAsync(slug, cancellationToken);
                if (!exists)
                {
                    _logger.LogWarning("Slug '{Slug}' does not exist", slug);
                    return Result.Failure<string[]>(TagErrors.SlugNotFound(slug));
                }
            }

            _logger.LogDebug("Successfully parsed {Count} slugs", slugs.Length);
            return Result.Success(slugs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing and validating slugs: {SlugString}", slugString);
            return Result.Failure<string[]>(TagErrors.SlugParseFailed);
        }
    }

    // Get existing slugs from a string, ignoring non-existent ones
    public async Task<IEnumerable<Tag>> GetExistingSlugsAsync(string slugString, CancellationToken cancellationToken = default)
    {
        try
        {
            var allSlugs = SlugHelper.SplitAndNormalizeSlugs(slugString);
            if (!allSlugs.Any())
                return Enumerable.Empty<Tag>();

            var existingTags = await _tagRepository.GetBySlugsAsync(allSlugs, cancellationToken);
            var tags = existingTags.AsEnumerable();

            // Log any ignored slugs (using ToArray() just for the check)
            var foundSlugs = existingTags.Select(t => t.Slug).ToArray();
            var ignoredSlugs = allSlugs.Except(foundSlugs, StringComparer.OrdinalIgnoreCase).ToList();
            if (ignoredSlugs.Any())
            {
                _logger.LogInformation("Ignored {Count} non-existent slugs: {Slugs}",
                    ignoredSlugs.Count, string.Join(", ", ignoredSlugs));
            }

            return tags;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting existing slugs from: {SlugString}", slugString);
            return Enumerable.Empty<Tag>();
        }
    }

    // Convert array of slugs back to a string
    public string CombineSlugs(IEnumerable<string> slugs, bool includeSpaces = true)
    {
        return SlugHelper.CombineSlugs(slugs, includeSpaces);
    }

    public string CombineSlugs(IEnumerable<Tag> tags, bool includeSpaces = true)
    {
        return SlugHelper.CombineSlugs(tags.Select(t => t.Slug), includeSpaces);
    }

    // Update user interests from a slug string
    public async Task<Result> SyncUserInterestsFromSlugStringAsync(
    User user,
    string slugString,
    IUserInterestService userInterestService,
    CancellationToken cancellationToken = default)
    {
        try
        {
            var existingTags = await GetExistingSlugsAsync(slugString, cancellationToken);
            var slugs = existingTags.Select(t => t.Slug).ToList();

            // Get current user's tags (now using Tag entity directly)
            var currentTags = user.Interests
                .Where(ui => ui.Tag.IsActive)
                .Select(ui => ui.Tag)
                .ToList();

            var currentSlugs = currentTags.Select(t => t.Slug).ToList();

            // Find differences
            var tagsToAdd = existingTags
                .Where(t => !currentSlugs.Contains(t.Slug, StringComparer.OrdinalIgnoreCase))
                .ToList();
            var slugsToRemove = currentSlugs
                .Except(slugs, StringComparer.OrdinalIgnoreCase)
                .ToList();

            _logger.LogInformation("User {UserId} sync: Adding {AddCount} interests, Removing {RemoveCount} interests",
                user.Id, tagsToAdd.Count, slugsToRemove.Count);

            // Apply changes
            foreach (var slug in slugsToRemove)
            {
                await userInterestService.RemoveInterestFromUserAsync(user.Id, slug, cancellationToken);
            }

            foreach (var tag in tagsToAdd)
            {
                await userInterestService.AddInterestToUserAsync(user.Id, tag.Slug, cancellationToken);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing user interests from slug string for user {UserId}", user.Id);
            return Result.Failure(UserInterestErrors.InterestSyncFailed);
        }
    }
}
