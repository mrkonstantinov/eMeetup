using eMeetup.Modules.Users.Domain.Tags;

namespace eMeetup.Modules.Users.Domain.Interfaces.Repositories;

public interface ITagRepository
{
    Task<UserTag?> GetById(Guid id, CancellationToken cancellationToken = default);
    Task<UserTag?> GetByTagAsync(string tag, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserTag>> GetAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<UserTag>> GetPopularAsync(int count, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserTag>> GetBySearchTermAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserTag>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<UserTag>> GetByTagsAsync(IEnumerable<string> tags, CancellationToken cancellationToken = default);
    Task<bool> TagExistsAsync(string tag, CancellationToken cancellationToken = default);
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);
}


