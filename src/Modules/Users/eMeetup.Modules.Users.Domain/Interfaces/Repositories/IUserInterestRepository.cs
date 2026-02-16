using eMeetup.Common.Domain;
using eMeetup.Modules.Users.Domain.UserInterests;
using eMeetup.Modules.Users.Domain.Users;

namespace eMeetup.Modules.Users.Domain.Interfaces.Repositories;

public interface IUserInterestRepository
{
    Task<UserInterest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserInterest>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserInterest>> GetByTagIdAsync(Guid tagId, CancellationToken cancellationToken = default);
    Task<UserInterest?> GetByUserAndTagIdAsync(Guid userId, Guid tagId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid userId, Guid tagId, CancellationToken cancellationToken = default);

    // Add single interest
    Task<UserInterest?> AddAsync(Guid userId, string tagName, CancellationToken cancellationToken = default);

    // Add multiple interests from comma-separated string
    Task<IEnumerable<UserInterest>> AddRangeAsync(Guid userId, string tagNames, CancellationToken cancellationToken = default);

    // Update all interests for a user - order of words doesn't matter
    Task<IEnumerable<UserInterest>> UpdateUserInterestsAsync(Guid userId, string tagNames, CancellationToken cancellationToken = default);

    // Remove specific interests
    Task<bool> RemoveAsync(Guid userId, Guid tagId, CancellationToken cancellationToken = default);
    Task<bool> RemoveRangeAsync(Guid userId, IEnumerable<Guid> tagIds, CancellationToken cancellationToken = default);

    // Remove all interests for a user
    Task<bool> RemoveAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    // Bulk operations
    Task<int> GetCountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Tag>> GetPopularTagsByUserCountAsync(int count, CancellationToken cancellationToken = default);
}
