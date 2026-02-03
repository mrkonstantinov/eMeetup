using System.Text.Json.Serialization;

namespace eMeetup.Modules.Users.Domain.Photos;

/// <summary>
/// Represents a failed photo upload attempt.
/// </summary>
public sealed class PhotoUploadFailure
{
    [JsonPropertyName("fileName")]
    public string FileName { get; set; } = string.Empty;

    [JsonPropertyName("errorMessage")]
    public string ErrorMessage { get; set; } = string.Empty;

    [JsonPropertyName("errorCode")]
    public string? ErrorCode { get; set; }

    [JsonPropertyName("attemptedAt")]
    public DateTime AttemptedAt { get; set; }

    [JsonPropertyName("sizeBytes")]
    public long? SizeBytes { get; set; }

    [JsonPropertyName("contentType")]
    public string? ContentType { get; set; }
}

