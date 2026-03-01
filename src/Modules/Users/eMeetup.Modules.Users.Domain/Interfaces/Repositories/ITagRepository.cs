using eMeetup.Modules.Users.Domain.Tags;

namespace eMeetup.Modules.Users.Domain.Interfaces.Repositories;

public interface ITagRepository
{
    Task<Tag?> GetById(Guid id, CancellationToken cancellationToken = default);
    Task<Tag?> GetByTagAsync(string tag, CancellationToken cancellationToken = default);
    Task<IEnumerable<Tag>> GetAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Tag>> GetPopularAsync(int count, CancellationToken cancellationToken = default);
    Task<IEnumerable<Tag>> GetBySearchTermAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<IEnumerable<Tag>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Tag>> GetByTagsAsync(IEnumerable<string> tags, CancellationToken cancellationToken = default);
    Task<bool> TagExistsAsync(string tag, CancellationToken cancellationToken = default);
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);
}


