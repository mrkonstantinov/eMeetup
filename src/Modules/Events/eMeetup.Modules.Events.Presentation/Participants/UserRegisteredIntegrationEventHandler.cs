using eMeetup.Common.Application.EventBus;
using eMeetup.Common.Application.Exceptions;
using eMeetup.Common.Domain;
using eMeetup.Modules.Events.Application.Participants.CreateParticipant;
using eMeetup.Modules.Events.Domain.Participants;
using eMeetup.Modules.Users.IntegrationEvents;
using MediatR;

namespace eMeetup.Modules.Events.Presentation.Participants;

internal sealed class UserRegisteredIntegrationEventHandler(ISender sender)
    : IntegrationEventHandler<UserRegisteredIntegrationEvent>
{
    public override async Task Handle(
        UserRegisteredIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        Result result = await sender.Send(
            new CreateParticipantCommand(
                integrationEvent.UserId,
                integrationEvent.Email,
                integrationEvent.UserName,
                integrationEvent.DateOfBirth,
                (Gender)(integrationEvent.Gender),
                integrationEvent.OccurredOnUtc),
            cancellationToken);

        if (result.IsFailure)
        {
            throw new EmeetupException(nameof(CreateParticipantCommand), result.Error);
        }
    }
}
