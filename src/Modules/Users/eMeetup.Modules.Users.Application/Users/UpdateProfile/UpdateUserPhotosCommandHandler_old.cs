using System;
using System.Collections.Generic;
using System.Text;
using eMeetup.Common.Application.Messaging;
using eMeetup.Common.Domain;
using eMeetup.Modules.Users.Application.Abstractions.Data;
using eMeetup.Modules.Users.Domain.Interfaces.Repositories;
using eMeetup.Modules.Users.Domain.Interfaces.Services;
using eMeetup.Modules.Users.Domain.Users;
using Microsoft.Extensions.Logging;

namespace eMeetup.Modules.Users.Application.Users.UpdateUser;

public sealed class UpdateUserPhotosCommandHandler(
    IUserRepository userRepository,
    IFileStorageService fileStorageService,
    IUnitOfWork unitOfWork,
    ILogger<UpdateUserPhotosCommandHandler> logger)
    : ICommandHandler<UpdateUserPhotosCommand, UpdateUserPhotosResult>
{
    public async Task<Result<UpdateUserPhotosResult>> Handle(
        UpdateUserPhotosCommand request,
        CancellationToken cancellationToken)
    {
        using var loggerScope = logger.BeginScope(
            "UpdateUserPhotos UserId:{UserId}, Add:{AddCount}, Remove:{RemoveCount}",
            request.UserId,
            request.Photos?.Count ?? 0,
            request.RemovePhotoIds?.Count ?? 0);

        try
        {
            logger.LogInformation("Starting photo update for user {UserId}", request.UserId);

            // Get user with photos loaded
            var user = await userRepository.GetByIdWithPhotosAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                logger.LogWarning("User {UserId} not found for photo update", request.UserId);
                return Result.Failure<UpdateUserPhotosResult>(UserErrors.NotFound);
            }

            // Validate current photo count
            var currentPhotoCount = user.Photos.Count;
            var maxPhotos = 10; // Configurable max photos per user

            // Process removals first
            var removedCount = await ProcessPhotoRemovalsAsync(
                user, request.RemovePhotoIds, cancellationToken);

            // Process additions
            var addedCount = 0;
            if (request.Photos?.Count > 0)
            {
                var addResult = await ProcessPhotoAdditionsAsync(
                    user, request.Photos, maxPhotos, cancellationToken);

                if (addResult.IsFailure)
                    return Result.Failure<UpdateUserPhotosResult>(addResult.Error);

                addedCount = addResult.Value;
            }

            // Set primary photo if requested
            var primaryPhotoId = await ProcessPrimaryPhotoAsync(
                user, request.SetPrimaryPhotoId, cancellationToken);

            // Reorder photos if needed
            if (request.Photos?.Any(p => p.DisplayOrder.HasValue) == true)
            {
                await ReorderPhotosAsync(user, request.Photos, cancellationToken);
            }

            // Save changes
            await unitOfWork.SaveChangesAsync(cancellationToken);

            // Get updated photos
            var updatedPhotos = GetPhotoInfoList(user);

            var result = new UpdateUserPhotosResult(
                user.Id,
                addedCount,
                removedCount,
                primaryPhotoId,
                updatedPhotos);

            logger.LogInformation(
                "Successfully updated photos for user {UserId}: Added {Added}, Removed {Removed}",
                request.UserId, addedCount, removedCount);

            return Result.Success(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating photos for user {UserId}", request.UserId);
            return Result.Failure<UpdateUserPhotosResult>(UserErrors.PhotoUpdateFailed);
        }
    }

    private async Task<int> ProcessPhotoRemovalsAsync(
        User user,
        List<Guid>? photoIdsToRemove,
        CancellationToken cancellationToken)
    {
        if (photoIdsToRemove == null || !photoIdsToRemove.Any())
            return 0;

        var removedCount = 0;
        var photosToDeleteFromStorage = new List<string>();

        foreach (var photoId in photoIdsToRemove)
        {
            var photoResult = user.GetPhoto(photoId);
            if (photoResult.IsFailure)
            {
                logger.LogWarning("Photo {PhotoId} not found for removal from user {UserId}",
                    photoId, user.Id);
                continue;
            }

            var photo = photoResult.Value;
            photosToDeleteFromStorage.Add(photo.Url);

            var removeResult = user.RemovePhoto(photoId);
            if (removeResult.IsFailure)
            {
                logger.LogWarning("Failed to remove photo {PhotoId} from user {UserId}: {Error}",
                    photoId, user.Id, removeResult.Error);
                continue;
            }

            removedCount++;
            logger.LogDebug("Removed photo {PhotoId} from user {UserId}", photoId, user.Id);
        }

        // Cleanup removed photos from storage
        await CleanupPhotosFromStorageAsync(photosToDeleteFromStorage, cancellationToken);

        return removedCount;
    }

    private async Task<Result<int>> ProcessPhotoAdditionsAsync(
        User user,
        List<UpdatePhotoRequest> photosToAdd,
        int maxPhotos,
        CancellationToken cancellationToken)
    {
        var addedCount = 0;
        var uploadedPhotoUrls = new List<string>();

        try
        {
            // Check if adding these photos would exceed the limit
            if (user.Photos.Count + photosToAdd.Count > maxPhotos)
            {
                logger.LogWarning(
                    "User {UserId} would exceed max photos ({MaxPhotos}) with {NewCount} new photos",
                    user.Id, maxPhotos, photosToAdd.Count);
                return Result.Failure<int>(UserErrors.TooManyPhotos(maxPhotos));
            }

            // Determine starting display order
            var currentMaxOrder = user.Photos.Any()
                ? user.Photos.Max(p => p.DisplayOrder) + 1
                : 0;

            foreach (var photoRequest in photosToAdd)
            {
                if (photoRequest.File == null || photoRequest.File.Length == 0)
                {
                    logger.LogWarning("Skipping empty photo file for user {UserId}", user.Id);
                    continue;
                }

                // Upload photo to storage
                var uploadResult = await fileStorageService.UploadPhotoAsync(
                    photoRequest.File.OpenReadStream(),
                    photoRequest.File.FileName,
                    photoRequest.File.ContentType,
                    cancellationToken);

                if (uploadResult.IsFailure)
                {
                    logger.LogError("Failed to upload photo {FileName} for user {UserId}: {Error}",
                        photoRequest.File.FileName, user.Id, uploadResult.Error);
                    continue;
                }

                var photoUrl = uploadResult.Value;
                uploadedPhotoUrls.Add(photoUrl);

                // Determine display order
                var displayOrder = photoRequest.DisplayOrder ?? currentMaxOrder;

                // Add photo to user
                var addResult = user.AddPhoto(photoUrl, photoRequest.IsPrimary);
                if (addResult.IsFailure)
                {
                    logger.LogError("Failed to add photo to user {UserId}: {Error}",
                        user.Id, addResult.Error);

                    // Cleanup uploaded photo
                    await fileStorageService.DeletePhotoAsync(photoUrl, cancellationToken);
                    uploadedPhotoUrls.Remove(photoUrl);
                    continue;
                }

                // Update display order if specified
                if (photoRequest.DisplayOrder.HasValue)
                {
                    var photo = user.Photos.FirstOrDefault(p => p.Url == photoUrl);
                    if (photo != null)
                    {
                        var updateOrderResult = photo.UpdateDisplayOrder(displayOrder);
                        if (updateOrderResult.IsFailure)
                        {
                            logger.LogWarning("Failed to set display order for photo: {Error}",
                                updateOrderResult.Error);
                        }
                    }
                }

                addedCount++;
                currentMaxOrder++;

                logger.LogDebug("Added photo {FileName} to user {UserId}",
                    photoRequest.File.FileName, user.Id);
            }

            return Result.Success(addedCount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding photos to user {UserId}", user.Id);

            // Cleanup uploaded photos on failure
            await CleanupPhotosFromStorageAsync(uploadedPhotoUrls, cancellationToken);

            return Result.Failure<int>(UserErrors.PhotoUploadFailed);
        }
    }

    private async Task<Guid?> ProcessPrimaryPhotoAsync(
        User user,
        Guid? primaryPhotoId,
        CancellationToken cancellationToken)
    {
        if (!primaryPhotoId.HasValue)
            return user.GetPrimaryPhoto()?.Id;

        var result = user.SetPrimaryPhoto(primaryPhotoId.Value);
        if (result.IsFailure)
        {
            logger.LogWarning("Failed to set primary photo {PhotoId} for user {UserId}: {Error}",
                primaryPhotoId, user.Id, result.Error);
            return user.GetPrimaryPhoto()?.Id;
        }

        logger.LogDebug("Set primary photo to {PhotoId} for user {UserId}",
            primaryPhotoId, user.Id);

        return primaryPhotoId;
    }

    private async Task ReorderPhotosAsync(
        User user,
        List<UpdatePhotoRequest> photos,
        CancellationToken cancellationToken)
    {
        var photoOrders = new Dictionary<Guid, int>();

        foreach (var photoRequest in photos.Where(p => p.DisplayOrder.HasValue))
        {
            // Need to find the photo by matching filename or other identifier
            // This assumes photos have been added in this request
            // For existing photos, you'd need a different approach
        }

        if (photoOrders.Any())
        {
            var reorderResult = user.ReorderPhotos(photoOrders);
            if (reorderResult.IsFailure)
            {
                logger.LogWarning("Failed to reorder photos for user {UserId}: {Error}",
                    user.Id, reorderResult.Error);
            }
        }
    }

    private List<PhotoInfo> GetPhotoInfoList(User user)
    {
        return user.Photos
            .OrderBy(p => p.DisplayOrder)
            .Select(p => new PhotoInfo(
                p.Id,
                p.Url,
                p.IsPrimary,
                p.DisplayOrder))
            .ToList();
    }

    private async Task CleanupPhotosFromStorageAsync(
        List<string> photoUrls,
        CancellationToken cancellationToken)
    {
        if (!photoUrls.Any())
            return;

        logger.LogInformation("Cleaning up {Count} photos from storage", photoUrls.Count);

        foreach (var photoUrl in photoUrls)
        {
            try
            {
                await fileStorageService.DeletePhotoAsync(photoUrl, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to cleanup photo from storage: {Url}", photoUrl);
            }
        }
    }
}
