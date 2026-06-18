using eMeetup.Common.Domain;

namespace eMeetup.Modules.Events.Domain.EventSessions;

public sealed class SessionCreatedDomainEvent(Guid sessionId, Guid eventId) : DomainEvent
{
    public Guid SessionId { get; init; } = sessionId;
    public Guid EventId { get; init; } = eventId;
}







