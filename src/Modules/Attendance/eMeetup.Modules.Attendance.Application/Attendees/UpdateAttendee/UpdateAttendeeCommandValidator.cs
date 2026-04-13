using FluentValidation;

namespace eMeetup.Modules.Attendance.Application.Attendees.UpdateAttendee;

internal sealed class UpdateAttendeeCommandValidator : AbstractValidator<UpdateAttendeeCommand>
{
    public UpdateAttendeeCommandValidator()
    {
        RuleFor(c => c.AttendeeId).NotEmpty();
        RuleFor(c => c.UserName).NotEmpty();
    }
}
