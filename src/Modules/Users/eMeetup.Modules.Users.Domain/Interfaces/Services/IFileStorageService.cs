using eMeetup.Common.Domain;

namespace eMeetup.Modules.Users.Domain.Interfaces.Services;
public interface IFileStorageService
{
    Task<Result<string>> UploadPhotoAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);

    Task<Result> DeletePhotoAsync(
        string fileUrl,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        string fileUrl,
        CancellationToken cancellationToken = default);

    Task<string?> GetContentTypeAsync(
        string fileUrl,
        CancellationToken cancellationToken = default);
}
