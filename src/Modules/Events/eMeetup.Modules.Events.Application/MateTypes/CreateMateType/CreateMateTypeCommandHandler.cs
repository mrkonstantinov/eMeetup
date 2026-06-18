using eMeetup.Common.Application.Messaging;
using eMeetup.Common.Domain;
using eMeetup.Modules.Events.Application.Abstractions.Data;
using eMeetup.Modules.Events.Domain.EventSessions;
using eMeetup.Modules.Events.Domain.MateTypes;

namespace eMeetup.Modules.Events.Application.MateTypes.CreateMateType;

internal sealed class CreateMateTypeCommandHandler(
    ISessionRepository eventSessionRepository,
    IMateTypeRepository mateTypeRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateMateTypeCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateMateTypeCommand request, CancellationToken cancellationToken)
    {
        var eventSession = await eventSessionRepository.GetAsync(request.SessionId, cancellationToken);
        if (eventSession is null)
        {
            return Result.Failure<Guid>(Error.NotFound(
                "EventSession.NotFound",
                $"EventSession with ID '{request.SessionId}' was not found."));
        }

        Result<MateType> result = MateType.Create(
            request.SessionId,
            request.Title,
            request.Description,
            request.AllocatedSlots,
            request.Budget,
            request.MinAge,
            request.MaxAge,
            request.Gender,
            request.PreferredGender,
            request.PreferredAgeRange,
            request.Priority);

        if (result.IsFailure)
        {
            return Result.Failure<Guid>(result.Error);
        }

        mateTypeRepository.Insert(result.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return result.Value.Id;
    }
}
