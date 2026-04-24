namespace eMeetup.Modules.Events.Domain.TagGroups;

public interface ITagRepository
{
    Task<Tag?> GetById(Guid id, CancellationToken cancellationToken = default);
    Task<Tag?> GetByTagAsync(string tag, CancellationToken cancellationToken = default);
}
