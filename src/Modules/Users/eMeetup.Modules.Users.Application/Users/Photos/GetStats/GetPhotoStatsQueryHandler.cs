using eMeetup.Common.Application.Messaging;
using eMeetup.Common.Domain;
using eMeetup.Modules.Users.Application.Abstractions.Authentication;
using eMeetup.Modules.Users.Domain.Interfaces.Repositories;
using eMeetup.Modules.Users.Domain.Photos;
using eMeetup.Modules.Users.Domain.Users;
using Microsoft.Extensions.Logging;

namespace eMeetup.Modules.Users.Application.Users.Photos.GetStats;

internal sealed class GetPhotoStatsQueryHandler(
    IUserRepository userRepository,
    IUserContext userContext,
    ILogger<GetPhotoStatsQueryHandler> logger)
    : IQueryHandler<GetPhotoStatsQuery, PhotoStatsResponse>
{
    private const int MaxPhotos = 10;

    public async Task<Result<PhotoStatsResponse>> Handle(
        GetPhotoStatsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;

        try
        {
            var user = await userRepository.GetByIdWithPhotosAsync(userId, cancellationToken);
            if (user is null)
                return Result.Failure<PhotoStatsResponse>(UserErrors.NotFound(userId.ToString()));

            var photos = user.Photos.ToList();
            var primaryPhoto = photos.FirstOrDefault(p => p.IsPrimary);
            var lastUploaded = photos.Any()
                ? photos.Max(p => p.UploadedAt)
                : (DateTime?)null;
            var totalSize = photos.Sum(p => p.FileSize ?? 0);

            return Result.Success(new PhotoStatsResponse(
                TotalPhotos: photos.Count,
                MaxPhotos: MaxPhotos,
                RemainingSlots: Math.Max(0, MaxPhotos - photos.Count),
                HasPrimaryPhoto: primaryPhoto is not null,
                PrimaryPhotoUrl: primaryPhoto?.Url,
                LastUploadedAt: lastUploaded,
                TotalFileSize: totalSize));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get photo stats for user {UserId}", userId);
            return Result.Failure<PhotoStatsResponse>(PhotoErrors.InvalidOperation(ex.Message));
        }
    }
}
