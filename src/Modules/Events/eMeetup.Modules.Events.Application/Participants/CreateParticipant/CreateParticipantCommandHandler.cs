using eMeetup.Common.Application.Messaging;
using eMeetup.Common.Domain;
using eMeetup.Modules.Events.Application.Abstractions.Data;
using eMeetup.Modules.Events.Domain.Participants;

namespace eMeetup.Modules.Events.Application.Participants.CreateParticipant;

internal sealed class CreateParticipantCommandHandler(IParticipantRepository participantRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateParticipantCommand>
{
    public async Task<Result> Handle(CreateParticipantCommand request, CancellationToken cancellationToken)
    {
        var participant = Participant.Create(request.ParticipantId, request.Email, request.UserName, request.DateOfBirth, request.Gender, request.SyncedAt);

        participantRepository.Insert(participant);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
