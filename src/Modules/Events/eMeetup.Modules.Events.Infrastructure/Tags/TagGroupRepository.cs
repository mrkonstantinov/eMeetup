using eMeetup.Modules.Events.Domain.TagGroups;
using eMeetup.Modules.Events.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace eMeetup.Modules.Events.Infrastructure.Tags;

internal sealed class TagGroupRepository(EventsDbContext context, ILogger<TagRepository> logger) : ITagGroupRepository
{
    private readonly EventsDbContext _context = context;


    public async Task<TagGroup?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.TagGroups
            .FirstOrDefaultAsync(tg => tg.Id == id, cancellationToken);
    }

    public async Task<string?> GetPictureFileNameByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.TagGroups
            .Where(tg => tg.Id == id)
            .Select(tg => tg.PictureFileName)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
