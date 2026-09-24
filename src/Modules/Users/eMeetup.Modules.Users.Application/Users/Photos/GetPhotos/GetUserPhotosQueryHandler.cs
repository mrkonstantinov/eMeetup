using eMeetup.Common.Application.Messaging;
using eMeetup.Common.Domain;
using eMeetup.Modules.Users.Application.Abstractions;
using eMeetup.Modules.Users.Application.Abstractions.Authentication;
using eMeetup.Modules.Users.Domain.Errors;
using eMeetup.Modules.Users.Domain.Interfaces.Repositories;
using eMeetup.Modules.Users.Domain.Photos;
using eMeetup.Modules.Users.Domain.Users;
using Microsoft.Extensions.Logging;

namespace eMeetup.Modules.Users.Application.Users.Photos.GetPhotos;

internal sealed class GetUserPhotosQueryHandler(
    IUserRepository userRepository,
    IUserContext userContext,
    ILogger<GetUserPhotosQueryHandler> logger)
    : IQueryHandler<GetUserPhotosQuery, UserPhotosResponse>
{
    private const int MaxPhotos = 10;

    public async Task<Result<UserPhotosResponse>> Handle(
        GetUserPhotosQuery request,
        CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;

        using var loggerScope = logger.BeginScope("GetUserPhotos {UserId}", userId);

        try
        {
            logger.LogInformation("Getting photos for user {UserId}", userId);

            var user = await userRepository.GetByIdWithPhotosAsync(userId, cancellationToken);

            if (user is null)
            {
                logger.LogWarning("User {UserId} not found", userId);
                return Result.Failure<UserPhotosResponse>(UserErrors.NotFound(userId.ToString()));
            }

            var photos = user.Photos
                .OrderBy(p => p.DisplayOrder)
                .ThenBy(p => p.UploadedAt)
                .Select(MapPhoto)
                .ToArray();

            var response = new UserPhotosResponse(
                TotalCount: photos.Length,
                MaxPhotos: MaxPhotos,
                RemainingSlots: Math.Max(0, MaxPhotos - photos.Length),
                PrimaryPhotoUrl: user.ProfileImageUrl,
                Photos: photos);

            logger.LogInformation(
                "Retrieved {Count} photos for user {UserId}",
                photos.Length,
                userId);

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get photos for user {UserId}", userId);
            return Result.Failure<UserPhotosResponse>(
                PhotoErrors.InvalidOperation(ex.Message));
        }
    }

    private static UserPhotoDto MapPhoto(Domain.Photos.UserPhoto photo)
    {
        return new UserPhotoDto(
            Id: photo.Id,
            Url: photo.Url,
            ThumbnailUrl: photo.ThumbnailUrl,
            IsPrimary: photo.IsPrimary,
            DisplayOrder: photo.DisplayOrder,
            FileName: photo.FileName,
            FileSize: photo.FileSize,
            ContentType: photo.ContentType,
            UploadedAt: photo.UploadedAt);
    }
}
