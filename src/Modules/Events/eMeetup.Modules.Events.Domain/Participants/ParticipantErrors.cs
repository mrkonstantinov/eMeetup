using eMeetup.Common.Domain;

namespace eMeetup.Modules.Events.Domain.Participants;

public static class ParticipantErrors
{
    public static Error NotFound(Guid participantId) =>
        Error.NotFound("Participants.NotFound", $"The participant with the identifier {participantId} was not found");
}
