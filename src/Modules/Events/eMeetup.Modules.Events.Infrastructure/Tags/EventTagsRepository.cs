
using eMeetup.Common.Domain;
using eMeetup.Common.Domain.Interfaces.Repositories;
using eMeetup.Modules.Events.Domain.EventTags;
using eMeetup.Modules.Events.Domain.Interfaces.Repositories;
using eMeetup.Modules.Events.Domain.Tags;
using eMeetup.Modules.Events.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace eMeetup.Modules.Events.Infrastructure.Tags;



public class EventTagsRepository(EventsDbContext context, ITagRepository tagRepository, ILogger<EventTagsRepository> logger) : IEventTagsRepository
{
    private readonly EventsDbContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly ILogger<EventTagsRepository> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ITagRepository _tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(_tagRepository));

    public async Task<IEnumerable<EventTag>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return await _context.EventTags
            .Include(ui => ui.Tag)
            .Where(ui => ui.EventId == eventId)
            .OrderBy(ui => ui.Tag.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<EventTag>> UpdateEventTagsAsync(Guid eventId, string tagNames, CancellationToken cancellationToken = default)
    {
        // If empty string, remove all interests
        if (string.IsNullOrWhiteSpace(tagNames))
        {
            await RemoveAllByEventIdAsync(eventId, cancellationToken);
            return Enumerable.Empty<EventTag>();
        }

        var tagNameList = ParseTagNames(tagNames);
        if (!tagNameList.Any())
            return await GetByEventIdAsync(eventId, cancellationToken);

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
        var currentInterests = await _context.EventTags
            .Include(ui => ui.Tag)
            .Where(ui => ui.EventId == eventId)
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

            _context.EventTags.RemoveRange(interestsToRemove);

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
        var addedInterests = new List<EventTag>();
        if (tagsToAdd.Any())
        {
            var tagsToAddEntities = uniqueTags.Where(t => tagsToAdd.Contains(t.Id)).ToList();

            foreach (var tag in tagsToAddEntities)
            {
                var userInterest = EventTag.Create(eventId, tag.Id).Value;
                addedInterests.Add(userInterest);

                // Increment usage count for new tags
                tag.IncrementUsage();
                _context.Tags.Update(tag);
            }

            if (addedInterests.Any())
            {
                await _context.EventTags.AddRangeAsync(addedInterests, cancellationToken);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Return all current interests
        var allInterests = currentInterests
            .Where(ui => !tagsToRemove.Contains(ui.TagId))
            .Concat(addedInterests)
            .OrderBy(ui => ui.Tag.Name)
            .ToList();

        return allInterests;
    }

    public async Task<bool> RemoveAllByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        var eventTags = await _context.EventTags
            .Where(ui => ui.EventId == eventId)
            .ToListAsync(cancellationToken);

        if (!eventTags.Any())
            return false;

        var tagIds = eventTags.Select(ui => ui.TagId).Distinct().ToList();

        _context.EventTags.RemoveRange(eventTags);

        // Decrement usage count for all removed tags
        var tags = await _context.Tags
            .Where(t => tagIds.Contains(t.Id))
            .ToListAsync(cancellationToken);

        foreach (var tag in tags)
        {
            tag.SetUsageCount(Math.Max(0, tag.UsageCount - eventTags.Count(ui => ui.TagId == tag.Id)));
            _context.Tags.Update(tag);
        }

        var result = await _context.SaveChangesAsync(cancellationToken);
        return result > 0;
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
