using eMeetup.Common.Domain;

namespace eMeetup.Modules.Users.Domain.Users;


public static class UserErrors
{
    // === ОБЩИЕ ОШИБКИ ===

    public static Error NotFound(string userId) =>
        Error.NotFound("Users.NotFound", $"User with ID '{userId}' was not found");

    public static Error NotFoundByKeycloak(string keycloakId) =>
        Error.NotFound("Users.NotFoundByKeycloak", $"User with Keycloak ID '{keycloakId}' was not found");

    public static Error NotFoundByEmail(string email) =>
        Error.NotFound("Users.NotFoundByEmail", $"User with email '{email}' was not found");

    public static Error NotFoundByUsername(string username) =>
        Error.NotFound("Users.NotFoundByUsername", $"User with username '{username}' was not found");

    public static Error AlreadyExists(string field, string value) =>
        Error.Conflict("Users.AlreadyExists", $"User with {field} '{value}' already exists");

    public static Error InvalidOperation(string message) =>
        Error.Problem("Users.InvalidOperation", message);

    public static Error UnauthorizedOperation(string userId) =>
        Error.Unauthorized("Users.Unauthorized", $"User '{userId}' is not authorized to perform this operation");


    public static Error EmailAlreadyRegistered(string email) =>
        Error.Conflict("Users.EmailAlreadyRegistered", $"The email address '{email}' is already registered");

    public static Error UsernameAlreadyExists(string username) =>
        Error.Conflict("Users.UsernameAlreadyExists", $"The username '{username}' is already taken");

    public static Error KeycloakSyncFailed =>
        Error.Failure("User.KeycloakSyncFailed", "Failed to synchronize user changes with Keycloak");

    public static Error KeycloakUserNotFound =>
        Error.NotFound("User.KeycloakUserNotFound", "User not found in Keycloak");

    public static Error KeycloakUpdateFailed =>
        Error.Failure("User.KeycloakUpdateFailed", "Failed to update user in Keycloak");

    public static Error KeycloakConflict =>
        Error.Conflict("User.KeycloakConflict", "Keycloak update conflict occurred");

    public static Error KeycloakTimeout =>
        Error.Failure("User.KeycloakTimeout", "Keycloak request timed out");

    public static Error KeycloakAuthorizationFailed =>
        Error.Failure("User.KeycloakAuthorizationFailed", "Authorization failed for Keycloak update");

    public static Error KeycloakServiceUnavailable =>
        Error.Failure("User.KeycloakServiceUnavailable", "Keycloak service unavailable");

    public static Error RegistrationFailed(string error) =>
    Error.Failure("Users.RegistrationFailed", $"User registration failed. {error}.");

    public static Error DatabaseSaveFailed(string error) =>
        Error.Failure("Users.DatabaseSaveFailed", $"Failed {error}.");

    public static Error UpdateFailed(string error) =>
        Error.Failure("Users.UpdateFailed", $"Failed to update user to database. {error}.");

    public static Error OperationFailed(string error) =>
        Error.Failure("Users.OperationFailed", $"Failed. {error}.");    
}
