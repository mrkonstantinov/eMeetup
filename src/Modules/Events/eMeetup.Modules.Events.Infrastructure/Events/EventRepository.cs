using eMeetup.Modules.Events.Domain.Events;
using eMeetup.Modules.Events.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace eMeetup.Modules.Events.Infrastructure.Events;

internal sealed class EventRepository(EventsDbContext context) : IEventRepository
{
    public async Task<Event?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Events.SingleOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public void Insert(Event @event)
    {
        context.Events.Add(@event);
    }
}
