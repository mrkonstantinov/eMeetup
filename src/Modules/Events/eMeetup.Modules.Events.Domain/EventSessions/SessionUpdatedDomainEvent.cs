using eMeetup.Common.Domain;

namespace eMeetup.Modules.Events.Domain.EventSessions;

public sealed class SessionUpdatedDomainEvent(Guid sessionId) : DomainEvent
{
    public Guid SessionId { get; init; } = sessionId;
}







