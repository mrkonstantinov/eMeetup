using eMeetup.Common.Domain;

namespace eMeetup.Modules.Events.Domain.EventSessions;

public sealed class SessionCompletedDomainEvent(Guid sessionId) : DomainEvent
{
    public Guid SessionId { get; init; } = sessionId;
}







