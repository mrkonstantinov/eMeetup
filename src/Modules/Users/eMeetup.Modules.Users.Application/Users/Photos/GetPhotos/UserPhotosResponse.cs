namespace eMeetup.Modules.Users.Application.Users.Photos.GetPhotos;

public sealed record UserPhotosResponse(
    int TotalCount,
    int MaxPhotos,
    int RemainingSlots,
    string? PrimaryPhotoUrl,
    UserPhotoDto[] Photos);

public sealed record UserPhotoDto(
    Guid Id,
    string Url,
    string? ThumbnailUrl,
    bool IsPrimary,
    int DisplayOrder,
    string? FileName,
    long? FileSize,
    string? ContentType,
    DateTime UploadedAt);
