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

namespace eMeetup.Modules.Users.Application.Users.UpdateUser;


internal sealed class UpdateUserCommandHandler2(
    IUserRepository userRepository,
    IIdentityProviderService identityProviderService,
    ISlugService slugService,
    IFileStorageService fileStorageService,
    IGeocodingService geocodingService,
    IUnitOfWork unitOfWork,
    ILogger<UpdateUserCommandHandler> logger)
    : ICommandHandler<UpdateUserCommand>
{
    public async Task<Result> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        using var loggerScope = logger.BeginScope("UserUpdate {IdentityId}", request.IdentityId);

        List<string> uploadedPhotoUrls = new();
        string? profilePictureUrl = null;
        UserUpdateSet? updates = null;

        try
        {
            logger.LogInformation("Starting atomic user update for {IdentityId}", request.IdentityId);

            // Step 1: Get existing user
            var user = await userRepository.GetByIdentityIdAsync(request.IdentityId, cancellationToken);
            if (user == null)
            {
                logger.LogWarning("User not found for identity ID: {IdentityId}", request.IdentityId);
                return Result.Failure(UserErrors.NotFoundByIdentity(request.IdentityId));
            }

            // Step 2: Initialize update tracking
            updates = new UserUpdateSet(user);

            // Step 4: Handle photo uploads (with cleanup on failure)
            if (request.Photos?.Count > 0)
            {
                var photoResult = await HandlePhotoUploadsWithCleanupAsync(
                    user, request.Photos, uploadedPhotoUrls, cancellationToken);

                if (photoResult.IsFailure)
                {
                    await CleanupUploadedPhotosAsync(uploadedPhotoUrls, cancellationToken);
                    return Result.Failure(photoResult.Error);
                }

                profilePictureUrl = photoResult.Value.ProfilePictureUrl;
                updates.ProfilePictureUrl = profilePictureUrl;
                logger.LogInformation("Profile picture URL set to: {Url}", profilePictureUrl);
            }

            // Step 5: Update bio if provided
            if (request.Bio != null && request.Bio != user.Bio)
            {
                user.UpdateBio(request.Bio);
                updates.Bio = request.Bio;
                logger.LogInformation("Bio updated");
            }

            // Step 6: Update location if coordinates provided
            if (request.Latitude.HasValue && request.Longitude.HasValue)
            {
                var locationResult = await CreateLocationAsync(
                    request.Latitude.Value,
                    request.Longitude.Value,
                    request.City,
                    request.Country,
                    cancellationToken);

                if (locationResult.IsFailure)
                {
                    await CleanupUploadedPhotosAsync(uploadedPhotoUrls, cancellationToken);
                    return Result.Failure(locationResult.Error);
                }

                user.UpdateLocation(locationResult.Value);
                updates.Latitude = request.Latitude.Value;
                updates.Longitude = request.Longitude.Value;
                updates.City = request.City;
                updates.Country = request.Country;
                logger.LogInformation("Location updated: {Latitude}, {Longitude}, {City}, {Country}",
                    request.Latitude, request.Longitude, request.City, request.Country);
            }

            // Step 7: Update interests if provided
            //if (request.Interests != null)
            //{
            //    var slugs = await slugService.GetExistingSlugsAsync(request.Interests, cancellationToken);
            //    user.UpdateInterests(slugs);
            //    updates.Interests = request.Interests;
            //    logger.LogInformation("Interests updated: {Interests}", request.Interests);
            //}

            // Step 8: Save to database


            //userRepository.Update(user);
            var dbResult = await SaveToDatabaseAsync(cancellationToken);

            if (dbResult.IsFailure)
            {
                await CleanupUploadedPhotosAsync(uploadedPhotoUrls, cancellationToken);
                return Result.Failure(dbResult.Error);
            }

            logger.LogInformation("Database save successful, affected rows: {Rows}", dbResult.Value);

            // Step 9: Update Keycloak if there are updates
            if (updates.HasUpdates)
            {

                var keycloakResult = await UpdateKeycloakWithRetryAsync(
                    request.IdentityId, updates, profilePictureUrl, cancellationToken);

                if (keycloakResult.IsFailure)
                {
                    // Cleanup uploaded photos
                    await CleanupUploadedPhotosAsync(uploadedPhotoUrls, cancellationToken);

                    // Attempt to rollback Keycloak changes
                    await RollbackKeycloakChangesAsync(request.IdentityId, updates, cancellationToken);

                    logger.LogError("Keycloak update failed: {Error}", keycloakResult.Error);
                    return Result.Failure(keycloakResult.Error);
                }

                logger.LogInformation("Keycloak update successful");
            }
            else
            {
                logger.LogInformation("No Keycloak updates required");
            }

            // Step 10: Commit transaction

            logger.LogInformation("""
                Atomic update completed successfully for {IdentityId}:
                - Updated fields: {Fields}
                - Database rows affected: {DbRows}
                - Keycloak updated: {KeycloakUpdated}
                - Photos uploaded: {PhotoCount}
                """,
                request.IdentityId,
                updates.UpdatedFieldsString,
                "dbResult.Value",
                updates.HasUpdates,
                uploadedPhotoUrls.Count);

            return Result.Success();
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Update operation cancelled for {IdentityId}", request.IdentityId);
            await CleanupUploadedPhotosAsync(uploadedPhotoUrls, cancellationToken);


            return Result.Failure(UserErrors.OperationCancelled);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, """
                Atomic update failed for {IdentityId}
                - Uploaded photos: {PhotoCount}
                - Updates attempted: {Updates}
                """,
                request.IdentityId,
                uploadedPhotoUrls.Count,
                updates?.UpdatedFieldsString ?? "none");

            // Cleanup uploaded photos if transaction failed
            await CleanupUploadedPhotosAsync(uploadedPhotoUrls, cancellationToken);


            return Result.Failure(UserErrors.AtomicUpdateFailed);
        }
        finally
        {
            //transaction?.Dispose();
        }
    }

    // Helper class to track updates
    private sealed class UserUpdateSet
    {
        public string? Bio { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? Interests { get; set; }
        public string? ProfilePictureUrl { get; set; }

        public bool HasUpdates =>
            Bio != null ||
            Latitude.HasValue ||
            Longitude.HasValue ||
            City != null ||
            Country != null ||
            Interests != null ||
            ProfilePictureUrl != null;

        public string UpdatedFieldsString
        {
            get
            {
                var fields = new List<string>();
                if (Bio != null) fields.Add("Bio");
                if (Latitude.HasValue) fields.Add("Latitude");
                if (Longitude.HasValue) fields.Add("Longitude");
                if (City != null) fields.Add("City");
                if (Country != null) fields.Add("Country");
                if (Interests != null) fields.Add("Interests");
                if (ProfilePictureUrl != null) fields.Add("ProfilePictureUrl");
                return fields.Count > 0 ? string.Join(", ", fields) : "None";
            }
        }

        public UserUpdateSet(User user)
        {
            // Store original values for potential rollback
        }
    }

    private async Task<Result<PhotoUploadResult>> HandlePhotoUploadsWithCleanupAsync(
        User user,
        List<IFormFile> photos,
        List<string> uploadedPhotoUrls,
        CancellationToken cancellationToken)
    {
        var result = new PhotoUploadResult
        {
            OperationId = Guid.NewGuid().ToString(),
            StartedAt = DateTime.UtcNow
        };

        var temporaryUploads = new List<(string Url, string FileName, long Size)>();
        var hasPartialSuccess = false;

        try
        {
            logger.LogInformation("Starting photo upload with cleanup for user {UserId}, {PhotoCount} photos",
                user.Id, photos?.Count ?? 0);

            if (photos == null || photos.Count == 0)
            {
                logger.LogWarning("No photos provided for upload");
                return Result.Failure<PhotoUploadResult>(UserErrors.NoPhotosProvided);
            }

            // Validate photos before starting uploads
            var validationResult = await ValidatePhotosAsync(photos, cancellationToken);
            if (validationResult.IsFailure)
            {
                logger.LogError("Photo validation failed: {Error}", validationResult.Error);
                return Result.Failure<PhotoUploadResult>(validationResult.Error);
            }

            // Process each photo
            for (int i = 0; i < photos.Count; i++)
            {
                var photo = photos[i];
                var isPrimary = i == 0;

                try
                {
                    logger.LogInformation("Processing photo {Index}/{Total}: {FileName} (Size: {Size} bytes)",
                        i + 1, photos.Count, photo.FileName, photo.Length);

                    // Validate individual photo
                    var photoValidation = ValidatePhoto(photo);
                    if (photoValidation.IsFailure)
                    {
                        result.AddFailure(photo.FileName, photoValidation.Error.Description, "PHOTO_VALIDATION_FAILED");
                        logger.LogWarning("Photo validation failed for {FileName}: {Error}",
                            photo.FileName, photoValidation.Error);
                        continue;
                    }

                    // Upload photo
                    var uploadResult = await UploadPhotoWithRetryAsync(
                        photo,
                        user.Id.ToString(),
                        cancellationToken);

                    if (uploadResult.IsFailure)
                    {
                        result.AddFailure(photo.FileName, uploadResult.Error.Description, "UPLOAD_FAILED");
                        logger.LogError("Photo upload failed for {FileName}: {Error}",
                            photo.FileName, uploadResult.Error);
                        continue;
                    }

                    var uploadedUrl = uploadResult.Value.Url;
                    var fileSize = photo.Length;

                    // Track temporary upload for cleanup
                    temporaryUploads.Add((uploadedUrl, photo.FileName, fileSize));
                    uploadedPhotoUrls.Add(uploadedUrl);

                    // Add to result
                    result.AddSuccess(uploadedUrl, photo.FileName, fileSize, isPrimary);
                    hasPartialSuccess = true;

                    // Add to user domain
                    var addToUserResult = user.AddPhoto(uploadedUrl, isPrimary);
                    if (addToUserResult.IsFailure)
                    {
                        // Remove from temporary uploads since it won't be persisted
                        temporaryUploads.RemoveAt(temporaryUploads.Count - 1);
                        uploadedPhotoUrls.Remove(uploadedUrl);

                        // Delete the uploaded photo since we can't add it to user
                        await SafeDeletePhotoAsync(uploadedUrl, cancellationToken);

                        result.AddFailure(photo.FileName, addToUserResult.Error.Description, "DOMAIN_VALIDATION_FAILED");
                        logger.LogError("Failed to add photo to user domain: {Error}", addToUserResult.Error);

                        // Continue with other photos
                        continue;
                    }

                    logger.LogInformation("Successfully uploaded and added photo: {FileName} -> {Url}",
                        photo.FileName, uploadedUrl);

                }
                catch (OperationCanceledException)
                {
                    logger.LogWarning("Photo upload operation was cancelled while processing {FileName}",
                        photo.FileName);
                    result.AddFailure(photo.FileName, "Operation cancelled", "OPERATION_CANCELLED");
                    throw;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Unexpected error processing photo {FileName}", photo.FileName);
                    result.AddFailure(photo.FileName, ex.Message, "UNEXPECTED_ERROR");

                    // Continue with next photo unless it's a critical error
                    if (IsCriticalError(ex))
                    {
                        logger.LogCritical(ex, "Critical error occurred, aborting photo uploads");
                        break;
                    }
                }
            }

            // Final validation
            if (result.UploadedCount == 0 && result.FailedCount > 0)
            {
                logger.LogError("All photo uploads failed. Failures: {Failures}",
                    string.Join("; ", result.Failures.Select(f => $"{f.FileName}: {f.ErrorMessage}")));

                // Cleanup any temporary uploads
                await CleanupTemporaryUploadsAsync(temporaryUploads, cancellationToken);
                uploadedPhotoUrls.Clear();

                return Result.Failure<PhotoUploadResult>(UserErrors.AllPhotosUploadFailed);
            }

            if (result.HasFailures && result.UploadedCount > 0)
            {
                logger.LogWarning("Partial photo upload success. Uploaded: {SuccessCount}, Failed: {FailedCount}",
                    result.UploadedCount, result.FailedCount);
            }

            result.MarkCompleted();

            logger.LogInformation("""
                Photo upload completed for user {UserId}:
                - Operation: {OperationId}
                - Success: {SuccessCount}
                - Failures: {FailureCount}
                - Has Profile Picture: {HasProfilePicture}
                - Total Size: {TotalSize}
                """,
                user.Id,
                result.OperationId,
                result.UploadedCount,
                result.FailedCount,
                result.HasProfilePicture(),
                FormatBytes(result.TotalSizeBytes));

            return Result.Success(result);

        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Photo upload operation was cancelled entirely");

            // Cleanup any uploaded photos
            await CleanupTemporaryUploadsAsync(temporaryUploads, cancellationToken);
            uploadedPhotoUrls.Clear();

            result.AddFailure("Operation", "Upload operation was cancelled", "OPERATION_CANCELLED");
            result.MarkCompleted();

            return Result.Failure<PhotoUploadResult>(UserErrors.OperationCancelled);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Critical error during photo upload process");

            // Cleanup any uploaded photos
            await CleanupTemporaryUploadsAsync(temporaryUploads, cancellationToken);
            uploadedPhotoUrls.Clear();

            result.AddFailure("System", ex.Message, "CRITICAL_ERROR");
            result.MarkCompleted();

            return Result.Failure<PhotoUploadResult>(UserErrors.PhotoUploadFailed);
        }
    }

    private async Task<Result> ValidatePhotosAsync(List<IFormFile> photos, CancellationToken cancellationToken)
    {
        if (photos.Count > 10)
        {
            return Result.Failure(UserErrors.TooManyPhotos(photos.Count));
        }

        long totalSize = 0;
        var maxTotalSize = 50 * 1024 * 1024;

        foreach (var photo in photos)
        {
            totalSize += photo.Length;
            if (totalSize > maxTotalSize)
            {
                return Result.Failure(UserErrors.TotalPhotoSizeExceeded);
            }
        }

        return Result.Success();
    }

    private Result ValidatePhoto(IFormFile photo)
    {
        if (photo.Length > 10 * 1024 * 1024)
        {
            return Result.Failure(UserErrors.PhotoTooLarge(10 * 1024 * 1024));
        }

        if (photo.Length == 0)
        {
            return Result.Failure(UserErrors.PhotoEmpty);
        }

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var fileExtension = Path.GetExtension(photo.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(fileExtension))
        {
            return Result.Failure(UserErrors.InvalidPhotoFormat);
        }

        var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };

        if (!allowedMimeTypes.Contains(photo.ContentType.ToLowerInvariant()))
        {
            return Result.Failure(UserErrors.InvalidPhotoMimeType);
        }

        return Result.Success();
    }

    private async Task<Result<(string Url, long Size)>> UploadPhotoWithRetryAsync(
        IFormFile photo,
        string userId,
        CancellationToken cancellationToken,
        int maxRetries = 3)
    {
        int retryCount = 0;
        Exception? lastException = null;

        while (retryCount < maxRetries)
        {
            try
            {
                await using var stream = photo.OpenReadStream();

                var uploadResult = await fileStorageService.UploadPhotoAsync(
                    stream,
                    GenerateUniqueFileName(photo.FileName, userId),
                    photo.ContentType,
                    cancellationToken);

                if (uploadResult.IsFailure)
                {
                    lastException = new InvalidOperationException(uploadResult.Error.Description);
                    retryCount++;

                    if (retryCount < maxRetries)
                    {
                        logger.LogWarning("Upload failed, retrying ({Retry}/{MaxRetries}) for {FileName}",
                            retryCount, maxRetries, photo.FileName);
                        await Task.Delay(100 * retryCount, cancellationToken);
                    }
                    continue;
                }

                return Result.Success((uploadResult.Value, photo.Length));
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                lastException = ex;
                retryCount++;

                if (retryCount < maxRetries)
                {
                    logger.LogWarning(ex, "Upload exception, retrying ({Retry}/{MaxRetries}) for {FileName}",
                        retryCount, maxRetries, photo.FileName);
                    await Task.Delay(100 * retryCount, cancellationToken);
                }
            }
        }

        logger.LogError(lastException, "Failed to upload {FileName} after {MaxRetries} retries",
            photo.FileName, maxRetries);

        return Result.Failure<(string Url, long Size)>(UserErrors.PhotoUploadRetryFailed);
    }

    private async Task CleanupTemporaryUploadsAsync(
        List<(string Url, string FileName, long Size)> temporaryUploads,
        CancellationToken cancellationToken)
    {
        if (temporaryUploads.Count == 0) return;

        logger.LogInformation("Cleaning up {Count} temporary photo uploads", temporaryUploads.Count);

        var deleteTasks = new List<Task>();
        var failedDeletions = new List<string>();

        foreach (var (url, fileName, _) in temporaryUploads)
        {
            var deleteTask = SafeDeletePhotoAsync(url, cancellationToken)
                .ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        logger.LogError(t.Exception, "Failed to delete temporary photo {FileName} ({Url})",
                            fileName, url);
                        failedDeletions.Add(fileName);
                    }
                }, cancellationToken);

            deleteTasks.Add(deleteTask);
        }

        try
        {
            await Task.WhenAll(deleteTasks);

            if (failedDeletions.Count > 0)
            {
                logger.LogWarning("Failed to delete {Count} temporary photos: {Files}",
                    failedDeletions.Count, string.Join(", ", failedDeletions));
            }
            else
            {
                logger.LogInformation("Successfully cleaned up all temporary photo uploads");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during temporary upload cleanup");
        }
    }

    private async Task SafeDeletePhotoAsync(string url, CancellationToken cancellationToken)
    {
        try
        {
            await fileStorageService.DeletePhotoAsync(url, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete photo at URL: {Url}", url);
        }
    }

    private bool IsCriticalError(Exception ex)
    {
        return ex is OutOfMemoryException ||
               ex is StackOverflowException ||
               ex is InvalidOperationException ||
               (ex is HttpRequestException httpEx &&
                httpEx.StatusCode.HasValue &&
                (int)httpEx.StatusCode.Value >= 500);
    }

    private string GenerateUniqueFileName(string originalFileName, string userId)
    {
        var extension = Path.GetExtension(originalFileName);
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
        var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);

        return $"{userId}_{fileNameWithoutExtension}_{timestamp}_{uniqueId}{extension}";
    }

    private async Task<Result<int>> SaveToDatabaseAsync(CancellationToken cancellationToken)
    {
        try
        {
            var affectedRows = await unitOfWork.SaveChangesAsync(cancellationToken);
            if (affectedRows == 0)
            {
                logger.LogError("No rows were affected during database save");
                return Result.Failure<int>(UserErrors.DatabaseSaveFailed);
            }

            return Result.Success(affectedRows);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database save operation failed");
            return Result.Failure<int>(UserErrors.DatabaseSaveFailed);
        }
    }

    private async Task<Result> UpdateKeycloakWithRetryAsync(
        Guid identityId,
        UserUpdateSet updates,
        string? profilePictureUrl,
        CancellationToken cancellationToken)
    {
        const int maxRetries = 3;
        int retryCount = 0;

        while (retryCount < maxRetries)
        {
            try
            {
                var result = await identityProviderService.UpdateKeycloakUserAttributesAsync(
                    identityId: identityId,
                    bio: updates.Bio,
                    latitude: updates.Latitude,
                    longitude: updates.Longitude,
                    city: updates.City,
                    country: updates.Country,
                    interests: updates.Interests,
                    profilePictureUrl: profilePictureUrl ?? updates.ProfilePictureUrl,
                    cancellationToken);

                if (result.IsSuccess)
                {
                    return Result.Success();
                }

                if (result.Error.Type == ErrorType.Conflict)
                {
                    retryCount++;
                    logger.LogWarning("Keycloak conflict (attempt {Retry}/{MaxRetries})",
                        retryCount, maxRetries);

                    if (retryCount == maxRetries)
                    {
                        return Result.Failure(UserErrors.KeycloakConflict);
                    }

                    await Task.Delay(TimeSpan.FromMilliseconds(100 * Math.Pow(2, retryCount)), cancellationToken);
                    continue;
                }

                return result;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.RequestTimeout)
            {
                retryCount++;
                logger.LogWarning(ex,
                    "Keycloak request timeout (attempt {Retry}/{MaxRetries})",
                    retryCount, maxRetries);

                if (retryCount == maxRetries)
                {
                    return Result.Failure(UserErrors.KeycloakTimeout);
                }

                await Task.Delay(TimeSpan.FromMilliseconds(100 * Math.Pow(2, retryCount)), cancellationToken);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                logger.LogError(ex, "User not found in Keycloak: {IdentityId}", identityId);
                return Result.Failure(UserErrors.KeycloakUserNotFound);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error updating Keycloak");
                return Result.Failure(UserErrors.KeycloakUpdateFailed);
            }
        }

        return Result.Failure(UserErrors.KeycloakUpdateFailed);
    }

    private async Task RollbackKeycloakChangesAsync(
        Guid identityId,
        UserUpdateSet attemptedUpdates,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogWarning("Attempting to rollback Keycloak changes for {IdentityId}", identityId);

            logger.LogWarning("""
                Keycloak rollback needed for {IdentityId}. 
                Attempted updates: {Updates}
                Manual intervention may be required.
                """,
                identityId,
                attemptedUpdates.UpdatedFieldsString);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to rollback Keycloak changes for {IdentityId}", identityId);
        }
    }

    private async Task CleanupUploadedPhotosAsync(
        List<string> uploadedUrls,
        CancellationToken cancellationToken)
    {
        if (uploadedUrls.Count == 0) return;

        logger.LogWarning("Cleaning up {Count} uploaded photos", uploadedUrls.Count);

        try
        {
            var tasks = uploadedUrls.Select(url =>
                fileStorageService.DeletePhotoAsync(url, cancellationToken));

            await Task.WhenAll(tasks);
            logger.LogInformation("Successfully cleaned up uploaded photos");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to cleanup some uploaded photos");
        }
    }

    private async Task<Result<Location>> CreateLocationAsync(
        double latitude,
        double longitude,
        string? city,
        string? country,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(city) && !string.IsNullOrWhiteSpace(country))
            {
                return Location.Create(latitude, longitude, city, country);
            }

            logger.LogInformation("Reverse geocoding coordinates: {Latitude}, {Longitude}", latitude, longitude);
            var geocodingResult = await geocodingService.ReverseGeocodeAsync(latitude, longitude, cancellationToken);
            if (geocodingResult.IsSuccess)
            {
                logger.LogInformation("Geocoding successful: {City}, {Country}",
                    geocodingResult.Value.City, geocodingResult.Value.Country);
                return geocodingResult;
            }

            logger.LogWarning("Geocoding failed for coordinates {Latitude}, {Longitude}, using default values",
                latitude, longitude);
            return Location.Create(latitude, longitude, "Unknown", "Unknown");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during location creation for coordinates {Latitude}, {Longitude}",
                latitude, longitude);
            return Result.Failure<Location>(LocationErrors.GeocodingFailed);
        }
    }

    private static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        int order = 0;
        double len = bytes;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    private sealed class PhotoUploadResult
    {
        public string? ProfilePictureUrl { get; set; }
        public List<string> AllPhotoUrls { get; set; } = new List<string>();
        public List<string> UploadedFileNames { get; set; } = new List<string>();
        public int UploadedCount { get; set; }
        public int FailedCount { get; set; }
        public long TotalSizeBytes { get; set; }
        public string? OperationId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public List<PhotoUploadFailure> Failures { get; set; } = new List<PhotoUploadFailure>();

        public bool HasPhotos => UploadedCount > 0;
        public bool HasFailures => FailedCount > 0;
        public string? PrimaryPhotoUrl => ProfilePictureUrl;

        public void AddSuccess(string url, string fileName, long sizeBytes, bool isPrimary = false)
        {
            AllPhotoUrls.Add(url);
            UploadedFileNames.Add(fileName);
            UploadedCount++;
            TotalSizeBytes += sizeBytes;

            if (isPrimary)
            {
                ProfilePictureUrl = url;
            }
        }

        public void AddFailure(string fileName, string errorMessage, string? errorCode = null)
        {
            FailedCount++;
            Failures.Add(new PhotoUploadFailure
            {
                FileName = fileName,
                ErrorMessage = errorMessage,
                ErrorCode = errorCode,
                AttemptedAt = DateTime.UtcNow
            });
        }

        public void MarkCompleted()
        {
            CompletedAt = DateTime.UtcNow;
        }

        public bool HasProfilePicture()
        {
            return !string.IsNullOrEmpty(ProfilePictureUrl);
        }
    }

    private sealed class PhotoUploadFailure
    {
        public string FileName { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string? ErrorCode { get; set; }
        public DateTime AttemptedAt { get; set; }
    }
}



