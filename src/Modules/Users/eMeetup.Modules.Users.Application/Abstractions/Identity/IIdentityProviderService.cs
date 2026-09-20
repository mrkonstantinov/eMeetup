using eMeetup.Common.Domain;

namespace eMeetup.Modules.Users.Application.Abstractions.Identity;

public interface IIdentityProviderService
{
    Task<Result<Guid>> RegisterUserAsync(
            UserModel user,
            CancellationToken cancellationToken = default);

    //Task<Result> DeleteUserAsync(
    //    Guid identityId,
    //    CancellationToken cancellationToken = default);

    //Task<Result> UpdateUserAsync(
    //    Guid identityId,
    //    UserModel user,
    //    CancellationToken cancellationToken = default);

    Task<Result<UserModel>> GetUserAsync(
        Guid identityId,
        CancellationToken cancellationToken = default);


    //Task<Result> ChangePasswordAsync(
    //    string identityId,
    //    string currentPassword,
    //    string newPassword,
    //    CancellationToken cancellationToken = default);

    //Task<Result> UpdateKeycloakUserAttributesAsync(
    //    Guid identityId,
    //    string? locality,
    //    string? street,
    //    string? bio,
    //    string? interests,
    //    string? profileImageUrl,
    //    CancellationToken cancellationToken = default);
}
