using eMeetup.Common.Domain;

namespace eMeetup.Modules.Users.Application.Abstractions.Identity;

public interface IIdentityProviderService
{
    Task<Result<string>> RegisterUserAsync(UserModel user, CancellationToken cancellationToken = default);
    Task<Result> UpdateUserAsync(UserProfileModel user, CancellationToken cancellationToken = default);
}
