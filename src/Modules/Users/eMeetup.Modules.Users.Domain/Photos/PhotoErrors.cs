using eMeetup.Common.Domain;

namespace eMeetup.Modules.Users.Domain.Photos;

public static class PhotoErrors
{
    // === ОШИБКИ ВАЛИДАЦИИ ===

    public static Error InvalidUserId =>
        Error.Validation("Users.Photo.InvalidUserId", "Invalid user ID format");
    
    public static Error EmptyUrl =>
        Error.Validation("Users.Photo.EmptyUrl", "Empty photo Url");

    public static Error InvalidUrl =>
        Error.Validation("Users.Photo.InvalidUrl", "Invalid photo URL format");

    public static Error EmptyPhoto =>
        Error.Validation("Users.Photo.Empty", "Photo URL cannot be empty");

    public static Error InvalidDisplayOrder =>
        Error.Validation("Users.Photo.InvalidDisplayOrder", "Display order must be greater than or equal to 0");

    public static Error TooManyPhotos(int maxCount) =>
        Error.Validation("Users.Photo.TooMany", $"Maximum {maxCount} photos allowed per user");

    public static Error InvalidFileType =>
        Error.Validation("Users.Photo.InvalidFileType", "Invalid file type. Allowed: JPG, PNG, GIF, WebP");

    public static Error FileTooLarge(long maxSize) =>
        Error.Validation("Users.Photo.FileTooLarge", $"File size exceeds maximum of {maxSize} bytes");

    public static Error InvalidFileName =>
        Error.Validation("Users.Photo.InvalidFileName", "Invalid file name");

    // === ОШИБКИ НЕ НАЙДЕНО ===

    public static Error NotFound(Guid photoId) =>
        Error.NotFound("Users.Photo.NotFound", $"Photo with ID '{photoId}' was not found");

    public static Error NotFoundByUrl(string url) =>
        Error.NotFound("Users.Photo.NotFoundByUrl", $"Photo with URL '{url}' was not found");

    public static Error PrimaryPhotoNotFound =>
        Error.NotFound("Users.Photo.PrimaryNotFound", "Primary photo not found for this user");

    // === ОШИБКИ СТАТУСА ===

    public static Error CannotUploadForInactiveUser =>
        Error.Problem("Users.Photo.CannotUploadInactive", "Cannot upload photos for inactive user");

    public static Error CannotUploadForDeletedUser =>
        Error.Problem("Users.Photo.CannotUploadDeleted", "Cannot upload photos for deleted user");

    public static Error CannotUploadForSuspendedUser =>
        Error.Problem("Users.Photo.CannotUploadSuspended", "Cannot upload photos for suspended user");

    public static Error CannotDeletePrimaryPhoto =>
        Error.Problem("Users.Photo.CannotDeletePrimary", "Cannot delete primary photo. Set another photo as primary first");

    public static Error CannotSetAsPrimary =>
        Error.Problem("Users.Photo.CannotSetAsPrimary", "Cannot set this photo as primary");

    // === ОШИБКИ ОПЕРАЦИЙ ===

    public static Error UploadFailed(string reason) =>
        Error.Failure("Users.Photo.UploadFailed", $"Photo upload failed: {reason}");

    public static Error DeleteFailed(string reason) =>
        Error.Failure("Users.Photo.DeleteFailed", $"Photo deletion failed: {reason}");

    public static Error ThumbnailGenerationFailed =>
        Error.Failure("Users.Photo.ThumbnailFailed", "Failed to generate thumbnail for photo");

    public static Error StorageError(string reason) =>
        Error.Failure("Users.Photo.StorageError", $"Storage error: {reason}");

    public static Error InvalidOperation(string reason) =>
        Error.Problem("Users.Photo.InvalidOperation", reason);
}
