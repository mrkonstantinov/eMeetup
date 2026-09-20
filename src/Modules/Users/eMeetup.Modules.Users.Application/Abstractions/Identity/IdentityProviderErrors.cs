using eMeetup.Common.Domain;

namespace eMeetup.Modules.Users.Application.Abstractions.Identity;

public static class IdentityProviderErrors
{
    // === ОШИБКИ РЕГИСТРАЦИИ ===

    public static readonly Error EmailIsNotUnique = Error.Conflict(
        "Identity.EmailIsNotUnique",
        "The specified email is not unique.");

    public static Error UsernameIsNotUnique =>
        Error.Conflict("Identity.UsernameIsNotUnique", "Username is already taken in identity provider");

    public static Error RegistrationFailed =>
        Error.Failure("Identity.RegistrationFailed", "Failed to register user in identity provider");

    public static Error InvalidIdentityId =>
        Error.Failure("Identity.InvalidIdentityId", "Identity provider returned an invalid user ID");

    // === ОШИБКИ ПОИСКА ===

    public static Error UserNotFound(Guid identityId) =>
        Error.NotFound("Identity.UserNotFound", $"User with ID '{identityId}' not found in identity provider");

    // === ОШИБКИ ОБНОВЛЕНИЯ ===

    public static Error UpdateFailed(string reason) =>
        Error.Failure("Identity.UpdateFailed", $"Failed to update user in identity provider: {reason}");

    // === ОШИБКИ УДАЛЕНИЯ ===

    public static Error DeleteFailed =>
        Error.Failure("Identity.DeleteFailed", "Failed to delete user from identity provider");

    // === ОШИБКИ ПОЛУЧЕНИЯ ===

    public static Error GetFailed =>
        Error.Failure("Identity.GetFailed", "Failed to get user from identity provider");

    // === ОШИБКИ СОЕДИНЕНИЯ ===

    public static Error ConnectionFailed =>
        Error.Failure("Identity.ConnectionFailed", "Failed to connect to identity provider");

    public static Error Timeout =>
        Error.Failure("Identity.Timeout", "Identity provider request timeout");

    public static Error Unauthorized =>
        Error.Unauthorized("Identity.Unauthorized", "Unauthorized request to identity provider");

    public static Error Forbidden =>
        Error.Forbidden("Identity.Forbidden", "Forbidden request to identity provider");

    // === ОШИБКИ ВАЛИДАЦИИ ===

    public static Error InvalidEmail =>
        Error.Validation("Identity.InvalidEmail", "Invalid email format for identity provider");

    public static Error InvalidUsername =>
        Error.Validation("Identity.InvalidUsername", "Invalid username for identity provider");

    public static Error InvalidPassword =>
        Error.Validation("Identity.InvalidPassword", "Invalid password for identity provider");

    public static Error PasswordTooWeak =>
        Error.Validation("Identity.PasswordTooWeak", "Password does not meet identity provider requirements");

    // === ОШИБКИ ВНЕШНИХ СЕРВИСОВ ===

    public static Error KeycloakError(string reason) =>
        Error.Failure("Identity.KeycloakError", $"Keycloak error: {reason}");

    public static Error ServiceUnavailable =>
        Error.Problem("Identity.ServiceUnavailable", "Identity provider service is unavailable");
}
