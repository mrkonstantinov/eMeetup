using eMeetup.Common.Domain;

namespace eMeetup.Modules.Users.Domain.Users;

public static class RegistrationErrors
{
    // === ОШИБКИ ВАЛИДАЦИИ ===

    public static Error InvalidEmail(string email) =>
        Error.Validation("Users.Registration.InvalidEmail", $"Email '{email}' has invalid format");

    public static Error InvalidUsername(string username) =>
        Error.Validation("Users.Registration.InvalidUsername", $"Username '{username}' is invalid");

    public static Error UsernameTooShort(int minLength) =>
        Error.Validation("Users.Registration.UsernameTooShort", $"Username must be at least {minLength} characters");

    public static Error UsernameTooLong(int maxLength) =>
        Error.Validation("Users.Registration.UsernameTooLong", $"Username cannot exceed {maxLength} characters");

    public static Error UsernameContainsInvalidChars =>
        Error.Validation("Users.Registration.UsernameInvalidChars", "Username contains invalid characters");

    public static Error PasswordTooShort(int minLength) =>
        Error.Validation("Users.Registration.PasswordTooShort", $"Password must be at least {minLength} characters");

    public static Error PasswordTooWeak =>
        Error.Validation("Users.Registration.PasswordTooWeak", "Password does not meet security requirements");

    public static Error InvalidKeycloakId =>
        Error.Validation("Users.Registration.InvalidKeycloakId", "Invalid Keycloak user ID");

    public static Error InvalidDisplayName =>
        Error.Validation("Users.Registration.InvalidDisplayName", "Invalid display name");

    public static Error NicknameRequired =>
        Error.Validation("Users.Registration.NicknameRequired", "Nickname is required");

    public static Error NicknameTooShort(int minLength) =>
        Error.Validation("Users.Registration.NicknameTooShort", $"Nickname must be at least {minLength} characters");

    public static Error NicknameTooLong(int maxLength) =>
        Error.Validation("Users.Registration.NicknameTooLong", $"Nickname cannot exceed {maxLength} characters");

    public static Error NicknameContainsInvalidChars =>
        Error.Validation("Users.Registration.NicknameInvalidChars", "Nickname contains invalid characters");

    // === ОШИБКИ КОНФЛИКТА ===

    public static Error EmailAlreadyRegistered =>
        Error.Conflict("Users.Registration.EmailAlreadyRegistered", "Email is already registered");

    public static Error UsernameAlreadyTaken =>
        Error.Conflict("Users.Registration.UsernameAlreadyTaken", "Username is already taken");

    public static Error DisplayNameAlreadyTaken =>
        Error.Conflict("Users.Registration.DisplayNameAlreadyTaken", "Display name is already taken");

    public static Error NicknameAlreadyTaken =>
        Error.Conflict("Users.Registration.NicknameAlreadyTaken", "Nickname is already taken");

    // === ОШИБКИ ВНЕШНИХ СЕРВИСОВ ===

    public static Error KeycloakRegistrationFailed(string reason) =>
        Error.Failure("Users.Registration.KeycloakFailed", $"Keycloak registration failed: {reason}");

    public static Error KeycloakUserCreationFailed(string reason) =>
        Error.Failure("Users.Registration.KeycloakUserCreationFailed", $"Failed to create user in Keycloak: {reason}");

    public static Error EmailServiceFailed(string reason) =>
        Error.Failure("Users.Registration.EmailServiceFailed", $"Failed to send confirmation email: {reason}");

    public static Error KeycloakConnectionFailed =>
        Error.Failure("Users.Registration.KeycloakConnectionFailed", "Failed to connect to Keycloak service");

    public static Error KeycloakTimeout =>
        Error.Failure("Users.Registration.KeycloakTimeout", "Keycloak service timeout");

    // === ОШИБКИ ОПЕРАЦИЙ ===

    public static Error RegistrationFailed(string reason) =>
        Error.Failure("Users.Registration.Failed", $"User registration failed: {reason}");

    public static Error EmailConfirmationFailed =>
        Error.Failure("Users.Registration.EmailConfirmationFailed", "Email confirmation failed");

    public static Error InvalidConfirmationToken =>
        Error.Validation("Users.Registration.InvalidConfirmationToken", "Invalid confirmation token");

    public static Error ConfirmationTokenExpired =>
        Error.Validation("Users.Registration.ConfirmationTokenExpired", "Confirmation token has expired");

    public static Error ConfirmationTokenAlreadyUsed =>
        Error.Conflict("Users.Registration.ConfirmationTokenAlreadyUsed", "Confirmation token has already been used");

    public static Error UserAlreadyActivated =>
        Error.Conflict("Users.Registration.UserAlreadyActivated", "User is already activated");

    public static Error ActivationFailed(string reason) =>
        Error.Failure("Users.Registration.ActivationFailed", $"User activation failed: {reason}");

    // === ОШИБКИ РЕГИСТРАЦИИ С KEYCLOAK ===

    public static Error KeycloakUserNotFound(string keycloakId) =>
        Error.NotFound("Users.Registration.KeycloakUserNotFound", $"Keycloak user with ID '{keycloakId}' not found");

    public static Error KeycloakUserAlreadyLinked =>
        Error.Conflict("Users.Registration.KeycloakUserAlreadyLinked", "Keycloak user is already linked to an existing account");

    public static Error KeycloakUserNotActive =>
        Error.Problem("Users.Registration.KeycloakUserNotActive", "Keycloak user is not active");

    public static Error KeycloakEmailNotVerified =>
        Error.Problem("Users.Registration.KeycloakEmailNotVerified", "Email is not verified in Keycloak");
}
