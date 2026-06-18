using eMeetup.Common.Domain;

namespace eMeetup.Modules.Events.Domain.EventSessions;

public sealed class SessionCanceledDomainEvent(Guid sessionId, EventSessionStatus status) : DomainEvent
{
    public Guid SessionId { get; init; } = sessionId;
    public EventSessionStatus Status { get; init; } = status;
}







