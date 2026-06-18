namespace eMeetup.Modules.Events.Domain.Participants;

public interface IParticipantRepository
{
    Task<Participant?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    void Insert(Participant participant);
}
