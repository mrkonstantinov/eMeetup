using eMeetup.Common.Domain;
using eMeetup.Modules.Users.Application.Abstractions;
using eMeetup.Modules.Users.Domain.Errors;
using eMeetup.Modules.Users.Domain.Photos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace eMeetup.Modules.Users.Infrastructure.Storage;

internal sealed class LocalFileUploadService : IFileUploadService
{
    private readonly string _uploadPath;
    private readonly string _baseUrl;
    private readonly long _maxFileSizeBytes;
    private readonly string[] _allowedContentTypes;
    private readonly int _thumbnailSize;
    private readonly ILogger<LocalFileUploadService> _logger;

    public LocalFileUploadService(
        IConfiguration configuration,
        ILogger<LocalFileUploadService> logger)
    {
        var section = configuration.GetSection("Storage:Local");

        _uploadPath = section["UploadPath"]
            ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

        _baseUrl = section["BaseUrl"]
            ?? "https://localhost:5001";

        _maxFileSizeBytes = section.GetValue<long?>("MaxFileSizeBytes")
            ?? 5 * 1024 * 1024; // 5 MB

        _allowedContentTypes = section.GetSection("AllowedContentTypes").Get<string[]>()
            ?? new[] { "image/jpeg", "image/jpg", "image/png", "image/webp", "image/gif" };

        _thumbnailSize = section.GetValue<int?>("ThumbnailSize") ?? 300;

        _logger = logger;

        // Создаём директорию, если её нет
        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
            _logger.LogInformation("Created upload directory: {Path}", _uploadPath);
        }
    }

    // ================================================================
    // === UPLOAD ===
    // ================================================================
    public async Task<Result<UploadedFile>> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Валидация типа
            if (!_allowedContentTypes.Contains(contentType.ToLowerInvariant()))
            {
                _logger.LogWarning(
                    "Invalid file type: {ContentType}. Allowed: {Allowed}",
                    contentType,
                    string.Join(", ", _allowedContentTypes));

                return Result.Failure<UploadedFile>(PhotoErrors.InvalidFileType);
            }

            // Валидация размера
            if (fileStream.Length > _maxFileSizeBytes)
            {
                _logger.LogWarning(
                    "File too large: {Size} bytes (max: {Max})",
                    fileStream.Length,
                    _maxFileSizeBytes);

                return Result.Failure<UploadedFile>(PhotoErrors.FileTooLarge(_maxFileSizeBytes));
            }

            // Генерируем уникальное имя
            var extension = Path.GetExtension(fileName);
            var uniqueName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(_uploadPath, uniqueName);

            // Сохраняем оригинал
            await using (var outputStream = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(outputStream, cancellationToken);
            }

            var fileInfo = new FileInfo(filePath);

            // Генерируем миниатюру
            var thumbnailName = $"thumb_{uniqueName}";
            var thumbnailPath = Path.Combine(_uploadPath, thumbnailName);
            var thumbnailResult = await GenerateThumbnailAsync(filePath, thumbnailPath);

            var thumbnailUrl = thumbnailResult.IsSuccess
                ? $"{_baseUrl}/uploads/{thumbnailName}"
                : null;

            _logger.LogInformation(
                "File uploaded: {FileName} → {UniqueName} ({Size} bytes)",
                fileName,
                uniqueName,
                fileInfo.Length);

            return Result.Success(new UploadedFile(
                Url: $"{_baseUrl}/uploads/{uniqueName}",
                ThumbnailUrl: thumbnailUrl,
                FileName: fileName,
                FileSize: fileInfo.Length,
                ContentType: contentType));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload file {FileName}", fileName);
            return Result.Failure<UploadedFile>(PhotoErrors.UploadFailed(ex.Message));
        }
    }

    // ================================================================
    // === DELETE ===
    // ================================================================
    public Task<Result> DeleteAsync(
        string fileUrl,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fileUrl))
                return Task.FromResult(Result.Success());

            var fileName = Path.GetFileName(new Uri(fileUrl).LocalPath);
            var filePath = Path.Combine(_uploadPath, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("File deleted: {FileUrl}", fileUrl);
            }

            // Удаляем миниатюру
            var thumbnailPath = Path.Combine(_uploadPath, $"thumb_{fileName}");
            if (File.Exists(thumbnailPath))
                File.Delete(thumbnailPath);

            return Task.FromResult(Result.Success());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file {FileUrl}", fileUrl);
            return Task.FromResult(Result.Failure(PhotoErrors.DeleteFailed(ex.Message)));
        }
    }

    // ================================================================
    // === THUMBNAIL ===
    // ================================================================
    public async Task<Result<string>> GenerateThumbnailAsync(
        string originalUrl,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var fileName = Path.GetFileName(new Uri(originalUrl).LocalPath);
            var sourcePath = Path.Combine(_uploadPath, fileName);
            var thumbnailName = $"thumb_{fileName}";
            var thumbnailPath = Path.Combine(_uploadPath, thumbnailName);

            if (!File.Exists(thumbnailPath))
            {
                var result = await GenerateThumbnailInternalAsync(sourcePath, thumbnailPath);
                if (result.IsFailure)
                    return Result.Failure<string>(result.Error);
            }

            return Result.Success($"{_baseUrl}/uploads/{thumbnailName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate thumbnail for {Url}", originalUrl);
            return Result.Failure<string>(PhotoErrors.ThumbnailGenerationFailed);
        }
    }

    // ================================================================
    // === PRIVATE HELPERS ===
    // ================================================================

    private async Task<Result> GenerateThumbnailAsync(string sourcePath, string destPath)
    {
        try
        {
            using var image = await Image.LoadAsync(sourcePath);

            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Crop,
                Size = new Size(_thumbnailSize, _thumbnailSize),
                Position = AnchorPositionMode.Center
            }));

            await image.SaveAsync(destPath);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Thumbnail generation failed for {Path}", sourcePath);
            return Result.Failure(PhotoErrors.ThumbnailGenerationFailed);
        }
    }

    private async Task<Result> GenerateThumbnailInternalAsync(string sourcePath, string destPath)
    {
        return await GenerateThumbnailAsync(sourcePath, destPath);
    }
}
