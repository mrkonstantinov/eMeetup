using eMeetup.Modules.Events.Domain.TagGroups;
using eMeetup.Modules.Events.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace eMeetup.Modules.Events.Infrastructure.Tags;

internal sealed class TagRepository(EventsDbContext context, ILogger<TagRepository> logger) : ITagRepository
{
    private readonly EventsDbContext _context = context;

    public async Task<Tag?> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            if (id == Guid.Empty)
                return null;

            return await _context.Tags
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting tag by ID: {TagId}", id);
            throw;
        }
    }

    public async Task<Tag?> GetByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(tag))
                return null;

            var normalizedTag = tag.Trim().ToLowerInvariant();
            return await _context.Tags
                .FirstOrDefaultAsync(t => t.Name.ToLower() == normalizedTag, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting tag: {Tag}", tag);
            throw;
        }
    }
}
