using eMeetup.Common.Domain;
using eMeetup.Modules.Users.Domain.Users;

namespace eMeetup.Modules.Users.Domain.Interfaces.Services;

public interface IUserInterestService
{
    // Basic CRUD operations
    Task<Result> AddInterestToUserAsync(Guid userId, string slug, CancellationToken cancellationToken = default);
    Task<Result> RemoveInterestFromUserAsync(Guid userId, string slug, CancellationToken cancellationToken = default);
    Task<Result> RemoveInterestFromUserAsync(Guid userId, Guid tagId, CancellationToken cancellationToken = default);

    // Bulk operations
    Task<Result> SyncUserInterestsAsync(Guid userId, IEnumerable<string> slugs, CancellationToken cancellationToken = default);
    Task<Result> SyncUserInterestsFromStringAsync(Guid userId, string slugString, CancellationToken cancellationToken = default);
    Task<Result> ClearUserInterestsAsync(Guid userId, CancellationToken cancellationToken = default);

    // Query operations
    Task<Result<List<Tag>>> GetUserInterestsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<List<string>>> GetUserInterestSlugsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<List<string>>> GetUserInterestNamesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<bool>> UserHasInterestAsync(Guid userId, string slug, CancellationToken cancellationToken = default);
    Task<Result<bool>> UserHasInterestAsync(Guid userId, Guid tagId, CancellationToken cancellationToken = default);

    // Matching & Compatibility
    //Task<Result<List<UserInterestMatch>>> FindCommonInterestsAsync(Guid user1Id, Guid user2Id, CancellationToken cancellationToken = default);
    Task<Result<int>> GetInterestMatchScoreAsync(Guid user1Id, Guid user2Id, CancellationToken cancellationToken = default);
    Task<Result<List<Guid>>> FindUsersWithMatchingInterestsAsync(Guid userId, int minMatches = 3, CancellationToken cancellationToken = default);

    // Analytics
    Task<Result<List<Tag>>> GetPopularInterestsAsync(int count = 10, CancellationToken cancellationToken = default);
    Task<Result<Dictionary<string, int>>> GetInterestStatisticsAsync(CancellationToken cancellationToken = default);
    Task<Result<int>> GetUserInterestCountAsync(Guid userId, CancellationToken cancellationToken = default);
}
