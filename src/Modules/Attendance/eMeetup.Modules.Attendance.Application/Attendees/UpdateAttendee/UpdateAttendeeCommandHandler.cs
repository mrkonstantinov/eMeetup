using eMeetup.Common.Application.Messaging;
using eMeetup.Common.Domain;
using eMeetup.Modules.Attendance.Application.Abstractions.Data;
using eMeetup.Modules.Attendance.Domain.Attendees;

namespace eMeetup.Modules.Attendance.Application.Attendees.UpdateAttendee;

internal sealed class UpdateAttendeeCommandHandler(IAttendeeRepository attendeeRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateAttendeeCommand>
{
    public async Task<Result> Handle(UpdateAttendeeCommand request, CancellationToken cancellationToken)
    {
        Attendee? attendee = await attendeeRepository.GetAsync(request.AttendeeId, cancellationToken);

        if (attendee is null)
        {
            return Result.Failure(AttendeeErrors.NotFound(request.AttendeeId));
        }

        attendee.Update(request.UserName, request.SyncedAt);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
