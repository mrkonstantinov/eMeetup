using eMeetup.Common.Application.Messaging;
using eMeetup.Modules.Attendance.Domain.Attendees;

namespace eMeetup.Modules.Attendance.Application.Attendees.CreateAttendee;

public sealed record CreateAttendeeCommand(Guid AttendeeId, string Email, string UserName, DateTime DateOfBirth, Gender Gender)
    : ICommand;
