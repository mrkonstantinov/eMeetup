using eMeetup.Common.Application.Messaging;

namespace eMeetup.Modules.Attendance.Application.Attendees.CheckInAttendee;

public sealed record CheckInAttendeeCommand(Guid AttendeeId, Guid TicketId) : ICommand;
