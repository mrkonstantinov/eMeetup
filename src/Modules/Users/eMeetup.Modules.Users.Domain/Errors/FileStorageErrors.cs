using eMeetup.Common.Domain;

namespace eMeetup.Modules.Users.Domain.Errors;
public static class FileStorageErrors
{
    public static Error EmptyFile =>
        Error.Validation("FileStorage.EmptyFile", "File is empty");

    public static Error InvalidFileType =>
        Error.Validation("FileStorage.InvalidFileType", "Invalid file type. Supported types: JPEG, PNG, GIF, WEBP");

    public static Error InvalidFileUrl =>
        Error.Validation("FileStorage.InvalidFileUrl", "Invalid file URL");

    public static Error UploadFailed =>
        Error.Failure("FileStorage.UploadFailed", "Failed to upload file");

    public static Error DeleteFailed =>
        Error.Failure("FileStorage.DeleteFailed", "Failed to delete file");

    public static Error FileNotFound =>
        Error.NotFound("FileStorage.FileNotFound", "File not found");

    public static Error StorageUnavailable =>
        Error.Failure("FileStorage.StorageUnavailable", "File storage is unavailable");
}
