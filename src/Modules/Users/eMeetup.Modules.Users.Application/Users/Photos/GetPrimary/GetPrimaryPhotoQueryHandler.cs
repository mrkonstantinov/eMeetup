using eMeetup.Common.Application.Messaging;
using eMeetup.Common.Domain;
using eMeetup.Modules.Users.Application.Abstractions.Authentication;
using eMeetup.Modules.Users.Application.Users.Photos.GetPhotos;
using eMeetup.Modules.Users.Domain.Interfaces.Repositories;
using eMeetup.Modules.Users.Domain.Photos;
using eMeetup.Modules.Users.Domain.Users;
using Microsoft.Extensions.Logging;

namespace eMeetup.Modules.Users.Application.Users.Photos.GetPrimary;

internal sealed class GetPrimaryPhotoQueryHandler(
    IUserRepository userRepository,
    IUserContext userContext,
    ILogger<GetPrimaryPhotoQueryHandler> logger)
    : IQueryHandler<GetPrimaryPhotoQuery, UserPhotoDto?>
{
    public async Task<Result<UserPhotoDto?>> Handle(
        GetPrimaryPhotoQuery request,
        CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;

        try
        {
            var user = await userRepository.GetByIdWithPhotosAsync(userId, cancellationToken);
            if (user is null)
                return Result.Failure<UserPhotoDto?>(UserErrors.NotFound(userId.ToString()));

            var primaryPhoto = user.Photos.FirstOrDefault(p => p.IsPrimary);
            if (primaryPhoto is null)
                return Result.Success<UserPhotoDto?>(null);

            return Result.Success<UserPhotoDto?>(new UserPhotoDto(
                Id: primaryPhoto.Id,
                Url: primaryPhoto.Url,
                ThumbnailUrl: primaryPhoto.ThumbnailUrl,
                IsPrimary: primaryPhoto.IsPrimary,
                DisplayOrder: primaryPhoto.DisplayOrder,
                FileName: primaryPhoto.FileName,
                FileSize: primaryPhoto.FileSize,
                ContentType: primaryPhoto.ContentType,
                UploadedAt: primaryPhoto.UploadedAt));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get primary photo for user {UserId}", userId);
            return Result.Failure<UserPhotoDto?>(PhotoErrors.InvalidOperation(ex.Message));
        }
    }
}