//internal sealed class UpdateUserCommandHandler(
//    IIdentityProviderService identityProviderService,
//    IDbConnectionFactory dbConnectionFactory,
//    IUserRepository userRepository,
//    IUnitOfWork unitOfWork)
//    : ICommandHandler<UpdateUserCommand>
//{
//    public async Task<Result> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
//    {
//        Result result = await identityProviderService.UpdateUserAsync(new UserProfileModel(
//            request.IdentityId,
//            request.Email,
//            request.DateOfBirth,
//            request.Gender,
//            request.Bio,
//            request.Latitude,
//            request.Longitude,
//            request.Country,
//            request.City,
//            request.Interests,
//            request.Photos), cancellationToken);

//        if (result.IsFailure)
//        {
//            return Result.Failure<Guid>(result.Error);
//        }


//        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

//        const string sql =
//            $"""
//             UPDATE  users.users
//             SET gender = @Gender,
//                date_of_birth = @DateOfBirth,
//                profile_photo_url = @ProfilePhotoUrl,
//                bio = @Bio,
//                interests = @Interests,
//                location = ST_GeomFromText(@Location, 4326)
//             WHERE identity_id = @IdentityId
//             """;

//        //user_name = @UserName,
//        //     gender = @Gender,
//        //     date_of_birth = @DateOfBirth,
//        //     bio = @Bio,
//        //     interests = @Interests, 
//        //     location = ST_GeomFromText(@Location, 4326)


