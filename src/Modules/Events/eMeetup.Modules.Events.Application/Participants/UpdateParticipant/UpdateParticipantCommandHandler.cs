using eMeetup.Common.Application.Messaging;
using eMeetup.Common.Domain;
using eMeetup.Modules.Events.Application.Abstractions.Data;
using eMeetup.Modules.Events.Domain.Participants;

namespace eMeetup.Modules.Events.Application.Participants.UpdateParticipant;

internal sealed class UpdateParticipantCommandHandler(IParticipantRepository participantRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateParticipantCommand>
{
    public async Task<Result> Handle(UpdateParticipantCommand request, CancellationToken cancellationToken)
    {
        Participant? participant = await participantRepository.GetAsync(request.ParticipantId, cancellationToken);

        if (participant is null)
        {
            return Result.Failure(ParticipantErrors.NotFound(request.ParticipantId));
        }

        participant.Update(request.UserName, request.SyncedAt);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
