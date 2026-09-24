using eMeetup.Common.Application.Messaging;

namespace eMeetup.Modules.Users.Application.Users.Photos.GetStats;

public sealed record GetPhotoStatsQuery : IQuery<PhotoStatsResponse>;

public sealed record PhotoStatsResponse(
    int TotalPhotos,
    int MaxPhotos,
    int RemainingSlots,
    bool HasPrimaryPhoto,
    string? PrimaryPhotoUrl,
    DateTime? LastUploadedAt,
    long TotalFileSize);
