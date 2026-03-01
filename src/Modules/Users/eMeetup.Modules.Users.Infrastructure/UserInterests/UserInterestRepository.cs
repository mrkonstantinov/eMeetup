using eMeetup.Modules.Users.Domain.Interfaces.Repositories;
using eMeetup.Modules.Users.Domain.Tags;
using eMeetup.Modules.Users.Domain.UserInterests;
using eMeetup.Modules.Users.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace eMeetup.Modules.Users.Infrastructure.UserInterests;

public class UserInterestRepository(UsersDbContext context, ITagRepository tagRepository,  ILogger<UserInterestRepository> logger) : IUserInterestRepository
{
    private readonly UsersDbContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly ILogger<UserInterestRepository> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ITagRepository _tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(_tagRepository));


    public async Task<UserInterest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.UserInterests
            .Include(ui => ui.Tag)
            .Include(ui => ui.User)
            .FirstOrDefaultAsync(ui => ui.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<UserInterest>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserInterests
            .Include(ui => ui.Tag)
            .Where(ui => ui.UserId == userId)
            .OrderByDescending(ui => ui.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserInterest>> GetByTagIdAsync(Guid tagId, CancellationToken cancellationToken = default)
    {
        return await _context.UserInterests
            .Include(ui => ui.User)
            .Where(ui => ui.TagId == tagId)
            .OrderByDescending(ui => ui.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserInterest?> GetByUserAndTagIdAsync(Guid userId, Guid tagId, CancellationToken cancellationToken = default)
    {
        return await _context.UserInterests
            .Include(ui => ui.Tag)
            .FirstOrDefaultAsync(ui => ui.UserId == userId && ui.TagId == tagId, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid userId, Guid tagId, CancellationToken cancellationToken = default)
    {
        return await _context.UserInterests
            .AnyAsync(ui => ui.UserId == userId && ui.TagId == tagId, cancellationToken);
    }

    public async Task<UserInterest?> AddAsync(Guid userId, string tagName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tagName))
            return null;

        // Get or create tag
        var tag = await GetOrCreateTagAsync(tagName, cancellationToken);
        if (tag == null)
            return null;

        // Check if already exists
        var exists = await ExistsAsync(userId, tag.Id, cancellationToken);
        if (exists)
            return await GetByUserAndTagIdAsync(userId, tag.Id, cancellationToken);

        // Create user interest
        var userInterest = UserInterest.Create(userId, tag.Id).Value;

        await _context.UserInterests.AddAsync(userInterest, cancellationToken);

        // Increment tag usage count
        tag.IncrementUsage();
        _context.Tags.Update(tag);

        await _context.SaveChangesAsync(cancellationToken);

        // Load navigation property
        await _context.Entry(userInterest)
            .Reference(ui => ui.Tag)
            .LoadAsync(cancellationToken);

        return userInterest;
    }

    public async Task<IEnumerable<UserInterest>> AddRangeAsync(Guid userId, string tagNames, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tagNames))
            return Enumerable.Empty<UserInterest>();

        var tagNameList = ParseTagNames(tagNames);
        if (!tagNameList.Any())
            return Enumerable.Empty<UserInterest>();

        var addedInterests = new List<UserInterest>();
        var tagsToAdd = new List<Tag>();

        foreach (var tagName in tagNameList)
        {
            var tag = await GetOrCreateTagAsync(tagName, cancellationToken);
            if (tag == null)
                continue;

            tagsToAdd.Add(tag);
        }

        var uniqueTags = tagsToAdd.DistinctBy(t => t.Id).ToList();

        // Get existing interests
        var existingTagIds = await _context.UserInterests
            .Where(ui => ui.UserId == userId && uniqueTags.Select(t => t.Id).Contains(ui.TagId))
            .Select(ui => ui.TagId)
            .ToListAsync(cancellationToken);

        foreach (var tag in uniqueTags)
        {
            if (!existingTagIds.Contains(tag.Id))
            {
                var userInterest = UserInterest.Create(userId, tag.Id).Value;
                addedInterests.Add(userInterest);

                // Increment tag usage count
                tag.IncrementUsage();
                _context.Tags.Update(tag);
            }
        }

        if (addedInterests.Any())
        {
            await _context.UserInterests.AddRangeAsync(addedInterests, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            // Load navigation properties
            foreach (var ui in addedInterests)
            {
                await _context.Entry(ui)
                    .Reference(ui => ui.Tag)
                    .LoadAsync(cancellationToken);
            }
        }

        return addedInterests;
    }

    public async Task<IEnumerable<UserInterest>> UpdateUserInterestsAsync(Guid userId, string tagNames, CancellationToken cancellationToken = default)
    {
        // If empty string, remove all interests
        if (string.IsNullOrWhiteSpace(tagNames))
        {
            await RemoveAllByUserIdAsync(userId, cancellationToken);
            return Enumerable.Empty<UserInterest>();
        }

        var tagNameList = ParseTagNames(tagNames);
        if (!tagNameList.Any())
            return await GetByUserIdAsync(userId, cancellationToken);

        // Get or create all tags
        var tags = new List<Tag>();
        foreach (var tagName in tagNameList)
        {
            var tag = await GetOrCreateTagAsync(tagName, cancellationToken);
            if (tag != null)
                tags.Add(tag);
        }

        var uniqueTags = tags.DistinctBy(t => t.Id).ToList();
        var requestedTagIds = uniqueTags.Select(t => t.Id).ToHashSet();

        // Get current user interests
        var currentInterests = await _context.UserInterests
            .Include(ui => ui.Tag)
            .Where(ui => ui.UserId == userId)
            .ToListAsync(cancellationToken);

        var currentTagIds = currentInterests.Select(ui => ui.TagId).ToHashSet();

        // Determine what to add and remove
        var tagsToAdd = requestedTagIds.Except(currentTagIds).ToList();
        var tagsToRemove = currentTagIds.Except(requestedTagIds).ToList();

        // Remove old interests
        if (tagsToRemove.Any())
        {
            var interestsToRemove = currentInterests
                .Where(ui => tagsToRemove.Contains(ui.TagId))
                .ToList();

            _context.UserInterests.RemoveRange(interestsToRemove);

            // Decrement usage count for removed tags
            var removedTags = tagsToRemove;//777uniqueTags.Where(t => tagsToRemove.Contains(t.Id)).ToList();
            foreach (var id in removedTags)
            {
                var tag = await tagRepository.GetById(id);
                if (tag != null)
                {
                    tag!.DecrementUsage();
                    _context.Tags.Update(tag);
                }
            }
        }

        // Add new interests
        var addedInterests = new List<UserInterest>();
        if (tagsToAdd.Any())
        {
            var tagsToAddEntities = uniqueTags.Where(t => tagsToAdd.Contains(t.Id)).ToList();

            foreach (var tag in tagsToAddEntities)
            {
                var userInterest = UserInterest.Create(userId, tag.Id).Value;
                addedInterests.Add(userInterest);

                // Increment usage count for new tags
                tag.IncrementUsage();
                _context.Tags.Update(tag);
            }

            if (addedInterests.Any())
            {
                await _context.UserInterests.AddRangeAsync(addedInterests, cancellationToken);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Return all current interests
        var allInterests = currentInterests
            .Where(ui => !tagsToRemove.Contains(ui.TagId))
            .Concat(addedInterests)
            .OrderByDescending(ui => ui.CreatedAt)
            .ToList();

        return allInterests;
    }

    public async Task<bool> RemoveAsync(Guid userId, Guid tagId, CancellationToken cancellationToken = default)
    {
        var userInterest = await _context.UserInterests
            .FirstOrDefaultAsync(ui => ui.UserId == userId && ui.TagId == tagId, cancellationToken);

        if (userInterest == null)
            return false;

        _context.UserInterests.Remove(userInterest);

        // Decrement tag usage count
        var tag = await _context.Tags.FindAsync(new object[] { tagId }, cancellationToken);
        if (tag != null)
        {
            tag.DecrementUsage();
            _context.Tags.Update(tag);
        }

        var result = await _context.SaveChangesAsync(cancellationToken);
        return result > 0;
    }

    public async Task<bool> RemoveRangeAsync(Guid userId, IEnumerable<Guid> tagIds, CancellationToken cancellationToken = default)
    {
        var tagIdList = tagIds.ToList();
        if (!tagIdList.Any())
            return false;

        var userInterests = await _context.UserInterests
            .Where(ui => ui.UserId == userId && tagIdList.Contains(ui.TagId))
            .ToListAsync(cancellationToken);

        if (!userInterests.Any())
            return false;

        _context.UserInterests.RemoveRange(userInterests);

        // Decrement usage count for removed tags
        var tags = await _context.Tags
            .Where(t => tagIdList.Contains(t.Id))
            .ToListAsync(cancellationToken);

        foreach (var tag in tags)
        {
            tag.DecrementUsage();
            _context.Tags.Update(tag);
        }

        var result = await _context.SaveChangesAsync(cancellationToken);
        return result > 0;
    }

    public async Task<bool> RemoveAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var userInterests = await _context.UserInterests
            .Where(ui => ui.UserId == userId)
            .ToListAsync(cancellationToken);

        if (!userInterests.Any())
            return false;

        var tagIds = userInterests.Select(ui => ui.TagId).Distinct().ToList();

        _context.UserInterests.RemoveRange(userInterests);

        // Decrement usage count for all removed tags
        var tags = await _context.Tags
            .Where(t => tagIds.Contains(t.Id))
            .ToListAsync(cancellationToken);

        foreach (var tag in tags)
        {
            tag.SetUsageCount(Math.Max(0, tag.UsageCount - userInterests.Count(ui => ui.TagId == tag.Id)));
            _context.Tags.Update(tag);
        }

        var result = await _context.SaveChangesAsync(cancellationToken);
        return result > 0;
    }

    public async Task<int> GetCountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserInterests
            .CountAsync(ui => ui.UserId == userId, cancellationToken);
    }

    public async Task<IEnumerable<Tag>> GetPopularTagsByUserCountAsync(int count, CancellationToken cancellationToken = default)
    {
        return await _context.Tags
            .Where(t => t.IsActive && t.UsageCount > 0)
            .OrderByDescending(t => t.UsageCount)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    #region Private Helper Methods

    private List<string> ParseTagNames(string tagNames)
    {
        if (string.IsNullOrWhiteSpace(tagNames))
            return new List<string>();

        return tagNames.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Trim())
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private async Task<Tag?> GetOrCreateTagAsync(string tagName, CancellationToken cancellationToken)
    {
        var slug = GenerateSlug(tagName);

        // Try to get existing tag
        var existingTag = await _tagRepository.GetByTagAsync(slug, cancellationToken);
        if (existingTag != null)
            return existingTag;

        // Create new tag
        var tagResult = Tag.Create(tagName);
        if (tagResult.IsFailure)
            return null;

        var newTag = tagResult.Value;

        await _context.Tags.AddAsync(newTag, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return newTag;
    }

    private string GenerateSlug(string name)
    {
        return name.Trim()
            .ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("--", "-")
            .Trim('-');
    }

    #endregion
}
