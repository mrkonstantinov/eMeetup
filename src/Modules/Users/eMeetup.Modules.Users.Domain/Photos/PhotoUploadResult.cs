using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace eMeetup.Modules.Users.Domain.Photos;

public sealed class PhotoUploadResult
{
    /// <summary>
    /// Gets or sets the URL of the primary/profile photo.
    /// This is typically the first photo in the upload batch.
    /// </summary>
    [JsonPropertyName("profilePictureUrl")]
    public string? ProfilePictureUrl { get; set; }

    /// <summary>
    /// Gets or sets the list of URLs for all uploaded photos.
    /// </summary>
    [JsonPropertyName("allPhotoUrls")]
    public List<string> AllPhotoUrls { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets the count of successfully uploaded photos.
    /// </summary>
    [JsonPropertyName("uploadedCount")]
    public int UploadedCount { get; set; }

    /// <summary>
    /// Gets or sets the count of photos that failed to upload.
    /// </summary>
    [JsonPropertyName("failedCount")]
    public int FailedCount { get; set; }

    /// <summary>
    /// Gets or sets the list of photo upload failures, if any.
    /// </summary>
    [JsonPropertyName("failures")]
    public List<PhotoUploadFailure> Failures { get; set; } = new List<PhotoUploadFailure>();

    /// <summary>
    /// Gets a value indicating whether any photos were uploaded.
    /// </summary>
    [JsonIgnore]
    public bool HasPhotos => UploadedCount > 0;

    /// <summary>
    /// Gets a value indicating whether all photos were uploaded successfully.
    /// </summary>
    [JsonIgnore]
    public bool AllSucceeded => FailedCount == 0;

    /// <summary>
    /// Gets a value indicating whether any uploads failed.
    /// </summary>
    [JsonIgnore]
    public bool HasFailures => FailedCount > 0;

    /// <summary>
    /// Gets the primary photo URL (alias for ProfilePictureUrl).
    /// Provided for backward compatibility.
    /// </summary>
    [JsonIgnore]
    public string? PrimaryPhotoUrl => ProfilePictureUrl;

    /// <summary>
    /// Gets the file names of all uploaded photos.
    /// </summary>
    [JsonIgnore]
    public List<string> UploadedFileNames { get; set; } = new List<string>();

    /// <summary>
    /// Gets the total size of uploaded photos in bytes.
    /// </summary>
    [JsonPropertyName("totalSizeBytes")]
    public long TotalSizeBytes { get; set; }

    /// <summary>
    /// Gets or sets the operation identifier for tracking.
    /// </summary>
    [JsonPropertyName("operationId")]
    public string? OperationId { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the upload started.
    /// </summary>
    [JsonPropertyName("startedAt")]
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the timestamp when the upload completed.
    /// </summary>
    [JsonPropertyName("completedAt")]
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Gets the duration of the upload operation.
    /// </summary>
    [JsonIgnore]
    public TimeSpan? Duration => CompletedAt.HasValue ? CompletedAt.Value - StartedAt : null;

    /// <summary>
    /// Adds a successful photo upload to the result.
    /// </summary>
    /// <param name="url">The URL of the uploaded photo.</param>
    /// <param name="fileName">The original file name.</param>
    /// <param name="sizeBytes">The size of the photo in bytes.</param>
    /// <param name="isPrimary">Whether this is the primary/profile photo.</param>
    public void AddSuccess(string url, string fileName, long sizeBytes, bool isPrimary = false)
    {
        AllPhotoUrls.Add(url);
        UploadedFileNames.Add(fileName);
        UploadedCount++;
        TotalSizeBytes += sizeBytes;

        if (isPrimary)
        {
            ProfilePictureUrl = url;
        }
    }

    /// <summary>
    /// Adds a failed photo upload to the result.
    /// </summary>
    /// <param name="fileName">The file name that failed.</param>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="errorCode">The error code.</param>
    public void AddFailure(string fileName, string errorMessage, string? errorCode = null)
    {
        FailedCount++;
        Failures.Add(new PhotoUploadFailure
        {
            FileName = fileName,
            ErrorMessage = errorMessage,
            ErrorCode = errorCode,
            AttemptedAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Marks the upload operation as completed.
    /// </summary>
    public void MarkCompleted()
    {
        CompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a summary of the upload operation.
    /// </summary>
    /// <returns>A summary string.</returns>
    public string GetSummary()
    {
        var duration = Duration.HasValue ? $"{Duration.Value.TotalSeconds:F1}s" : "in progress";
        return $"""
            Photo Upload Result:
            - Operation ID: {OperationId ?? "N/A"}
            - Status: {(AllSucceeded ? "Success" : "Partial")}
            - Uploaded: {UploadedCount} photo(s)
            - Failed: {FailedCount} photo(s)
            - Total Size: {FormatBytes(TotalSizeBytes)}
            - Duration: {duration}
            - Profile Picture: {(!string.IsNullOrEmpty(ProfilePictureUrl) ? "✓" : "✗")}
            """;
    }

    private static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        int order = 0;
        double len = bytes;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    /// <summary>
    /// Creates a success result with a single photo.
    /// </summary>
    /// <param name="url">The photo URL.</param>
    /// <param name="fileName">The file name.</param>
    /// <param name="sizeBytes">The file size.</param>
    /// <param name="isProfilePhoto">Whether this is the profile photo.</param>
    /// <returns>A PhotoUploadResult instance.</returns>
    public static PhotoUploadResult CreateSuccess(
        string url,
        string fileName,
        long sizeBytes,
        bool isProfilePhoto = true)
    {
        var result = new PhotoUploadResult
        {
            OperationId = Guid.NewGuid().ToString()
        };

        result.AddSuccess(url, fileName, sizeBytes, isProfilePhoto);
        result.MarkCompleted();

        return result;
    }

    /// <summary>
    /// Creates a failure result.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="errorCode">The error code.</param>
    /// <returns>A PhotoUploadResult instance.</returns>
    public static PhotoUploadResult CreateFailure(string errorMessage, string? errorCode = null)
    {
        return new PhotoUploadResult
        {
            OperationId = Guid.NewGuid().ToString(),
            FailedCount = 1,
            Failures = new List<PhotoUploadFailure>
            {
                new PhotoUploadFailure
                {
                    FileName = "Unknown",
                    ErrorMessage = errorMessage,
                    ErrorCode = errorCode,
                    AttemptedAt = DateTime.UtcNow
                }
            },
            CompletedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Merges multiple PhotoUploadResult instances into one.
    /// </summary>
    /// <param name="results">The results to merge.</param>
    /// <returns>A merged PhotoUploadResult.</returns>
    public static PhotoUploadResult Merge(params PhotoUploadResult[] results)
    {
        var merged = new PhotoUploadResult
        {
            OperationId = Guid.NewGuid().ToString(),
            StartedAt = results.Min(r => r.StartedAt)
        };

        foreach (var result in results)
        {
            // Merge successful uploads
            for (int i = 0; i < result.AllPhotoUrls.Count; i++)
            {
                var url = result.AllPhotoUrls[i];
                var fileName = i < result.UploadedFileNames.Count ?
                    result.UploadedFileNames[i] : "Unknown";

                // We don't have size info in merge, use 0
                merged.AddSuccess(url, fileName, 0, url == result.ProfilePictureUrl);
            }

            // Merge failures
            merged.FailedCount += result.FailedCount;
            merged.Failures.AddRange(result.Failures);
            merged.TotalSizeBytes += result.TotalSizeBytes;
        }

        merged.MarkCompleted();
        return merged;
    }

    /// <summary>
    /// Converts the result to a simplified DTO for API responses.
    /// </summary>
    /// <returns>A simplified DTO.</returns>
    public PhotoUploadResultDto ToDto()
    {
        return new PhotoUploadResultDto
        {
            ProfilePictureUrl = ProfilePictureUrl,
            AllPhotoUrls = AllPhotoUrls.ToArray(),
            UploadedCount = UploadedCount,
            FailedCount = FailedCount,
            TotalSizeBytes = TotalSizeBytes,
            OperationId = OperationId,
            DurationMs = Duration?.TotalMilliseconds
        };
    }
}
