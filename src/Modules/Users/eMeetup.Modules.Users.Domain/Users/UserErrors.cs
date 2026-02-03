using eMeetup.Common.Domain;

namespace eMeetup.Modules.Users.Domain.Users;


public static class UserErrors
{
    // User errors
    public static Error NotFound(string userId) =>
        Error.NotFound("Users.NotFound", $"The user with the identifier {userId} was not found");

    public static Error NotFoundByIdentity(Guid IdentityId) =>
        Error.NotFound("Users.NotFound", $"The user with the IdentityId {IdentityId} was not found");    

    public static Error EmailAlreadyExists(string email) =>
        Error.Conflict("Users.EmailAlreadyExists", $"The email address '{email}' is already registered");

    public static Error UsernameAlreadyExists(string username) =>
        Error.Conflict("Users.UsernameAlreadyExists", $"The username '{username}' is already taken");

    public static Error InvalidDateOfBirth =>
        Error.Validation("Users.InvalidDateOfBirth", "You must be at least 18 years old to register");

    public static Error InvalidEmail =>
        Error.Validation("Users.InvalidEmail", "Email address is required");

    public static Error InvalidPassword =>
        Error.Validation("Users.InvalidPassword", "Password is required");

    public static Error InvalidUsername =>
        Error.Validation("Users.InvalidUsername", "Username is required");

    public static Error InvalidIdentityId =>
        Error.Validation("Users.InvalidIdentityId", "Identity ID is required");

    public static Error PasswordTooShort =>
        Error.Validation("Users.PasswordTooShort", "Password must be at least 6 characters long");

    public static Error UsernameTooShort =>
        Error.Validation("Users.UsernameTooShort", "Username must be at least 3 characters long");

    public static Error InvalidGender =>
        Error.Validation("Users.InvalidGender", "Invalid gender value");

    public static Error RegistrationFailed =>
        Error.Failure("Users.RegistrationFailed", "User registration failed. Please try again.");

    public static Error DatabaseSaveFailed =>
        Error.Failure("Users.DatabaseSaveFailed", "Failed to save user to database. Please try again.");

    public static Error UpdateFailed =>
        Error.Failure("Users.UpdateFailed", "Failed to update user to database. Please try again.");

    public static Error DatabaseUpdateFailed => 
        Error.Failure("User.DatabaseUpdateFailed", "Database update operation failed");

    public static Error DatabaseConcurrencyConflict => 
        Error.Conflict("User.DatabaseConcurrencyConflict", "Database concurrency conflict occurred");


    public static Error InvalidLocation =>
        Error.Validation("Users.InvalidLocation", "Invalid location provided");

    public static Error InvalidStatusTransition =>
        Error.Validation("Users.InvalidStatusTransition", "Invalid user status transition");

    // Photo errors
    public static Error PhotoNotFound(Guid photoId) =>
        Error.NotFound("Users.PhotoNotFound", $"The photo with the identifier {photoId} was not found");

    public static Error PhotoNotOwnedByUser =>
        Error.Validation("Users.PhotoNotOwnedByUser", "The photo does not belong to this user");

    public static Error TooManyPhotos(int maxPhotos) =>
        Error.Validation("Users.TooManyPhotos", $"Maximum {maxPhotos} photos allowed per user");

    public static Error EmptyPhoto =>
        Error.Validation("Users.EmptyPhoto", "Photo file is empty");

    public static Error PhotoTooLarge(long maxSizeInMB) =>
        Error.Validation("Users.PhotoTooLarge", $"Photo size exceeds {maxSizeInMB}MB limit");

    public static Error InvalidPhotoFormat =>
        Error.Validation("Users.InvalidPhotoFormat", "Invalid photo format. Supported formats: JPEG, PNG, GIF, WEBP");

    public static Error PhotoUploadFailed =>
        Error.Failure("Users.PhotoUploadFailed", "Failed to upload one or more photos");

    public static Error InvalidDisplayOrder =>
        Error.Validation("Users.InvalidDisplayOrder", "Display order cannot be negative");

    public static Error DuplicateDisplayOrder =>
        Error.Validation("Users.DuplicateDisplayOrder", "Duplicate display order found");

    public static Error PhotoUpdateFailed =>
        Error.Failure("Users.PhotoUpdateFailed", "Failed to update user photos");

    public static Error OperationCancelled =>
        Error.Failure("User.OperationCancelled", "Photo upload operation was cancelled.");

    // Photo upload specific errors
    public static Error NoPhotosProvided => 
        Error.Validation("User.NoPhotosProvided", "No photos were provided for upload");

    public static Error TotalPhotoSizeExceeded => 
        Error.Validation("User.TotalPhotoSizeExceeded", "Total size of all photos exceeds the limit of 50MB");

    public static Error PhotoEmpty => 
        Error.Validation("User.PhotoEmpty", "Photo file is empty");

    public static Error InvalidPhotoMimeType => Error.Validation(
        "User.InvalidPhotoMimeType",
        "Invalid photo MIME type");

    public static Error PhotoUploadRetryFailed => Error.Failure(
        "User.PhotoUploadRetryFailed",
        "Failed to upload photo after multiple retries");

    public static Error AllPhotosUploadFailed => Error.Failure(
        "User.AllPhotosUploadFailed",
        "All photo uploads failed");

    public static Error PhotoDomainValidationFailed => Error.Validation(
        "User.PhotoDomainValidationFailed",
        "Failed to add photo to user domain model");

    public static Error PhotoCleanupFailed => Error.Failure(
        "User.PhotoCleanupFailed",
        "Failed to cleanup uploaded photos");

    public static Error PhotoDeletionFailed => Error.Failure(
        "User.PhotoDeletionFailed",
        "Failed to delete photo");


    // Interest errors
    public static Error InvalidTagSlug => 
        Error.Validation("User.InvalidTagSlug", "Tag slug is invalid");

    public static Error InterestAddFailed => 
        Error.Validation("User.InterestAddFailed", "Failed to add interest to user");

    public static Error InterestRemoveFailed => 
        Error.Validation("User.InterestRemoveFailed", "Failed to remove interest from user");

    public static Error InterestSyncFailed => 
        Error.Validation("User.InterestSyncFailed", "Failed to sync user interests");

    public static Error InterestRetrievalFailed => 
        Error.Validation("User.InterestRetrievalFailed", "Failed to retrieve user interests");

    public static Error InterestAlreadyAdded => 
        Error.Validation("User.InterestAlreadyAdded", "This interest has already been added to the user");

    public static Error TooManyInterests(int maxInterests) =>
    Error.Validation("User.TooManyInterests", $"Cannot add more than {maxInterests} interests");

    public static Error InterestNotFound => 
        Error.NotFound("User.InterestNotFound", "Interest was not found for this user");

    // General errors
    public static Error OperationFailed =>
        Error.Failure("Users.OperationFailed", "The operation failed. Please try again.");

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

    public static Error AtomicUpdateFailed => 
        Error.Failure("User.AtomicUpdateFailed", "Atomic update operation failed");
}
