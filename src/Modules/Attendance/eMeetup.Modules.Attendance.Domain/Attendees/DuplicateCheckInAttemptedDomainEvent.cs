using eMeetup.Common.Domain;

namespace eMeetup.Modules.Attendance.Domain.Attendees;

public sealed class DuplicateCheckInAttemptedDomainEvent(Guid attendeeId, Guid eventId, Guid ticketId, string ticketCode)
    : DomainEvent
{
    public Guid AttendeeId { get; init; } = attendeeId;

    public Guid EventId { get; init; } = eventId;

    public Guid TicketId { get; init; } = ticketId;

    public string TicketCode { get; init; } = ticketCode;
}
