using System.Globalization;
using System.Net;
using eMeetup.Common.Domain;
using eMeetup.Modules.Users.Application.Abstractions.Identity;
using eMeetup.Modules.Users.Domain.Users;
using Microsoft.Extensions.Logging;

namespace eMeetup.Modules.Users.Infrastructure.Identity;

internal sealed class IdentityProviderService(KeyCloakClient keyCloakClient, ILogger<IdentityProviderService> logger)
    : IIdentityProviderService
{
    private const string PasswordCredentialType = "Password";

    // List of fields that can be updated in Keycloak based on permissions in your config
    private static readonly HashSet<string> AllowedKeycloakAttributes = new()
    {
        "locality",
        "street",
        "bio",
        "interests",
        "uri"
    };

    // POST /admin/realms/{realm}/users
    public async Task<Result<string>> RegisterUserAsync(UserModel user, CancellationToken cancellationToken = default)
    {
        var userRepresentation = new UserRepresentation(
            user.Username,
            user.Email,
            new Dictionary<string, List<string>>
            {
               { "gender", new List<string> { user.Gender.ToString() } },
               { "birthdate", new List<string> { user.DateOfBirth.ToString() } }
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
            user.UserName,
            new Dictionary<string, List<string>>
            {
                { "uri", !string.IsNullOrEmpty(user.Uri) ? new List<string> { user.Uri } : new List<string>() },
                { "bio", !string.IsNullOrEmpty(user.Bio) ? new List<string> { user.Bio } : new List<string>() },
                { "locality", !string.IsNullOrWhiteSpace(user.Locality) ? new List<string> { user.Locality! } : new List<string>() },
                { "street", !string.IsNullOrWhiteSpace(user.Street) ? new List<string> { user.Street! } : new List<string>() },
                { "interests", !string.IsNullOrEmpty(user.Interests) ? new List<string> { user.Interests } : new List<string>() }
            },            
            true,
            true);
        try
        {
            await keyCloakClient.UpdateUserAsync(user.IdentityId, userRepresentation, cancellationToken);
        }
        catch (HttpRequestException exception) when (exception.StatusCode == HttpStatusCode.Conflict)
        {
            return Result.Failure(Error.NullValue);
        }
        catch (Exception exception) 
        {
            return Result.Failure(Error.NullValue);
        }
        return Result.Success();
    }

    public async Task<UserProfileModel> GetUserAsync(Guid identityId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get user from Keycloak
            var keycloakUser = await keyCloakClient.GetUserAsync(identityId, cancellationToken);

            // Map Keycloak representation to your domain model
            return MapToUserProfileModel(keycloakUser, identityId);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            logger.LogWarning(ex, "User with ID {IdentityId} not found", identityId);
            throw; // Or return null/default based on your error handling strategy
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving user with ID {IdentityId}", identityId);
            throw;
        }
    }

    private static UserProfileModel MapToUserProfileModel(UserProfileRepresentation keycloakUser, Guid identityId)
    {
        // Helper method to get a single attribute value
        string? GetAttributeValue(string key)
        {
            if (keycloakUser.Attributes != null &&
                keycloakUser.Attributes.TryGetValue(key, out var values) &&
                values != null &&
                values.Any())
            {
                return values[0];
            }
            return null;
        }

        // Parse date of birth from attributes
        DateTime? birthdate = null;
        var birthdateStr = GetAttributeValue("birthdate");
        if (!string.IsNullOrEmpty(birthdateStr))
        {
            if (DateTime.TryParse(birthdateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
            {
                birthdate = parsedDate;
            }
        }

        // Parse gender from attributes
        Gender gender = Gender.Other; // Default value
        var genderStr = GetAttributeValue("gender");
        if (!string.IsNullOrEmpty(genderStr) && Enum.TryParse<Gender>(genderStr, true, out var parsedGender))
        {
            gender = parsedGender;
        }

        return new UserProfileModel(
            IdentityId: identityId,
            Email: keycloakUser.Email ?? string.Empty,
            UserName: keycloakUser.Username ?? string.Empty,
            DateOfBirth: birthdate ?? DateTime.MinValue, // Or use DateTime? in your record if allowed
            Gender: gender,
            Locality: GetAttributeValue("locality"),
            Street: GetAttributeValue("street"),
            Bio: GetAttributeValue("bio"),
            Interests: GetAttributeValue("interests"),
            Uri: GetAttributeValue("uri")
        );
    }

    // Optional: Helper method to get user by email
    //public async Task<UserProfileModel?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    //{
    //    try
    //    {
    //        var keycloakUser = await keyCloakClient.GetUserByEmailAsync(email, cancellationToken);
    //        if (keycloakUser == null)
    //            return null;

    //        return MapToUserProfileModel(keycloakUser, keycloakUser.IdentityId);
    //    }
    //    catch (Exception ex)
    //    {
    //        logger.LogError(ex, "Error retrieving user by email {Email}", email);
    //        throw;
    //    }
    //}

    public async Task<Result> UpdateKeycloakUserAttributesAsync(
        Guid identityId,
        string? locality,
        string? street,
        string? bio,
        string? interests,
        string? uri,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Updating Keycloak attributes for user {IdentityId}", identityId);

            // First, get the current user from Keycloak
            var currentUser = await keyCloakClient.GetUserAsync(identityId, cancellationToken);

            if (currentUser == null)
            {
                logger.LogWarning("User {IdentityId} not found in Keycloak", identityId);
                return Result.Failure(Error.NotFound("IdentityProviderService.UpdateKeycloakUserAttributesAsync", $"User {identityId} not found in Keycloak"));
            }

            // Create a dictionary to hold the updated attributes
            var updatedAttributes = currentUser.Attributes != null
                ? new Dictionary<string, List<string>>(currentUser.Attributes)
                : new Dictionary<string, List<string>>();

            // Update only the allowed attributes
            UpdateAttributeIfNotNull(updatedAttributes, "bio", bio);
            UpdateAttributeIfNotNull(updatedAttributes, "locality", locality);
            UpdateAttributeIfNotNull(updatedAttributes, "street", street);
            UpdateAttributeIfNotNull(updatedAttributes, "interests", interests);
            UpdateAttributeIfNotNull(updatedAttributes, "uri", uri);

            // Create a minimal user representation for Keycloak update
            // Only include the attributes that can be updated
            var keycloakUpdate = new UserProfileRepresentation(
                Username: currentUser.Username, // Don't update username
                Email: currentUser.Email, // Don't update email
                Attributes: updatedAttributes,
                EmailVerified: currentUser.EmailVerified, // Don't update email verification status
                Enabled: currentUser.Enabled // Don't update enabled status                
                //Credentials: null
            );

            // Update in Keycloak
            await keyCloakClient.UpdateUserAsync(identityId, keycloakUpdate, cancellationToken);

            logger.LogInformation("Successfully updated Keycloak attributes for user {IdentityId}", identityId);
            return Result.Success();
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            logger.LogWarning(ex, "User {IdentityId} not found in Keycloak", identityId);
            return Result.Failure(UserErrors.NotFound($"User {identityId} not found in Keycloak"));
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
        {
            logger.LogError(ex, "Conflict updating Keycloak user {IdentityId}", identityId);
            return Result.Failure(Error.Conflict("IdentityProviderService.UpdateKeycloakUserAttributesAsync", "Conflict updating user in Keycloak"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating Keycloak attributes for user {IdentityId}", identityId);
            return Result.Failure(Error.Validation("IdentityProviderService.UpdateKeycloakUserAttributesAsync", "Failed to update Keycloak user attributes"));
        }
    }

    private static void UpdateAttributeIfNotNull(
        Dictionary<string, List<string>> attributes,
        string key,
        string? value)
    {
        if (value != null)
        {
            attributes[key] = new List<string> { value };
        }
        else if (attributes.ContainsKey(key))
        {
            attributes.Remove(key);
        }
        // If value is null and key doesn't exist, do nothing
    }

}
