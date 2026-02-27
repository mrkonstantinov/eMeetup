using System.Data;
using System.Net;
using eMeetup.Common.Application.Messaging;
using eMeetup.Common.Domain;
using eMeetup.Modules.Users.Application.Abstractions.Data;
using eMeetup.Modules.Users.Application.Abstractions.Identity;
using eMeetup.Modules.Users.Domain.Interfaces.Repositories;
using eMeetup.Modules.Users.Domain.Interfaces.Services;
using eMeetup.Modules.Users.Domain.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace eMeetup.Modules.Users.Application.Users.UpdateUser;

internal sealed class UploadUserPhotosCommandHandler(
    IUserRepository userRepository,
    IUserPhotoUpdateService photoUpdateService,
    IUserPhotoRepository photoRepository,
    IFileStorageService fileStorageService,
    IIdentityProviderService identityProviderService,
    IUnitOfWork unitOfWork,
    ILogger<UploadUserPhotosCommandHandler> logger)
    : ICommandHandler<UploadUserPhotosCommand>
{
    public async Task<Result> Handle(UploadUserPhotosCommand request, CancellationToken cancellationToken)
    {
        using var loggerScope = logger.BeginScope("UserPhotosUpdate {IdentityId}", request.IdentityId);

        try
        {
            logger.LogInformation("Starting user photos update for {IdentityId}", request.IdentityId);

            // 1. Get user
            var user = await userRepository.GetByIdentityIdAsync(request.IdentityId, cancellationToken);
            if (user == null)
                return Result.Failure(UserErrors.NotFoundByIdentity(request.IdentityId));

            // 2. Validate photos before processing
            var validationResult = await photoUpdateService.ValidatePhotosAsync(
                request.Photos ?? new List<IFormFile>(),
                cancellationToken);

            if (!validationResult.IsValid)
            {
                logger.LogWarning("Photo validation failed for user {UserId}: {Errors}",
                    user.Id, string.Join(", ", validationResult.Errors));
                return Result.Failure(UserErrors.InvalidPhotos(string.Join("; ", validationResult.Errors)));
            }

            var photoUpdateResult = await photoUpdateService.UpdateUserPhotosAsync(
                user.Id, request.Photos,
                cancellationToken);

            if (!photoUpdateResult.Success)
            {
                logger.LogError("Failed to update photos for user {UserId}: {Errors}",
                    user.Id, string.Join("; ", photoUpdateResult.Errors));
                return Result.Failure(UserErrors.PhotoUpdateFailed(string.Join("; ", photoUpdateResult.Errors)));
            }

            // 3. Get current primary photo URL for potential rollback
            var currentPrimaryPhotoUrl = await photoRepository.GetPrimaryPhotoUrlAsync(user.Id, cancellationToken);
            user.UpdateProfilePictureUrl(currentPrimaryPhotoUrl);

            // 5. Save changes to database
            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Photos updated successfully for user {UserId}: " +
                                "Created={Created}, Updated={Updated}, Deleted={Deleted}",
                user.Id, photoUpdateResult.CreatedPhotos.Count,
                photoUpdateResult.UpdatedPhotos.Count, photoUpdateResult.DeletedPhotos.Count);

            // 6. Update Keycloak with new primary photo URL
            if (ShouldUpdateKeycloak(photoUpdateResult, currentPrimaryPhotoUrl))
            {
                var keycloakResult = await UpdateKeycloakWithRetryAsync(
                    identityId: request.IdentityId,
                    user: user,
                    profilePictureUrl: photoUpdateResult.NewPrimaryPhotoUrl,
                    originalProfilePictureUrl: currentPrimaryPhotoUrl,
                    cancellationToken: cancellationToken);

                if (keycloakResult.IsFailure)
                {
                    // Cleanup uploaded photos if Keycloak update fails
                    await CleanupUploadedPhotosAsync(photoUpdateResult, cancellationToken);

                    // Attempt to restore original state
                    await AttemptRollbackToOriginalStateAsync(
                        user.Id,
                        currentPrimaryPhotoUrl,
                        cancellationToken);

                    logger.LogError("Keycloak update failed after photo upload: {Error}", keycloakResult.Error);
                    return Result.Failure(keycloakResult.Error);
                }

                logger.LogInformation("Keycloak profile picture updated successfully for {IdentityId}",
                    request.IdentityId);
            }
            else
            {
                logger.LogDebug("No Keycloak update needed - primary photo unchanged");
            }

            logger.LogInformation("User photos update completed successfully for {IdentityId}", request.IdentityId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "User photos update failed for {IdentityId}", request.IdentityId);
            return Result.Failure(UserErrors.PhotoUpdateFailed(ex.Message));
        }
    }

    private bool ShouldUpdateKeycloak(PhotoUpdateResult photoUpdateResult, string? currentPrimaryPhotoUrl)
    {
        // Update Keycloak if:
        // 1. A new primary photo was set
        // 2. The primary photo URL changed
        return photoUpdateResult.NewPrimaryPhotoUrl != null &&
               photoUpdateResult.NewPrimaryPhotoUrl != currentPrimaryPhotoUrl;
    }

    private async Task<Result> UpdateKeycloakWithRetryAsync(
        Guid identityId,
        User user,
        string? profilePictureUrl,
        string? originalProfilePictureUrl,
        CancellationToken cancellationToken)
    {
        const int maxRetries = 3;
        int retryCount = 0;

        logger.LogDebug("Updating Keycloak for {IdentityId} with profile picture: {ProfilePictureUrl}",
            identityId, profilePictureUrl);

        while (retryCount < maxRetries)
        {
            try
            {
                var result = await identityProviderService.UpdateKeycloakUserAttributesAsync(
                    identityId: identityId,
                    bio: user.Bio,
                    latitude: user.Location?.Latitude,
                    longitude: user.Location?.Longitude,
                    city: user.Location?.City,
                    street: user.Location?.Street,
                    interests: user.Interests.ToString(),
                    profilePictureUrl: profilePictureUrl,
                    cancellationToken: cancellationToken);

                if (result.IsSuccess)
                {
                    logger.LogDebug("Keycloak update successful on attempt {Attempt}", retryCount + 1);
                    return Result.Success();
                }

                // Handle specific error types
                switch (result.Error.Type)
                {
                    case ErrorType.Conflict:
                        retryCount++;
                        logger.LogWarning("Keycloak conflict (attempt {Retry}/{MaxRetries}) for {IdentityId}",
                            retryCount, maxRetries, identityId);

                        if (retryCount == maxRetries)
                        {
                            logger.LogError("Max retries reached for Keycloak conflict");
                            return Result.Failure(UserErrors.KeycloakConflict);
                        }

                        // Exponential backoff
                        var delay = TimeSpan.FromMilliseconds(100 * Math.Pow(2, retryCount));
                        logger.LogDebug("Retrying after {Delay}ms", delay.TotalMilliseconds);
                        await Task.Delay(delay, cancellationToken);
                        continue;

                    case ErrorType.NotFound:
                        logger.LogError("User not found in Keycloak: {IdentityId}", identityId);
                        return Result.Failure(UserErrors.KeycloakUserNotFound);

                    case ErrorType.Unauthorized:
                    case ErrorType.Forbidden:
                        logger.LogError("Authorization failed for Keycloak update: {IdentityId}", identityId);
                        return Result.Failure(UserErrors.KeycloakAuthorizationFailed);

                    default:
                        logger.LogError("Keycloak update failed with error: {Error}", result.Error);
                        return Result.Failure(UserErrors.KeycloakUpdateFailed);
                }
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.RequestTimeout)
            {
                retryCount++;
                logger.LogWarning(ex,
                    "Keycloak request timeout (attempt {Retry}/{MaxRetries}) for {IdentityId}",
                    retryCount, maxRetries, identityId);

                if (retryCount == maxRetries)
                {
                    logger.LogError("Max retries reached for Keycloak timeout");
                    return Result.Failure(UserErrors.KeycloakTimeout);
                }

                var delay = TimeSpan.FromMilliseconds(100 * Math.Pow(2, retryCount));
                await Task.Delay(delay, cancellationToken);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                retryCount++;
                logger.LogWarning(ex,
                    "Keycloak service unavailable (attempt {Retry}/{MaxRetries}) for {IdentityId}",
                    retryCount, maxRetries, identityId);

                if (retryCount == maxRetries)
                {
                    logger.LogError("Max retries reached for Keycloak service unavailable");
                    return Result.Failure(UserErrors.KeycloakServiceUnavailable);
                }

                var delay = TimeSpan.FromMilliseconds(200 * Math.Pow(2, retryCount));
                await Task.Delay(delay, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error updating Keycloak for {IdentityId}", identityId);
                return Result.Failure(UserErrors.KeycloakUpdateFailed);
            }
        }

        return Result.Failure(UserErrors.KeycloakUpdateFailed);
    }

    private async Task CleanupUploadedPhotosAsync(
        PhotoUpdateResult photoUpdateResult,
        CancellationToken cancellationToken)
    {
        if (!photoUpdateResult.UploadedFileUrls.Any())
        {
            logger.LogDebug("No uploaded files to cleanup");
            return;
        }

        logger.LogWarning("Cleaning up {Count} uploaded files after Keycloak failure",
            photoUpdateResult.UploadedFileUrls.Count);

        var cleanupTasks = new List<Task>();
        var errors = new List<string>();

        foreach (var url in photoUpdateResult.UploadedFileUrls)
        {
            try
            {
                var deleteTask = fileStorageService.DeleteFileAsync(url, cancellationToken);
                cleanupTasks.Add(deleteTask);
                logger.LogDebug("Scheduled cleanup for file: {Url}", url);
            }
            catch (Exception ex)
            {
                errors.Add($"Failed to schedule cleanup for {url}: {ex.Message}");
                logger.LogError(ex, "Failed to schedule file cleanup: {Url}", url);
            }
        }

        try
        {
            await Task.WhenAll(cleanupTasks);
            logger.LogInformation("Successfully cleaned up {Count} uploaded files",
                photoUpdateResult.UploadedFileUrls.Count);
        }
        catch (Exception ex)
        {
            errors.Add($"Cleanup task failed: {ex.Message}");
            logger.LogError(ex, "Failed during file cleanup");
        }

        if (errors.Any())
        {
            logger.LogWarning("Partial cleanup failures: {Errors}", string.Join("; ", errors));
        }
    }

    private async Task AttemptRollbackToOriginalStateAsync(
        Guid userId,
        string? originalPrimaryPhotoUrl,
        CancellationToken cancellationToken)
    {
        logger.LogWarning("Attempting to rollback to original state for user {UserId}", userId);

        try
        {
            // 1. Get current primary photo
            var currentPrimaryPhoto = await photoRepository.GetPrimaryPhotoAsync(userId, cancellationToken);

            if (currentPrimaryPhoto != null && currentPrimaryPhoto.Url != originalPrimaryPhotoUrl)
            {
                // 2. If we have an original primary photo URL, try to find it
                if (!string.IsNullOrEmpty(originalPrimaryPhotoUrl))
                {
                    var allPhotos = await photoRepository.GetByUserIdAsync(userId, cancellationToken);
                    var originalPhoto = allPhotos.FirstOrDefault(p => p.Url == originalPrimaryPhotoUrl);

                    if (originalPhoto != null)
                    {
                        // 3. Restore original as primary
                        await photoRepository.UpdatePrimaryPhotoAsync(userId, originalPhoto.Id, cancellationToken);
                        logger.LogInformation("Restored original primary photo for user {UserId}", userId);
                    }
                    else
                    {
                        logger.LogWarning("Original primary photo not found for user {UserId}", userId);
                    }
                }
                else
                {
                    // 4. If no original primary photo, mark all as secondary
                    await photoRepository.MarkAllAsSecondaryAsync(userId, cancellationToken);
                    logger.LogInformation("Marked all photos as secondary for user {UserId}", userId);
                }
            }

            // 5. Save changes
            await unitOfWork.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Successfully rolled back photo state for user {UserId}", userId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to rollback photo state for user {UserId}", userId);
            // Log the situation for manual intervention
            logger.LogCritical("""
                Manual intervention may be required for user {UserId}.
                Photo state rollback failed after Keycloak update failure.
                Original primary photo URL: {OriginalUrl}
                """,
                userId,
                originalPrimaryPhotoUrl ?? "None");
        }
    }

    private async Task RollbackKeycloakChangesAsync(
        Guid identityId,
        string? originalProfilePictureUrl,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogWarning("Attempting to rollback Keycloak profile picture for {IdentityId}", identityId);

            if (string.IsNullOrEmpty(originalProfilePictureUrl))
            {
                logger.LogWarning("No original profile picture URL to rollback for {IdentityId}", identityId);
                return;
            }

            // Get user to update Keycloak with original profile picture
            var user = await userRepository.GetByIdentityIdAsync(identityId, cancellationToken);
            if (user == null)
            {
                logger.LogError("User not found for Keycloak rollback: {IdentityId}", identityId);
                return;
            }

            var rollbackResult = await identityProviderService.UpdateKeycloakUserAttributesAsync(
                identityId: identityId,
                bio: user.Bio,
                latitude: user.Location?.Latitude,
                longitude: user.Location?.Longitude,
                city: user.Location?.City,
                street: user.Location?.Street,
                interests: user.Interests.ToString(),
                profilePictureUrl: originalProfilePictureUrl,
                cancellationToken: cancellationToken);

            if (rollbackResult.IsSuccess)
            {
                logger.LogInformation("Successfully rolled back Keycloak profile picture for {IdentityId}",
                    identityId);
            }
            else
            {
                logger.LogError("Failed to rollback Keycloak for {IdentityId}: {Error}",
                    identityId, rollbackResult.Error);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception during Keycloak rollback for {IdentityId}", identityId);

            // Critical log for manual intervention
            logger.LogCritical("""
                Manual Keycloak intervention required for {IdentityId}.
                Failed to rollback profile picture changes.
                Original profile picture URL: {OriginalUrl}
                """,
                identityId,
                originalProfilePictureUrl ?? "None");
        }
    }
}
