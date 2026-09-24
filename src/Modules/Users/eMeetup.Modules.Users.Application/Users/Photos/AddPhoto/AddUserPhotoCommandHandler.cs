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

namespace eMeetup.Modules.Users.Application.Users.Photos.AddPhoto;

internal sealed class AddUserPhotoCommandHandler(
    IUserRepository userRepository,
    IFileUploadService fileUploadService,
    IUserContext userContext,
    IUnitOfWork unitOfWork,
    ILogger<AddUserPhotoCommandHandler> logger)
    : ICommandHandler<AddUserPhotoCommand, Guid>
{
    private const int MaxPhotosPerUser = 10;

    public async Task<Result<Guid>> Handle(
        AddUserPhotoCommand request,
        CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;

        using var loggerScope = logger.BeginScope(
            "AddUserPhoto {UserId} {FileName} {IsPrimary}",
            userId,
            request.FileName,
            request.IsPrimary);

        try
        {
            logger.LogInformation("Starting photo upload for user {UserId}", userId);

            // ============================================================
            // 1. Загружаем пользователя
            // ============================================================
            var user = await userRepository.GetByIdWithPhotosAsync(userId, cancellationToken);

            if (user is null)
            {
                logger.LogWarning("User {UserId} not found", userId);
                return Result.Failure<Guid>(UserErrors.NotFound(userId.ToString()));
            }

            // ============================================================
            // 2. Проверяем лимит
            // ============================================================
            if (user.Photos.Count >= MaxPhotosPerUser)
            {
                logger.LogWarning("Photo limit reached for user {UserId}", userId);
                return Result.Failure<Guid>(PhotoErrors.TooManyPhotos(MaxPhotosPerUser));
            }

            // ============================================================
            // 3. Загружаем файл в хранилище
            // ============================================================
            logger.LogInformation("Uploading file {FileName}", request.FileName);

            var uploadResult = await fileUploadService.UploadAsync(
                request.FileStream,
                request.FileName,
                request.ContentType,
                cancellationToken);

            if (uploadResult.IsFailure)
            {
                logger.LogError("File upload failed: {Error}", uploadResult.Error);
                return Result.Failure<Guid>(uploadResult.Error);
            }

            var uploadedFile = uploadResult.Value;

            // ============================================================
            // 4. Запоминаем старый primary
            // ============================================================
            var oldPrimaryUrl = request.IsPrimary
                ? user.ProfileImageUrl
                : null;

            // ============================================================
            // 5. Добавляем фото со всеми полями
            // ============================================================
            logger.LogInformation(
                "Adding photo to user {UserId} (IsPrimary: {IsPrimary}, Thumbnail: {Thumbnail})",
                userId,
                request.IsPrimary,
                uploadedFile.ThumbnailUrl);

            var addResult = user.AddPhoto(
                url: uploadedFile.Url,
                isPrimary: request.IsPrimary,
                fileName: uploadedFile.FileName,
                fileSize: uploadedFile.FileSize,
                contentType: uploadedFile.ContentType,
                thumbnailUrl: uploadedFile.ThumbnailUrl);   // ← ВАЖНО

            if (addResult.IsFailure)
            {
                logger.LogError("Failed to add photo: {Error}", addResult.Error);

                await fileUploadService.DeleteAsync(uploadedFile.Url, cancellationToken);
                return Result.Failure<Guid>(addResult.Error);
            }

            // ============================================================
            // 6. Сохраняем в БД
            // ============================================================
            var affectedRows = await unitOfWork.SaveChangesAsync(cancellationToken);

            if (affectedRows == 0)
            {
                await fileUploadService.DeleteAsync(uploadedFile.Url, cancellationToken);
                return Result.Failure<Guid>(
                    UserErrors.DatabaseSaveFailed("No rows affected"));
            }

            // ============================================================
            // 7. Удаляем старый primary в фоне
            // ============================================================
            if (!string.IsNullOrEmpty(oldPrimaryUrl) && oldPrimaryUrl != uploadedFile.Url)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await fileUploadService.DeleteAsync(oldPrimaryUrl);
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Failed to delete old primary {Url}", oldPrimaryUrl);
                    }
                }, cancellationToken);
            }

            // ============================================================
            // 8. Возвращаем ID
            // ============================================================
            var newPhoto = user.Photos.FirstOrDefault(p => p.Url == uploadedFile.Url);

            logger.LogInformation(
                "Photo added successfully for user {UserId}: PhotoId={PhotoId}",
                userId,
                newPhoto?.Id);

            return Result.Success(newPhoto?.Id ?? Guid.Empty);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during photo upload for user {UserId}", userId);
            return Result.Failure<Guid>(PhotoErrors.UploadFailed(ex.Message));
        }
    }
}
