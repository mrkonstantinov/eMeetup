using eMeetup.Modules.Events.Domain.EventSessions;
using eMeetup.Modules.Events.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace eMeetup.Modules.Events.Infrastructure.EventSessions;

internal sealed class EventSessionRepository(EventsDbContext context) : IEventSessionRepository
{
    public async Task<EventSession?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.EventSessions.SingleOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public void Insert(EventSession session)
    {
        context.EventSessions.Add(session);
    }
}
