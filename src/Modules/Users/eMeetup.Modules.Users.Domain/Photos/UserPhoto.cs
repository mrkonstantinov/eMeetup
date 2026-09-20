using eMeetup.Common.Domain;
using eMeetup.Modules.Users.Domain.Users;

namespace eMeetup.Modules.Users.Domain.Photos;

public class UserPhoto
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Url { get; private set; }
    public string? ThumbnailUrl { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsPrimary { get; private set; }
    public DateTime UploadedAt { get; private set; }
    public string? FileName { get; private set; }
    public long? FileSize { get; private set; }
    public string? ContentType { get; private set; }

    private UserPhoto() { } // EF Core

    private UserPhoto(
        Guid userId,
        string url,
        int displayOrder,
        bool isPrimary,
        string? fileName = null,
        long? fileSize = null,
        string? contentType = null,
        string? thumbnailUrl = null)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Url = url;
        ThumbnailUrl = thumbnailUrl;
        DisplayOrder = displayOrder;
        IsPrimary = isPrimary;
        UploadedAt = DateTime.UtcNow;
        FileName = fileName;
        FileSize = fileSize;
        ContentType = contentType;
    }

    public static Result<UserPhoto> Create(
        Guid userId,
        string url,
        int displayOrder,
        bool isPrimary = false,
        string? fileName = null,
        long? fileSize = null,
        string? contentType = null,
        string? thumbnailUrl = null)
    {
        if (userId == Guid.Empty)
            return Result.Failure<UserPhoto>(PhotoErrors.InvalidUserId);

        if (string.IsNullOrWhiteSpace(url))
            return Result.Failure<UserPhoto>(PhotoErrors.EmptyUrl);

        if (!IsValidUrl(url))
            return Result.Failure<UserPhoto>(PhotoErrors.InvalidUrl);

        if (displayOrder < 0)
            return Result.Failure<UserPhoto>(PhotoErrors.InvalidDisplayOrder);

        if (fileSize.HasValue && fileSize > 10 * 1024 * 1024) // 10MB
            return Result.Failure<UserPhoto>(PhotoErrors.FileTooLarge(10 * 1024 * 1024));

        if (!string.IsNullOrEmpty(contentType) && !IsValidContentType(contentType))
            return Result.Failure<UserPhoto>(PhotoErrors.InvalidFileType);

        var photo = new UserPhoto(
            userId,
            url,
            displayOrder,
            isPrimary,
            fileName,
            fileSize,
            contentType,
            thumbnailUrl);

        return Result<UserPhoto>.Success(photo);
    }

    public Result UpdateDisplayOrder(int newOrder)
    {
        if (newOrder < 0)
            return Result.Failure(PhotoErrors.InvalidDisplayOrder);

        DisplayOrder = newOrder;
        return Result.Success();
    }

    public void SetAsPrimary()
    {
        IsPrimary = true;
    }

    public void SetAsSecondary()
    {
        IsPrimary = false;
    }

    public Result UpdateUrl(string newUrl)
    {
        if (string.IsNullOrWhiteSpace(newUrl))
            return Result.Failure(PhotoErrors.EmptyUrl);

        if (!IsValidUrl(newUrl))
            return Result.Failure(PhotoErrors.InvalidUrl);

        Url = newUrl;
        return Result.Success();
    }

    public void UpdateThumbnail(string thumbnailUrl)
    {
        ThumbnailUrl = thumbnailUrl;
    }

    public Result UpdateFileInfo(string? fileName, long? fileSize, string? contentType)
    {
        if (fileSize.HasValue && fileSize > 10 * 1024 * 1024)
            return Result.Failure(PhotoErrors.FileTooLarge(10 * 1024 * 1024));

        if (!string.IsNullOrEmpty(contentType) && !IsValidContentType(contentType))
            return Result.Failure(PhotoErrors.InvalidFileType);

        FileName = fileName;
        FileSize = fileSize;
        ContentType = contentType;
        return Result.Success();
    }

    private static bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }

    private static bool IsValidContentType(string contentType)
    {
        var allowedTypes = new[]
        {
            "image/jpeg",
            "image/png",
            "image/gif",
            "image/webp",
            "image/svg+xml"
        };
        return allowedTypes.Contains(contentType.ToLowerInvariant());
    }
}
