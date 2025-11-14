using System.Net;
using eMeetup.Common.Domain;
using eMeetup.Modules.Users.Application.Abstractions.Identity;
using Microsoft.Extensions.Logging;

namespace eMeetup.Modules.Users.Infrastructure.Identity;

internal sealed class IdentityProviderService(KeyCloakClient keyCloakClient, ILogger<IdentityProviderService> logger)
    : IIdentityProviderService
{
    private const string PasswordCredentialType = "Password";

    // POST /admin/realms/{realm}/users
    public async Task<Result<string>> RegisterUserAsync(UserModel user, CancellationToken cancellationToken = default)
    {
        var userRepresentation = new UserRepresentation(
            user.Username,
            user.Email,
            new Dictionary<string, List<string>>
            {
                { "gender", new List<string> { user.Gender.ToString() } }
            },
            true,
            true,
            [new CredentialRepresentation(PasswordCredentialType, user.Password, false)]);

        try
        {
            string identityId = await keyCloakClient.RegisterUserAsync(userRepresentation, cancellationToken);
            return identityId;
        }
        catch (HttpRequestException exception) when (exception.StatusCode == HttpStatusCode.Conflict)
        {
            logger.LogError(exception, "User registration failed");

            return Result.Failure<string>(IdentityProviderErrors.EmailIsNotUnique);
        }

    }

    public async Task<Result> UpdateUserAsync(UserProfileModel user, CancellationToken cancellationToken = default)
    {
        var userRepresentation = new UserProfileRepresentation(
            user.Email,
            new Dictionary<string, List<string>>
            {
                { "gender", user.Gender.HasValue ? new List<string> { user.Gender.Value.ToString() } : new List<string>() },
                { "dateofbirth", user.DateOfBirth.HasValue ? new List<string> { user.DateOfBirth.Value.ToString() } : new List<string>() },
                { "profilephotourl", !string.IsNullOrEmpty(user.ProfilePhotoUrl) ? new List<string> { user.ProfilePhotoUrl } : new List<string>() },
                { "bio", !string.IsNullOrEmpty(user.Bio) ? new List<string> { user.Bio } : new List<string>() },
                { "location", !string.IsNullOrEmpty(user.Location) ? new List<string> { user.Location } : new List<string>() },
                { "interests", !string.IsNullOrEmpty(user.Interests) ? new List<string> { user.Interests } : new List<string>() }
            });
        try
        {
            await keyCloakClient.UpdateUserAsync(user.IdentityId, userRepresentation, cancellationToken);
        }
        catch (HttpRequestException exception) when (exception.StatusCode == HttpStatusCode.Conflict)
        {
            return Result.Failure(Error.NullValue);
        }
        return Result.Success();
    }
}
