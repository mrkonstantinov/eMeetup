using eMeetup.Common.Domain;

namespace eMeetup.Modules.Users.Application.Abstractions;

public interface IFileUploadService
{
    /// <summary>
    /// Загружает файл и возвращает информацию о загруженном файле
    /// </summary>
    Task<Result<UploadedFile>> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Удаляет файл по URL
    /// </summary>
    Task<Result> DeleteAsync(
        string fileUrl,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Генерирует миниатюру
    /// </summary>
    Task<Result<string>> GenerateThumbnailAsync(
        string originalUrl,
        CancellationToken cancellationToken = default);
}

public sealed record UploadedFile(
    string Url,
    string? ThumbnailUrl,
    string FileName,
    long FileSize,
    string ContentType);
