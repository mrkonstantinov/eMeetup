using eMeetup.Common.Application.Messaging;
using eMeetup.Common.Domain;
using eMeetup.Modules.Events.Application.Abstractions.Data;
using eMeetup.Modules.Events.Domain.Events;
using eMeetup.Modules.Events.Domain.EventSessions;
using eMeetup.Modules.Events.Domain.MateTypes;

namespace eMeetup.Modules.Events.Application.EventSessions.PublishSession;

internal sealed class PublishSessionCommandHandler(
    ISessionRepository sessionRepository,
    IMateTypeRepository mateTypeRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<PublishSessionCommand>
{
    public async Task<Result> Handle(PublishSessionCommand request, CancellationToken cancellationToken)
    {
        EventSession? session = await sessionRepository.GetAsync(request.SessionId, cancellationToken);

        if (session is null)
        {
            return Result.Failure(EventSessionErrors.NotFound(request.SessionId));
        }

        if (!await mateTypeRepository.ExistsAsync(session.Id, cancellationToken))
        {
            return Result.Failure(EventErrors.NoTicketsFound);
        }

        session.Publish();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

