using eMeetup.Common.Application.Messaging;
using eMeetup.Common.Domain;
using eMeetup.Modules.Users.Application.Abstractions;
using eMeetup.Modules.Users.Application.Abstractions.Authentication;
using eMeetup.Modules.Users.Application.Abstractions.Data;
using eMeetup.Modules.Users.Domain.Errors;
using eMeetup.Modules.Users.Domain.Interfaces.Repositories;
using eMeetup.Modules.Users.Domain.Photos;
using eMeetup.Modules.Users.Domain.Users;
using Microsoft.Extensions.Logging;

namespace eMeetup.Modules.Users.Application.Users.Photos.Remove;

internal sealed class RemoveUserPhotoCommandHandler(
    IUserRepository userRepository,
    IFileUploadService fileUploadService,
    IUserContext userContext,
    IUnitOfWork unitOfWork,
    ILogger<RemoveUserPhotoCommandHandler> logger)
    : ICommandHandler<RemoveUserPhotoCommand>
{
    public async Task<Result> Handle(
        RemoveUserPhotoCommand request,
        CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;

        using var loggerScope = logger.BeginScope(
            "RemoveUserPhoto {UserId} {PhotoId}",
            userId,
            request.PhotoId);

        try
        {
            logger.LogInformation(
                "Removing photo {PhotoId} for user {UserId}",
                request.PhotoId,
                userId);

            // ============================================================
            // 1. Загружаем пользователя с фото
            // ============================================================
            var user = await userRepository.GetByIdWithPhotosAsync(userId, cancellationToken);

            if (user is null)
                return Result.Failure(UserErrors.NotFound(userId.ToString()));

            var photo = user.Photos.FirstOrDefault(p => p.Id == request.PhotoId);
            if (photo is null)
                return Result.Failure(PhotoErrors.NotFound(request.PhotoId));

            var photoUrl = photo.Url;

            // ============================================================
            // 2. Удаляем фото из домена
            // ============================================================
            var result = user.RemovePhoto(request.PhotoId);
            if (result.IsFailure)
                return result;

            // ============================================================
            // 3. Сохраняем в БД
            // ============================================================
            var affectedRows = await unitOfWork.SaveChangesAsync(cancellationToken);
            if (affectedRows == 0)
                return Result.Failure(UserErrors.DatabaseSaveFailed("No rows affected"));

            // ============================================================
            // 4. Удаляем файл из хранилища (после успешного сохранения)
            // ============================================================
            _ = Task.Run(async () =>
            {
                try
                {
                    await fileUploadService.DeleteAsync(photoUrl);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex,
                        "Failed to delete file from storage: {PhotoUrl}",
                        photoUrl);
                }
            }, cancellationToken);

            logger.LogInformation(
                "Photo {PhotoId} removed successfully for user {UserId}",
                request.PhotoId,
                userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to remove photo {PhotoId} for user {UserId}",
                request.PhotoId,
                userId);

            return Result.Failure(PhotoErrors.DeleteFailed(ex.Message));
        }
    }
}
