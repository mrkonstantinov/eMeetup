using eMeetup.Common.Application.EventBus;
using eMeetup.Common.Application.Exceptions;
using eMeetup.Common.Application.Messaging;
using eMeetup.Common.Domain;
using eMeetup.Modules.Users.Application.Users.GetUser;
using eMeetup.Modules.Users.Domain.Users;
using eMeetup.Modules.Users.IntegrationEvents;
using MediatR;

namespace eMeetup.Modules.Users.Application.Users.RegisterUser;

internal sealed class UserRegisteredDomainEventHandler(ISender sender, IEventBus bus)
    : DomainEventHandler<UserRegisteredDomainEvent>
{
    public override async Task Handle(
        UserRegisteredDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        Result<UserResponse> result = await sender.Send(
            new GetUserQuery(domainEvent.UserId),
            cancellationToken);

        if (result.IsFailure)
        {
            throw new EmeetupException(nameof(GetUserQuery), result.Error);
        }

        await bus.PublishAsync(
            new UserRegisteredIntegrationEvent(
                domainEvent.Id,
                domainEvent.OccurredOnUtc,
                result.Value.Id,
                result.Value.Email,
                result.Value.UserName,
                result.Value.Gender,
                result.Value.DateOfBirth,
                result.Value.Bio,
                result.Value.Interests),
            cancellationToken);
    }
}
