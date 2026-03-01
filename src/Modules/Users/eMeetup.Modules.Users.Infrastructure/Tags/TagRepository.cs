using eMeetup.Modules.Users.Domain.Interfaces.Repositories;
using eMeetup.Modules.Users.Domain.Tags;
using eMeetup.Modules.Users.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace eMeetup.Modules.Users.Infrastructure.Tags;

internal sealed class TagRepository(UsersDbContext context, ILogger<TagRepository> logger) : ITagRepository
{
    private readonly UsersDbContext _context = context;

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

    public async Task<IEnumerable<Tag>> GetAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Tags
                .OrderBy(t => t.Name)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting all tags");
            throw;
        }
    }

    public async Task<IEnumerable<Tag>> GetPopularAsync(int count, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Tags
                .Where(t => t.IsActive)
                .OrderByDescending(t => t.UsageCount)
                .ThenBy(t => t.Name)
                .Take(count)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting popular tags");
            throw;
        }
    }

    public async Task<IEnumerable<Tag>> GetBySearchTermAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetActiveAsync(cancellationToken);

            var term = searchTerm.Trim().ToLower();
            return await _context.Tags
                .Where(t => t.IsActive &&
                           (t.Name.ToLower().Contains(term) ||
                            t.Description.ToLower().Contains(term)))
                .OrderBy(t => t.Name)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error searching tags with term: {SearchTerm}", searchTerm);
            throw;
        }
    }

    public async Task<IEnumerable<Tag>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Tags
                .Where(t => t.IsActive)
                .OrderBy(t => t.Name)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting active tags");
            throw;
        }
    }

    public async Task<IEnumerable<Tag>> GetByTagsAsync(IEnumerable<string> tags, CancellationToken cancellationToken = default)
    {
        try
        {
            var tagList = tags.Select(s => s.Trim().ToLowerInvariant()).ToList();
            if (!tagList.Any())
                return Enumerable.Empty<Tag>();

            return await _context.Tags
                .Where(t => tagList.Contains(t.Name.ToLower()))
                .OrderBy(t => t.Name)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting tags");
            throw;
        }
    }

    public async Task<bool> TagExistsAsync(string tag, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(tag))
                return false;

            var normalizedTag = tag.Trim().ToLowerInvariant();
            return await _context.Tags
                .AnyAsync(t => t.Name.ToLower() == normalizedTag, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking if tag exists by tag: {Tag}", tag);
            throw;
        }
    }

    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Tags
                .CountAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting tag count");
            throw;
        }
    }
}
