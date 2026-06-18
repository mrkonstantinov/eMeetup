using eMeetup.Common.Application.Clock;
using eMeetup.Common.Application.Messaging;
using eMeetup.Common.Domain;
using eMeetup.Modules.Events.Application.Abstractions.Data;
using eMeetup.Modules.Events.Application.MateTypes.CreateMateType;
using eMeetup.Modules.Events.Domain.Events;
using eMeetup.Modules.Events.Domain.EventSessions;

namespace eMeetup.Modules.Events.Application.EventSessions.CreateSession;

internal sealed class CreateSessionCommandHandler(
    IDateTimeProvider dateTimeProvider,
    ISessionRepository eventSessionRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateSessionCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateSessionCommand request, CancellationToken cancellationToken)
    {
        if (request.StartsAtUtc < dateTimeProvider.UtcNow)
         {
            return Result.Failure<Guid>(EventErrors.StartDateInPast);
        }

        Result<EventSession> result = EventSession.Create(
            request.EventId,
            request.Title,
            request.Description,
            request.StartsAtUtc,
            request.EndsAtUtc,
            request.Locality,
            request.Address,
            request.Latitude,
            request.Latitude);

        if (result.IsFailure)
        {
            return Result.Failure<Guid>(result.Error);
        }

        eventSessionRepository.Insert(result.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return result.Value.Id;
    }
}
