using eMeetup.Common.Domain;

namespace eMeetup.Modules.Events.Domain.Events;

public sealed class EventArchivedDomainEvent(Guid eventId) : DomainEvent
{
    public Guid EventId { get; init; } = eventId;
}
