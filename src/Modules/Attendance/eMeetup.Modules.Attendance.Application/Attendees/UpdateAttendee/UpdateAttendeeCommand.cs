using eMeetup.Common.Application.Messaging;

namespace eMeetup.Modules.Attendance.Application.Attendees.UpdateAttendee;

public sealed record UpdateAttendeeCommand(Guid AttendeeId, string UserName) : ICommand;
