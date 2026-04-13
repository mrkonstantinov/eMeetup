using eMeetup.Common.Application.EventBus;
using eMeetup.Common.Application.Exceptions;
using eMeetup.Common.Domain;
using eMeetup.Modules.Enrolls.Application.Customers.CreateCustomer;
using eMeetup.Modules.Enrolls.Domain.Customers;
using eMeetup.Modules.Users.IntegrationEvents;
using MediatR;

namespace eMeetup.Modules.Enrolls.Presentation.Customers;

internal sealed class UserRegisteredIntegrationEventHandler(ISender sender)
    : IntegrationEventHandler<UserRegisteredIntegrationEvent>
{
    public override async Task Handle(
        UserRegisteredIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        Result result = await sender.Send(
            new CreateCustomerCommand(
                integrationEvent.UserId,
                integrationEvent.Email,
                integrationEvent.UserName,
                integrationEvent.DateOfBirth,
                (Gender)(integrationEvent.Gender)),
            cancellationToken);

        if (result.IsFailure)
        {
            throw new EmeetupException(nameof(CreateCustomerCommand), result.Error);
        }
    }
}