//        // Execute the update command
//        // Assuming 'connection' is an open IDbConnection
//        var interceptor = new LoggingCommandInterceptor();

//        try
//        {
//            var affectedRows = await ExecuteWithInterceptorAsync(connection, sql, request, interceptor);
//        }
//        catch (Exception e)
//        {
//            var message = e.Message;
//        }

//        //var affectedRows = await connection.ExecuteAsync(sql, request);

//        //User? user = await userRepository.GetAsync(request.UserId, cancellationToken);

//        //if (user is null)
//        //{
//        //    return Result.Failure(UserErrors.NotFound(request.UserId));
//        //}

//        //user.Update(request.Gender, request.DateOfBirth, request.Bio, request.Interests, request.Location);

//        //await unitOfWork.SaveChangesAsync(cancellationToken);

//        return Result.Success();
//    }

//    public async Task<int> ExecuteWithInterceptorAsync(IDbConnection connection, string sql, object parameters, ICommandInterceptor interceptor)
//    {
//        interceptor?.BeforeExecute(sql, parameters);
//        int affectedRows = 0;
//        try
//        {
//            affectedRows = await connection.ExecuteAsync(sql, parameters);
//        }
//        catch (Exception e)
//        {
//            // Optional: handle exceptions or rethrow
//            throw;
//        }
//        finally
//        {
//            interceptor?.AfterExecute(sql, parameters, affectedRows);
//        }
//        return affectedRows;
//    }
//}


public interface ICommandInterceptor
{
    void BeforeExecute(string commandText, object parameters);
    void AfterExecute(string commandText, object parameters, int affectedRows);
}

public class LoggingCommandInterceptor : ICommandInterceptor
{
    public void BeforeExecute(string commandText, object parameters)
    {
        Console.WriteLine("Executing SQL:");
        Console.WriteLine(commandText);
        Console.WriteLine("With parameters:");
        foreach (var prop in parameters.GetType().GetProperties())
        {
            Console.WriteLine($"{prop.Name} = {prop.GetValue(parameters)}");
        }
    }

    public void AfterExecute(string commandText, object parameters, int affectedRows)
    {
        Console.WriteLine($"Executed. Rows affected: {affectedRows}");
    }
}
