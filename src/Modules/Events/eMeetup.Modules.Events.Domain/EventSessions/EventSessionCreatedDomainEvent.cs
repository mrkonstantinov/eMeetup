using eMeetup.Common.Domain;

namespace eMeetup.Modules.Events.Domain.EventSessions;

public sealed class EventSessionCreatedDomainEvent(Guid eventSessionId, Guid eventId) : DomainEvent
{
    public Guid EventSessionId { get; init; } = eventSessionId;
    public Guid EventId { get; init; } = eventId;
}

public sealed class EventSessionUpdatedDomainEvent(Guid eventSessionId) : DomainEvent
{
    public Guid EventSessionId { get; init; } = eventSessionId;
}

public sealed class EventSessionPublishedDomainEvent(Guid eventSessionId) : DomainEvent
{
    public Guid EventSessionId { get; init; } = eventSessionId;
}

public sealed class EventSessionCanceledDomainEvent(Guid eventSessionId, EventSessionStatus status) : DomainEvent
{
    public Guid EventSessionId { get; init; } = eventSessionId;
    public EventSessionStatus Status { get; init; } = status;
}

public sealed class EventSessionCompletedDomainEvent(Guid eventSessionId) : DomainEvent
{
    public Guid EventSessionId { get; init; } = eventSessionId;
}







