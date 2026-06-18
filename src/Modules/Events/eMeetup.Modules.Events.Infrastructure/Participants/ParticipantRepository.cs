using eMeetup.Modules.Events.Domain.Participants;
using eMeetup.Modules.Events.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace eMeetup.Modules.Events.Infrastructure.Participants;

internal sealed class ParticipantRepository(EventsDbContext context) : IParticipantRepository
{
    public async Task<Participant?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Participants.SingleOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public void Insert(Participant participant)
    {
        context.Participants.Add(participant);
    }
}
