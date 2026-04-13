using eMeetup.Common.Application.EventBus;
using eMeetup.Common.Application.Exceptions;
using eMeetup.Common.Domain;
using eMeetup.Modules.Attendance.Application.Attendees.CreateAttendee;
using eMeetup.Modules.Attendance.Domain.Attendees;
using eMeetup.Modules.Users.IntegrationEvents;
using MediatR;

namespace eMeetup.Modules.Attendance.Presentation.Attendees;

internal sealed class UserRegisteredIntegrationEventHandler(ISender sender)
    : IntegrationEventHandler<UserRegisteredIntegrationEvent>
{
    public override async Task Handle(
        UserRegisteredIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        Result result = await sender.Send(
            new CreateAttendeeCommand(
                integrationEvent.UserId,
                integrationEvent.Email,
                integrationEvent.UserName,
                integrationEvent.DateOfBirth,
                (Gender)(integrationEvent.Gender)),
            cancellationToken);

        if (result.IsFailure)
        {
            throw new EmeetupException(nameof(CreateAttendeeCommand), result.Error);
        }
    }
}
