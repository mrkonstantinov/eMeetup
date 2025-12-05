using eMeetup.Common.Domain;
using eMeetup.Modules.Users.Domain.Errors;
using eMeetup.Modules.Users.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace eMeetup.Modules.Users.Infrastructure.Services;
public class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;
    private readonly string _baseUrl;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(
        IWebHostEnvironment environment,
        IConfiguration configuration,
        ILogger<LocalFileStorageService> logger)
    {
        _logger = logger;

        _basePath = configuration["FileStorage:Local:BasePath"]
                   ?? Path.Combine(environment.WebRootPath, "uploads");

        _baseUrl = configuration["FileStorage:Local:BaseUrl"]
                  ?? "/uploads";

        Directory.CreateDirectory(_basePath);
        Directory.CreateDirectory(Path.Combine(_basePath, "photos"));
    }

    public async Task<Result<string>> UploadPhotoAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (fileStream == null || fileStream.Length == 0)
                return Result.Failure<string>(FileStorageErrors.EmptyFile);

            if (!IsValidImageContentType(contentType))
                return Result.Failure<string>(FileStorageErrors.InvalidFileType);

            var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var relativePath = Path.Combine("photos", uniqueFileName);
            var fullPath = Path.Combine(_basePath, relativePath);
            var fileUrl = $"{_baseUrl}/{relativePath.Replace('\\', '/')}";

            var directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await using var outputStream = new FileStream(fullPath, FileMode.Create);
            await fileStream.CopyToAsync(outputStream, cancellationToken);

            _logger.LogInformation("File uploaded successfully: {FileName} -> {FileUrl}", fileName, fileUrl);

            return fileUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file: {FileName}", fileName);
            return Result.Failure<string>(FileStorageErrors.UploadFailed);
        }
    }

    public async Task<Result> DeletePhotoAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(fileUrl))
                return Result.Failure(FileStorageErrors.InvalidFileUrl);

            var filePath = UrlToFilePath(fileUrl);

            if (!File.Exists(filePath))
            {
                return Result.Failure(FileStorageErrors.FileNotFound);
            }

            File.Delete(filePath);

            _logger.LogInformation("File deleted successfully: {FileUrl}", fileUrl);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FileUrl}", fileUrl);
            return Result.Failure(FileStorageErrors.DeleteFailed);
        }
    }

    public async Task<bool> ExistsAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(fileUrl))
                return false;

            var filePath = UrlToFilePath(fileUrl);
            return File.Exists(filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking file existence: {FileUrl}", fileUrl);
            return false;
        }
    }

    public async Task<string?> GetContentTypeAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(fileUrl))
                return null;

            var fileExtension = Path.GetExtension(fileUrl).ToLowerInvariant();
            return fileExtension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting content type for file: {FileUrl}", fileUrl);
            return null;
        }
    }

    private string UrlToFilePath(string fileUrl)
    {
        var relativePath = fileUrl.Replace(_baseUrl, "").TrimStart('/');
        return Path.Combine(_basePath, relativePath);
    }

    private static bool IsValidImageContentType(string contentType)
    {
        var validContentTypes = new[]
        {
            "image/jpeg",
            "image/png",
            "image/gif",
            "image/webp"
        };
        return validContentTypes.Contains(contentType.ToLower());
    }
}
