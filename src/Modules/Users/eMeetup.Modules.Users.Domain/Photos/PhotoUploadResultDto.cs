using System.Text.Json.Serialization;

namespace eMeetup.Modules.Users.Domain.Photos;

/// <summary>
/// Simplified DTO for API responses.
/// </summary>
public sealed class PhotoUploadResultDto
{
    [JsonPropertyName("profilePictureUrl")]
    public string? ProfilePictureUrl { get; set; }

    [JsonPropertyName("allPhotoUrls")]
    public string[] AllPhotoUrls { get; set; } = Array.Empty<string>();

    [JsonPropertyName("uploadedCount")]
    public int UploadedCount { get; set; }

    [JsonPropertyName("failedCount")]
    public int FailedCount { get; set; }

    [JsonPropertyName("totalSizeBytes")]
    public long TotalSizeBytes { get; set; }

    [JsonPropertyName("operationId")]
    public string? OperationId { get; set; }

    [JsonPropertyName("durationMs")]
    public double? DurationMs { get; set; }

    [JsonPropertyName("success")]
    public bool Success => FailedCount == 0;
}

