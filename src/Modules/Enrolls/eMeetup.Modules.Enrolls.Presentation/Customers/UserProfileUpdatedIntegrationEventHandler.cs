using eMeetup.Common.Application.EventBus;
using eMeetup.Common.Application.Exceptions;
using eMeetup.Common.Domain;
using eMeetup.Modules.Enrolls.Application.Customers.UpdateCustomer;
using eMeetup.Modules.Users.IntegrationEvents;
using MediatR;

namespace eMeetup.Modules.Enrolls.Presentation.Customers;

internal sealed class UserProfileUpdatedIntegrationEventHandler(ISender sender)
    : IntegrationEventHandler<UserProfileUpdatedIntegrationEvent>
{
    public override async Task Handle(
        UserProfileUpdatedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        Result result = await sender.Send(
            new UpdateCustomerCommand(
                integrationEvent.UserId,
                integrationEvent.UserName),
            cancellationToken);

        if (result.IsFailure)
        {
            throw new EmeetupException(nameof(UpdateCustomerCommand), result.Error);
        }
    }
}
