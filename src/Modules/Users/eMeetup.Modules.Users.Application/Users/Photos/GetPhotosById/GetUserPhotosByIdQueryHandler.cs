using eMeetup.Common.Application.Messaging;
using eMeetup.Common.Domain;
using eMeetup.Modules.Users.Application.Users.Photos.GetPhotos;
using eMeetup.Modules.Users.Domain.Interfaces.Repositories;
using eMeetup.Modules.Users.Domain.Photos;
using eMeetup.Modules.Users.Domain.Users;
using Microsoft.Extensions.Logging;

namespace eMeetup.Modules.Users.Application.Users.Photos.GetPhotosById;

internal sealed class GetUserPhotosByIdQueryHandler(
    IUserRepository userRepository,
    ILogger<GetUserPhotosByIdQueryHandler> logger)
    : IQueryHandler<GetUserPhotosByIdQuery, UserPhotosResponse>
{
    private const int MaxPhotos = 10;

    public async Task<Result<UserPhotosResponse>> Handle(
        GetUserPhotosByIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await userRepository.GetByIdWithPhotosAsync(
                request.UserId, cancellationToken);

            if (user is null)
                return Result.Failure<UserPhotosResponse>(UserErrors.NotFound(request.UserId.ToString()));

            // Проверка: можно ли смотреть фото этого пользователя
            if (!user.CanBeSearched())
                return Result.Failure<UserPhotosResponse>(ProfileErrors.ProfileIsPrivate);

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

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to get photos for user {UserId}",
                request.UserId);

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
