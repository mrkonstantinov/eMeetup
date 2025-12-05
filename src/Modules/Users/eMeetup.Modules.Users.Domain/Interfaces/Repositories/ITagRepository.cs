using eMeetup.Modules.Users.Domain.Users;

namespace eMeetup.Modules.Users.Domain.Interfaces.Repositories;

public interface ITagRepository
{
    Task<Tag?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IEnumerable<Tag>> GetAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Tag>> GetPopularAsync(int count, CancellationToken cancellationToken = default);
    Task<IEnumerable<Tag>> GetBySearchTermAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<IEnumerable<Tag>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Tag>> GetBySlugsAsync(IEnumerable<string> slugs, CancellationToken cancellationToken = default);
    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default);
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);
}


