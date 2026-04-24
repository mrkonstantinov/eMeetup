using eMeetup.Modules.Events.Domain.EventTags;

namespace eMeetup.Modules.Events.Domain.Interfaces.Repositories;

public interface IEventTagsRepository
{
    Task<IEnumerable<EventTag>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task<IEnumerable<EventTag>> UpdateEventTagsAsync(Guid eventId, string tagNames, CancellationToken cancellationToken = default);
    // Remove all interests for a user
    Task<bool> RemoveAllByEventIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
