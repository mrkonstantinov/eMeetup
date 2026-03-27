using eMeetup.Modules.Events.Domain.Tags;

namespace eMeetup.Modules.Events.Domain.Interfaces.Repositories;

public interface ITagRepository
{
    Task<Tag?> GetById(Guid id, CancellationToken cancellationToken = default);
    Task<Tag?> GetByTagAsync(string tag, CancellationToken cancellationToken = default);
}
