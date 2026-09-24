using eMeetup.Common.Application.Messaging;
using eMeetup.Common.Domain;
using eMeetup.Modules.Users.Application.Abstractions.Authentication;
using eMeetup.Modules.Users.Application.Users.Photos.GetPhotos;
using eMeetup.Modules.Users.Domain.Interfaces.Repositories;
using eMeetup.Modules.Users.Domain.Photos;
using eMeetup.Modules.Users.Domain.Users;
using Microsoft.Extensions.Logging;

namespace eMeetup.Modules.Users.Application.Users.Photos.GetPhoto;

public sealed record GetUserPhotoQuery(Guid PhotoId) : IQuery<UserPhotoDto>;

internal sealed class GetUserPhotoQueryHandler(
    IUserRepository userRepository,
    IUserContext userContext,
    ILogger<GetUserPhotoQueryHandler> logger)
    : IQueryHandler<GetUserPhotoQuery, UserPhotoDto>
{
    public async Task<Result<UserPhotoDto>> Handle(
        GetUserPhotoQuery request,
        CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;

        try
        {
            var user = await userRepository.GetByIdWithPhotosAsync(userId, cancellationToken);
            if (user is null)
                return Result.Failure<UserPhotoDto>(UserErrors.NotFound(userId.ToString()));

            var photo = user.Photos.FirstOrDefault(p => p.Id == request.PhotoId);
            if (photo is null)
                return Result.Failure<UserPhotoDto>(PhotoErrors.NotFound(request.PhotoId));

            return Result.Success(new UserPhotoDto(
                Id: photo.Id,
                Url: photo.Url,
                ThumbnailUrl: photo.ThumbnailUrl,
                IsPrimary: photo.IsPrimary,
                DisplayOrder: photo.DisplayOrder,
                FileName: photo.FileName,
                FileSize: photo.FileSize,
                ContentType: photo.ContentType,
                UploadedAt: photo.UploadedAt));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get photo {PhotoId}", request.PhotoId);
            return Result.Failure<UserPhotoDto>(PhotoErrors.InvalidOperation(ex.Message));
        }
    }
}
