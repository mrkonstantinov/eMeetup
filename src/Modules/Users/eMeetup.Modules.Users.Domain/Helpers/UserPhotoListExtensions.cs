using eMeetup.Modules.Users.Domain.Photos;

namespace eMeetup.Modules.Users.Domain.Helpers;

public static class UserPhotoListExtensions
{
    public static string? GetPrimaryUrl(this List<UserPhoto> photos)
        => photos.FirstOrDefault(p => p.IsPrimary)?.Url;

    public static UserPhoto? GetPrimaryPhoto(this List<UserPhoto> photos)
        => photos.FirstOrDefault(p => p.IsPrimary);

    public static bool IsPrimary(this List<UserPhoto> photos, Guid photoId)
        => photos.Any(p => p.Id == photoId && p.IsPrimary);

    public static bool HasPhotos(this List<UserPhoto> photos)
        => photos.Any();

    public static IReadOnlyList<UserPhoto> GetOrderedPhotos(this List<UserPhoto> photos)
        => photos
            .OrderBy(p => p.DisplayOrder)
            .ThenBy(p => p.UploadedAt)
            .ToList()
            .AsReadOnly();
}
