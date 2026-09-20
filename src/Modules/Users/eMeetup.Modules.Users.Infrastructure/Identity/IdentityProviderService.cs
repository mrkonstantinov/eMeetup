using System.Globalization;
using System.Net;
using eMeetup.Common.Domain;
using eMeetup.Modules.Users.Application.Abstractions.Identity;
using eMeetup.Modules.Users.Domain.Users;
using Microsoft.Extensions.Logging;

namespace eMeetup.Modules.Users.Infrastructure.Identity;

internal sealed class IdentityProviderService(
    KeyCloakClient keyCloakClient,
    ILogger<IdentityProviderService> logger)
    : IIdentityProviderService
{
    private const string PasswordCredentialType = "Password";

    // ================================================================
    // === REGISTER ===
    // ================================================================
    public async Task<Result<Guid>> RegisterUserAsync(
        UserModel user,
        CancellationToken cancellationToken = default)
    {
        var userRepresentation = new UserRepresentation(
            Username: user.Username,
            Email: user.Email,
            Enabled: true,
            EmailVerified: false,
            Credentials:
            [
                new CredentialRepresentation(
                    Type: PasswordCredentialType,
                    Value: user.Password,
                    Temporary: false)
            ]);

        try
        {
            var identityId = await keyCloakClient.RegisterUserAsync(
                userRepresentation,
                cancellationToken);

            if (!Guid.TryParse(identityId, out var identityGuid))
            {
                logger.LogError("Keycloak returned invalid GUID: {IdentityId}", identityId);
                return Result.Failure<Guid>(IdentityProviderErrors.InvalidIdentityId);
            }

            return Result.Success(identityGuid);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
        {
            logger.LogError(ex, "User registration failed: {Email} already exists", user.Email);
            return Result.Failure<Guid>(IdentityProviderErrors.EmailIsNotUnique);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Keycloak registration failed for {Email}", user.Email);
            return Result.Failure<Guid>(IdentityProviderErrors.RegistrationFailed);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during registration for {Email}", user.Email);
            return Result.Failure<Guid>(IdentityProviderErrors.RegistrationFailed);
        }
    }

    // ================================================================
    // === DELETE ===
    // ================================================================
    //public async Task<Result> DeleteUserAsync(
    //    Guid identityId,
    //    CancellationToken cancellationToken = default)
    //{
    //    try
    //    {
    //        await keyCloakClient.DeleteUserAsync(identityId.ToString(), cancellationToken);
    //        return Result.Success();
    //    }
    //    catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
    //    {
    //        logger.LogWarning("User {IdentityId} not found in Keycloak during delete", identityId);
    //        return Result.Success();
    //    }
    //    catch (HttpRequestException ex)
    //    {
    //        logger.LogError(ex, "Failed to delete user {IdentityId} from Keycloak", identityId);
    //        return Result.Failure(IdentityProviderErrors.DeleteFailed);
    //    }
    //    catch (Exception ex)
    //    {
    //        logger.LogError(ex, "Unexpected error deleting user {IdentityId}", identityId);
    //        return Result.Failure(IdentityProviderErrors.DeleteFailed);
    //    }
    //}

    // ================================================================
    // === UPDATE ===
    // ================================================================
    //public async Task<Result> UpdateUserAsync(
    //    Guid identityId,
    //    UserModel user,
    //    CancellationToken cancellationToken = default)
    //{
    //    var userRepresentation = new UserRepresentation(
    //        Username: user.Username,
    //        Email: user.Email,
    //        Enabled: true,
    //        EmailVerified: true,
    //        Credentials: null);

    //    try
    //    {
    //        await keyCloakClient.UpdateUserAsync(
    //            identityId,
    //            userRepresentation,
    //            cancellationToken);

    //        return Result.Success();
    //    }
    //    catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
    //    {
    //        logger.LogWarning("User {IdentityId} not found in Keycloak during update", identityId);
    //        return Result.Failure(IdentityProviderErrors.UserNotFound);
    //    }
    //    catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
    //    {
    //        logger.LogError(ex, "Conflict updating user {IdentityId}", identityId);
    //        return Result.Failure(IdentityProviderErrors.EmailIsNotUnique);
    //    }
    //    catch (HttpRequestException ex)
    //    {
    //        logger.LogError(ex, "Failed to update user {IdentityId} in Keycloak", identityId);
    //        return Result.Failure(IdentityProviderErrors.UpdateFailed);
    //    }
    //    catch (Exception ex)
    //    {
    //        logger.LogError(ex, "Unexpected error updating user {IdentityId}", identityId);
    //        return Result.Failure(IdentityProviderErrors.UpdateFailed);
    //    }
    //}

    // ================================================================
    // === GET ===
    // ================================================================
    public async Task<Result<UserModel>> GetUserAsync(
        Guid identityId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var keycloakUser = await keyCloakClient.GetUserAsync(
                identityId,
                cancellationToken);

            if (keycloakUser == null)
                return Result.Failure<UserModel>(IdentityProviderErrors.UserNotFound(identityId));

            return Result.Success(keycloakUser);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            logger.LogWarning("User {IdentityId} not found in Keycloak", identityId);
            return Result.Failure<UserModel>(IdentityProviderErrors.UserNotFound(identityId));
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to get user {IdentityId} from Keycloak", identityId);
            return Result.Failure<UserModel>(IdentityProviderErrors.GetFailed);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error getting user {IdentityId}", identityId);
            return Result.Failure<UserModel>(IdentityProviderErrors.GetFailed);
        }
    }

    // ================================================================
    // === PRIVATE HELPERS ===
    // ================================================================
    private static UserModel MapToUserModel(UserRepresentation keycloakUser)
    {
        return new UserModel(
            Email: keycloakUser.Email ?? string.Empty,
            Password: string.Empty,
            Username: keycloakUser.Username ?? string.Empty);
    }
}
