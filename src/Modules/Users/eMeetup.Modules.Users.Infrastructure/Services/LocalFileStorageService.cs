using System.Security.Cryptography;
using eMeetup.Common.Domain;
using eMeetup.Modules.Users.Domain.Errors;
using eMeetup.Modules.Users.Domain.Interfaces.Services;
using eMeetup.Modules.Users.Domain.Photos;
using eMeetup.Modules.Users.Domain.Users;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace eMeetup.Modules.Users.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly LocalFileStorageOptions _options;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(
        IOptions<LocalFileStorageOptions> options,
        ILogger<LocalFileStorageService> logger)
    {
        _options = options.Value;
        _logger = logger;

        // Ensure the storage directory exists
        EnsureStorageDirectoryExists();
    }

    public async Task<Result<FileUploadResult>> UploadFileAsync(
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate file
            var validationResult = ValidateFile(file);
            if (validationResult.IsFailure)
            {
                return Result.Failure<FileUploadResult>(validationResult.Error);
            }

            // Generate unique filename to prevent collisions
            var uniqueFileName = GenerateUniqueFileName(file.FileName);
            var filePath = GetFilePath(uniqueFileName);
            var relativeUrl = GetRelativeUrl(uniqueFileName);

            // Create directory if it doesn't exist
            var directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory!);
            }

            // Save the file
            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            _logger.LogInformation("File uploaded successfully: {FileName} -> {FilePath}",
                file.FileName, filePath);

            return Result.Success(new FileUploadResult(
                relativeUrl,
                uniqueFileName,
                file.Length));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload file: {FileName}", file.FileName);
            return Result.Failure<FileUploadResult>(UserErrors.UpdateFailed);;
        }
    }

    public async Task<Result> DeleteFileAsync(
        string url,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return Result.Failure(new Error("FileStorage.InvalidUrl", "File URL cannot be empty", ErrorType.Failure));
            }

            var filePath = GetFilePathFromUrl(url);

            // Check if file exists
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("File not found for deletion: {FilePath}", filePath);
                return Result.Failure(new Error("FileStorage.FileNotFound", "File not found", ErrorType.Failure));
            }

            // Delete the file
            File.Delete(filePath);

            _logger.LogInformation("File deleted successfully: {FilePath}", filePath);

            // Optional: Clean up empty directories
            await CleanupEmptyDirectoriesAsync(Path.GetDirectoryName(filePath));

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file: {Url}", url);
            return Result.Failure(
                new Error("FileStorage.DeleteFailed", $"Failed to delete file: {ex.Message}", ErrorType.Failure));
        }
    }

    // Additional utility methods
    public Task<Stream> GetFileStreamAsync(string url, CancellationToken cancellationToken = default)
    {
        try
        {
            var filePath = GetFilePathFromUrl(url);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return Task.FromResult<Stream>(stream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get file stream: {Url}", url);
            throw;
        }
    }

    public Task<bool> FileExistsAsync(string url, CancellationToken cancellationToken = default)
    {
        var filePath = GetFilePathFromUrl(url);
        return Task.FromResult(File.Exists(filePath));
    }

    public Task<string> GetFileContentTypeAsync(string url, CancellationToken cancellationToken = default)
    {
        var filePath = GetFilePathFromUrl(url);
        var extension = Path.GetExtension(filePath).ToLowerInvariant();

        // Simple MIME type mapping
        var mimeTypes = new Dictionary<string, string>
        {
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png",
            [".gif"] = "image/gif",
            [".bmp"] = "image/bmp",
            [".webp"] = "image/webp",
            [".svg"] = "image/svg+xml",
            [".pdf"] = "application/pdf",
            [".txt"] = "text/plain",
            [".csv"] = "text/csv",
            [".json"] = "application/json",
            [".xml"] = "application/xml"
        };

        if (mimeTypes.TryGetValue(extension, out var contentType))
        {
            return Task.FromResult(contentType);
        }

        return Task.FromResult("application/octet-stream");
    }

    // Private helper methods
    private void EnsureStorageDirectoryExists()
    {
        if (!Directory.Exists(_options.StoragePath))
        {
            Directory.CreateDirectory(_options.StoragePath);
            _logger.LogInformation("Created storage directory: {StoragePath}", _options.StoragePath);
        }
    }

    private Result ValidateFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return Result.Failure(new Error("FileStorage.EmptyFile", "File is empty", ErrorType.Failure));
        }

        // Check file size
        if (file.Length > _options.MaxFileSizeBytes)
        {
            return Result.Failure(new Error("FileStorage.FileTooLarge",
                $"File size exceeds maximum allowed size of {_options.MaxFileSizeBytes / 1024 / 1024}MB", ErrorType.Failure));
        }

        // Validate file extension
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!string.IsNullOrEmpty(_options.AllowedExtensions))
        {
            var allowedExtensions = _options.AllowedExtensions
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(e => e.Trim().ToLowerInvariant())
                .ToArray();

            if (allowedExtensions.Length > 0 && !allowedExtensions.Contains(extension))
            {
                return Result.Failure(new Error("FileStorage.InvalidExtension",
                    $"File extension '{extension}' is not allowed. Allowed extensions: {_options.AllowedExtensions}", ErrorType.Failure));
            }
        }

        // Validate MIME type (optional, can be extended)
        var mimeType = file.ContentType;
        if (!string.IsNullOrEmpty(mimeType) && !string.IsNullOrEmpty(_options.AllowedMimeTypes))
        {
            var allowedMimeTypes = _options.AllowedMimeTypes
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(m => m.Trim().ToLowerInvariant())
                .ToArray();

            if (allowedMimeTypes.Length > 0 && !allowedMimeTypes.Contains(mimeType.ToLowerInvariant()))
            {
                return Result.Failure(new Error("FileStorage.InvalidMimeType",
                    $"MIME type '{mimeType}' is not allowed.", ErrorType.Failure));
            }
        }

        return Result.Success();
    }

    private string GenerateUniqueFileName(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName);
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);

        // Sanitize filename
        var sanitizedFileName = SanitizeFileName(fileNameWithoutExtension);

        // Generate unique identifier
        var uniqueId = GenerateUniqueId();

        // Create date-based subdirectory (optional, helps with organization)
        var datePrefix = DateTime.UtcNow.ToString("yyyy/MM/dd");

        // Combine everything
        return $"{datePrefix}/{sanitizedFileName}_{uniqueId}{extension}";
    }

    private string GenerateUniqueId()
    {
        // Use timestamp + random bytes for uniqueness
        var timestamp = DateTime.UtcNow.Ticks;
        var randomBytes = new byte[4];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        var randomNumber = BitConverter.ToUInt32(randomBytes, 0);

        return $"{timestamp:x16}{randomNumber:x8}";
    }

    private string SanitizeFileName(string fileName)
    {
        // Remove invalid characters
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(fileName
            .Where(ch => !invalidChars.Contains(ch))
            .ToArray());

        // Replace spaces with underscores
        sanitized = sanitized.Replace(' ', '_');

        // Limit length
        if (sanitized.Length > 100)
        {
            sanitized = sanitized.Substring(0, 100);
        }

        return sanitized;
    }

    private string GetFilePath(string fileName)
    {
        return Path.Combine(_options.StoragePath, fileName);
    }

    private string GetRelativeUrl(string fileName)
    {
        // For local storage, we might return a relative path or a URL
        if (_options.UseRelativeUrls)
        {
            return $"/{_options.UrlPrefix?.Trim('/')}/{fileName}".Replace('\\', '/');
        }

        return Path.Combine(_options.StoragePath, fileName).Replace('\\', '/');
    }

    private string GetFilePathFromUrl(string url)
    {
        if (_options.UseRelativeUrls)
        {
            // Remove the URL prefix to get the relative path
            var prefix = $"/{_options.UrlPrefix?.Trim('/')}/";
            if (url.StartsWith(prefix))
            {
                url = url.Substring(prefix.Length);
            }
            else if (url.StartsWith("/"))
            {
                url = url.Substring(1);
            }

            return Path.Combine(_options.StoragePath, url);
        }

        // For absolute paths, just use the path
        return url;
    }

    private async Task CleanupEmptyDirectoriesAsync(string? directoryPath)
    {
        if (string.IsNullOrEmpty(directoryPath) || !_options.CleanupEmptyDirectories)
        {
            return;
        }

        try
        {
            // Walk up the directory tree and remove empty directories
            var currentDir = new DirectoryInfo(directoryPath);
            var storageDir = new DirectoryInfo(_options.StoragePath);

            while (currentDir != null &&
                   currentDir.FullName.StartsWith(storageDir.FullName) &&
                   currentDir.FullName != storageDir.FullName)
            {
                if (!currentDir.EnumerateFileSystemInfos().Any())
                {
                    currentDir.Delete();
                    _logger.LogDebug("Cleaned up empty directory: {Directory}", currentDir.FullName);
                    currentDir = currentDir.Parent;
                }
                else
                {
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            // Don't fail the main operation if cleanup fails
            _logger.LogWarning(ex, "Failed to cleanup empty directories");
        }
    }
}


// Configuration options class
public class LocalFileStorageOptions
{
    public string StoragePath { get; set; } = "wwwroot/uploads";
    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024; // 10MB default
    public string? AllowedExtensions { get; set; } = ".jpg,.jpeg,.png,.gif,.bmp,.webp,.pdf";
    public string? AllowedMimeTypes { get; set; }
    public bool UseRelativeUrls { get; set; } = true;
    public string UrlPrefix { get; set; } = "uploads";
    public bool CleanupEmptyDirectories { get; set; } = true;

    // Optional: Configure subdirectory structure
    public string DateFormat { get; set; } = "yyyy/MM/dd";

    // Optional: File naming strategy
    public FileNamingStrategy NamingStrategy { get; set; } = FileNamingStrategy.Unique;
}

public enum FileNamingStrategy
{
    Original,      // Keep original filename (risk of collisions)
    Unique,        // Generate unique names (default)
    Guid,          // Use GUID as filename
    Timestamp      // Use timestamp
}

// Extension method for easy registration
public static class LocalFileStorageServiceExtensions
{
    public static IServiceCollection AddLocalFileStorage(
        this IServiceCollection services,
        Action<LocalFileStorageOptions>? configureOptions = null)
    {
        services.Configure<LocalFileStorageOptions>(options =>
        {
            configureOptions?.Invoke(options);

            // Set defaults if not configured
            if (string.IsNullOrEmpty(options.StoragePath))
            {
                options.StoragePath = "wwwroot/uploads";
            }
        });

        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        return services;
    }
}



// Optional: Health check for file storage
public class LocalFileStorageHealthCheck : IHealthCheck
{
    private readonly IOptions<LocalFileStorageOptions> _options;
    private readonly ILogger<LocalFileStorageHealthCheck> _logger;

    public LocalFileStorageHealthCheck(
        IOptions<LocalFileStorageOptions> options,
        ILogger<LocalFileStorageHealthCheck> logger)
    {
        _options = options;
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var options = _options.Value;

            // Check if storage directory exists and is accessible
            var directory = new DirectoryInfo(options.StoragePath);

            if (!directory.Exists)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy(
                    "Storage directory does not exist",
                    data: new Dictionary<string, object>
                    {
                        ["StoragePath"] = options.StoragePath
                    }));
            }

            // Try to create a test file to check write permissions
            var testFilePath = Path.Combine(options.StoragePath, $"healthcheck_{Guid.NewGuid()}.tmp");
            File.WriteAllText(testFilePath, "Health check test");
            File.Delete(testFilePath);

            return Task.FromResult(HealthCheckResult.Healthy(
                "Local file storage is healthy",
                data: new Dictionary<string, object>
                {
                    ["StoragePath"] = options.StoragePath,
                    ["AvailableSpace"] = GetAvailableSpace(directory)
                }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Local file storage health check failed");
            return Task.FromResult(HealthCheckResult.Unhealthy(
                "Local file storage health check failed",
                ex));
        }
    }

    private static string GetAvailableSpace(DirectoryInfo directory)
    {
        try
        {
            var drive = new DriveInfo(directory.Root.FullName);
            return $"{drive.AvailableFreeSpace / 1024 / 1024:N0} MB";
        }
        catch
        {
            return "Unknown";
        }
    }
}
