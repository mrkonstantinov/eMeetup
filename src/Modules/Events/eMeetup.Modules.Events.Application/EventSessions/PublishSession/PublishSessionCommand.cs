using System.Diagnostics.Eventing.Reader;
using eMeetup.Common.Application.EventBus;
using eMeetup.Common.Application.Exceptions;
using eMeetup.Common.Application.Messaging;
using eMeetup.Common.Domain;
using eMeetup.Modules.Events.Application.Events.GetEvent;
using eMeetup.Modules.Events.Domain.Events;
using MediatR;

namespace eMeetup.Modules.Events.Application.EventSessions.PublishSession;

public sealed record PublishSessionCommand(Guid SessionId) : ICommand;


internal sealed class SessionPublishedDomainEventHandler(ISender sender, IEventBus eventBus)
    : DomainEventHandler<EventPublishedDomainEvent>
{
    public override async Task Handle(
        EventPublishedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        Result<EventResponse> result = await sender.Send(new GetEventQuery(domainEvent.EventId), cancellationToken);

        if (result.IsFailure)
        {
            throw new EmeetupException(nameof(GetEventQuery), result.Error);
        }

        //await eventBus.PublishAsync(
        //    new EventPublishedIntegrationEvent(
        //        domainEvent.Id,
        //        domainEvent.OccurredOnUtc,
        //        result.Value.Id,
        //        result.Value.Title,
        //        result.Value.Description,
        //        result.Value.Location,
        //        result.Value.StartsAtUtc,
        //        result.Value.EndsAtUtc,
        //        result.Value.TicketTypes.Select(t => new TicketTypeModel
        //        {
        //            Id = t.TicketTypeId,
        //            EventId = result.Value.Id,
        //            Name = t.Name,
        //            Price = t.Price,
        //            Currency = t.Currency,
        //            Quantity = t.Quantity
        //        }).ToList()),
        //    cancellationToken);
    }
}
