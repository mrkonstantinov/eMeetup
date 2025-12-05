using eMeetup.Common.Domain;
using eMeetup.Modules.Users.Domain.Users;

namespace eMeetup.Modules.Users.Domain.Interfaces.Services;

public interface ISlugService
{
    // Parse slug string and validate each slug exists
    Task<Result<string[]>> ParseAndValidateSlugsAsync(string slugString, CancellationToken cancellationToken = default);

    // Get existing slugs from a string, ignoring non-existent ones
    Task<IEnumerable<Tag>> GetExistingSlugsAsync(string slugString, CancellationToken cancellationToken = default);

    // Convert array of slugs back to a string
    string CombineSlugs(IEnumerable<string> slugs, bool includeSpaces = true);

    string CombineSlugs(IEnumerable<Tag> tags, bool includeSpaces = true);

    // Update user interests from a slug string
    Task<Result> SyncUserInterestsFromSlugStringAsync(User user, string slugString, IUserInterestService userInterestService, CancellationToken cancellationToken = default);   
}
