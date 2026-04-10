using System.Net.Sockets;
using eMeetup.Modules.Events.Domain.Events;

namespace eMeetup.Modules.Events.Domain.EventSessions;

public interface IEventSessionRepository
{
    Task<EventSession?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    void Insert(EventSession session);
}
