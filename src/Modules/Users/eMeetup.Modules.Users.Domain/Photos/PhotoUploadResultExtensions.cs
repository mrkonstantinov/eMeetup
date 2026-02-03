using System;
using System.Collections.Generic;
using System.Text;

namespace eMeetup.Modules.Users.Domain.Photos;

/// <summary>
/// Extension methods for PhotoUploadResult.
/// </summary>
public static class PhotoUploadResultExtensions
{
    /// <summary>
    /// Checks if the result contains a profile picture.
    /// </summary>
    public static bool HasProfilePicture(this PhotoUploadResult result)
    {
        return !string.IsNullOrEmpty(result.ProfilePictureUrl);
    }

    /// <summary>
    /// Gets all photo URLs except the profile picture.
    /// </summary>
    public static IEnumerable<string> GetAdditionalPhotos(this PhotoUploadResult result)
    {
        return result.AllPhotoUrls.Where(url => url != result.ProfilePictureUrl);
    }

    /// <summary>
    /// Gets the photo at a specific index.
    /// </summary>
    public static string? GetPhotoAt(this PhotoUploadResult result, int index)
    {
        return index >= 0 && index < result.AllPhotoUrls.Count
            ? result.AllPhotoUrls[index]
            : null;
    }

    /// <summary>
    /// Creates a formatted error message from failures.
    /// </summary>
    public static string GetFailureSummary(this PhotoUploadResult result)
    {
        if (!result.HasFailures)
            return "No failures";

        return string.Join("; ", result.Failures.Select(f =>
            $"{f.FileName}: {f.ErrorMessage}"));
    }
}
