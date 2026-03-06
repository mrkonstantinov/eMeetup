using System;
using System.Collections.Generic;
using System.Text;
using eMeetup.Common.Application.Clock;
using eMeetup.Common.Application.Messaging;
using eMeetup.Common.Domain;
using eMeetup.Modules.Events.Application.Abstractions.Data;
using eMeetup.Modules.Events.Domain.Events;

namespace eMeetup.Modules.Events.Application.Events.CreateEvent;

internal sealed class CreateEventCommandHandler(
    //IDateTimeProvider dateTimeProvider,
    IEventRepository eventRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateEventCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        //if (request.StartsAtUtc < dateTimeProvider.UtcNow)
        //{
        //    return Result.Failure<Guid>(EventErrors.StartDateInPast);
        //}

        Result<Event> result = Event.Create(
            request.CreatedByUserId,
            request.CreatedByUserName,
            request.Title,
            request.Description,
            request.Url,
            request.Tags);

        if (result.IsFailure)
        {
            return Result.Failure<Guid>(result.Error);
        }

        eventRepository.Insert(result.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return result.Value.Id;
    }
}

