using eMeetup.Common.Application.Messaging;

namespace eMeetup.Modules.Attendance.Application.Attendees.GetAttendee;

public sealed record GetAttendeeQuery(Guid CustomerId) : IQuery<AttendeeResponse>;
