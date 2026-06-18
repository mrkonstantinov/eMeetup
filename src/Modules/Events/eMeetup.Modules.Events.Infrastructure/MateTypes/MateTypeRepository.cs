using eMeetup.Modules.Events.Domain.MateTypes;
using eMeetup.Modules.Events.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace eMeetup.Modules.Events.Infrastructure.MateTypes;

internal sealed class MateTypeRepository(EventsDbContext context) : IMateTypeRepository
{
    public async Task<MateType?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.MateTypes.SingleOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        return await context.MateTypes.AnyAsync(t => t.SessionId == sessionId, cancellationToken);
    }

    public void Insert(MateType mateType)
    {
        context.MateTypes.Add(mateType);
    }
}
