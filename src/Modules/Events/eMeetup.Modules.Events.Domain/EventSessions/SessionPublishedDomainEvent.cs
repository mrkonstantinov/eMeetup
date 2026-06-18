using eMeetup.Common.Domain;

namespace eMeetup.Modules.Events.Domain.EventSessions;

public sealed class SessionPublishedDomainEvent(Guid sessionId) : DomainEvent
{
    public Guid SessionId { get; init; } = sessionId;
}







