using eMeetup.Common.Domain;

namespace eMeetup.Modules.Users.Application.Abstractions.Identity;

public interface IIdentityProviderService
{
    Task<Result<string>> RegisterUserAsync(UserModel user, CancellationToken cancellationToken = default);
    Task<Result> UpdateUserAsync(UserProfileModel user, CancellationToken cancellationToken = default);
    Task<UserProfileModel> GetUserAsync(Guid IdentityId, CancellationToken cancellationToken = default);

    Task<Result> UpdateKeycloakUserAttributesAsync(
        Guid identityId,
        string? bio,
        double? latitude,
        double? longitude,
        string? city,
        string? street,
        string? interests,
        string? profilePictureUrl,
        CancellationToken cancellationToken = default);
}
