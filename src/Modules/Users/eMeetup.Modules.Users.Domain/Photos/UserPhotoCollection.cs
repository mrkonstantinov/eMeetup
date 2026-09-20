using eMeetup.Common.Domain;
using eMeetup.Modules.Users.Domain.Users;

namespace eMeetup.Modules.Users.Domain.Photos;

public class UserPhotoCollection
{
    private List<UserPhoto> _photos = new();

    public UserPhotoCollection()
    {
    }

    public UserPhotoCollection(IEnumerable<UserPhoto> photos)
    {
        _photos = photos?.ToList() ?? new List<UserPhoto>();
    }

    public Result Add(Guid userId, string url, bool isPrimary = false)
    {
        if (userId == Guid.Empty)
            return Result.Failure(PhotoErrors.InvalidUserId);

        if (string.IsNullOrWhiteSpace(url))
            return Result.Failure(PhotoErrors.EmptyPhoto);

        if (!IsValidUrl(url))
            return Result.Failure(PhotoErrors.InvalidUrl);

        if (_photos.Count >= 10)
            return Result.Failure(PhotoErrors.TooManyPhotos(10));

        var displayOrder = _photos.Count;
        var shouldBePrimary = isPrimary || !_photos.Any();

        var photo = UserPhoto.Create(userId, url, displayOrder, shouldBePrimary);
        if (photo.IsFailure)
            return Result.Failure(photo.Error);

        // Если новое фото primary - сбрасываем у остальных
        if (shouldBePrimary)
        {
            foreach (var p in _photos)
            {
                p.SetAsSecondary();
            }
        }

        _photos.Add(photo.Value);
        return Result.Success();
    }

    public Result SetPrimary(Guid photoId)
    {
        var photo = _photos.FirstOrDefault(p => p.Id == photoId);
        if (photo == null)
            return Result.Failure(PhotoErrors.NotFound(photoId));

        // Сбрасываем все primary
        foreach (var p in _photos)
        {
            p.SetAsSecondary();
        }

        photo.SetAsPrimary();
        return Result.Success();
    }

    public Result Remove(Guid photoId)
    {
        var photo = _photos.FirstOrDefault(p => p.Id == photoId);
        if (photo == null)
            return Result.Failure(PhotoErrors.NotFound(photoId));

        var wasPrimary = photo.IsPrimary;
        _photos.Remove(photo);

        // Если удалили primary и есть другие фото - делаем первое primary
        if (wasPrimary && _photos.Any())
        {
            _photos.First().SetAsPrimary();
        }

        return Result.Success();
    }

    public Result Reorder(Dictionary<Guid, int> orderMap)
    {
        if (orderMap == null || !orderMap.Any())
            return Result.Success();

        foreach (var (photoId, newOrder) in orderMap)
        {
            var photo = _photos.FirstOrDefault(p => p.Id == photoId);
            if (photo == null)
                return Result.Failure(PhotoErrors.NotFound(photoId));

            if (newOrder < 0)
                return Result.Failure(PhotoErrors.InvalidDisplayOrder);

            var result = photo.UpdateDisplayOrder(newOrder);
            if (result.IsFailure)
                return result;
        }

        // Сортируем по новому порядку
        _photos = _photos.OrderBy(p => p.DisplayOrder).ToList();
        return Result.Success();
    }

    public string? GetPrimaryUrl()
    {
        return _photos.FirstOrDefault(p => p.IsPrimary)?.Url;
    }

    public UserPhoto? GetPrimaryPhoto()
    {
        return _photos.FirstOrDefault(p => p.IsPrimary);
    }

    public bool IsPrimary(Guid photoId)
    {
        return _photos.Any(p => p.Id == photoId && p.IsPrimary);
    }

    public bool HasPhotos()
    {
        return _photos.Any();
    }

    public int Count => _photos.Count;

    public IReadOnlyList<UserPhoto> ToList()
    {
        return _photos
            .OrderBy(p => p.DisplayOrder)
            .ThenBy(p => p.UploadedAt)
            .ToList()
            .AsReadOnly();
    }

    public IReadOnlyList<UserPhoto> GetOrderedPhotos()
    {
        return ToList();
    }

    public void Clear()
    {
        _photos.Clear();
    }

    private static bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}
