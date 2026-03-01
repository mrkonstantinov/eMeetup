using eMeetup.Modules.Users.Domain.Interfaces.Repositories;
using eMeetup.Modules.Users.Domain.Interfaces.Services;
using eMeetup.Modules.Users.Domain.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace eMeetup.Modules.Users.Infrastructure.Services;


public class UserPhotoUpdateService : IUserPhotoUpdateService
{
    private readonly IUserPhotoRepository _photoRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<UserPhotoUpdateService> _logger;

    public UserPhotoUpdateService(
        IUserPhotoRepository photoRepository,
        IFileStorageService fileStorageService,
        ILogger<UserPhotoUpdateService> logger)
    {
        _photoRepository = photoRepository;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<PhotoUpdateResult> UpdateUserPhotosAsync(
        Guid userId, List<IFormFile>? photos,
        CancellationToken cancellationToken = default)
    {
        var result = new PhotoUpdateResult
        { 
            UserId = userId
        };

        try
        {
            _logger.LogInformation("Starting photo update for user {UserId}", userId);

            // Get existing photos from database
            var existingPhotos = await _photoRepository.GetByUserIdWithTrackingAsync(userId, cancellationToken);

            _logger.LogDebug("Found {Count} existing photos for user {UserId}",
                existingPhotos.Count, userId);

            // Handle the case when no new photos are provided
            if (photos == null || photos.Count == 0)
            {
                _logger.LogInformation("No new photos provided. Deleting all existing photos for user {UserId}", userId);
                await DeleteAllPhotosAsync(existingPhotos, result, cancellationToken);
                result.Success = true;
                return result;
            }

            // Process the photos
            _logger.LogInformation("Processing {Count} new photos for user {UserId}", photos.Count, userId);
            await ProcessPhotoUpdatesAsync(
                userId,
                photos,
                existingPhotos,
                result,
                cancellationToken);

            // Get primary photo after update
            await UpdatePrimaryPhotoInfoAsync(userId, result, cancellationToken);

            result.Success = true;
            _logger.LogInformation("Successfully updated photos for user {UserId}. " +
                                  "Updated: {Updated}, Created: {Created}, Deleted: {Deleted}",
                userId, result.UpdatedPhotos.Count, result.CreatedPhotos.Count,
                result.DeletedPhotos.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update photos for user {UserId}", userId);
            result.Errors.Add($"An error occurred while updating photos: {ex.Message}");
            result.Success = false;
        }

        return result;
    }

    public async Task<PhotoUpdateResult> ReplaceUserPhotoAsync(
        ReplaceUserPhotoCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = new PhotoUpdateResult
        {
            UserId = command.UserId
        };

        try
        {
            _logger.LogInformation("Replacing photo for user {UserId}", command.UserId);

            // Validate command
            if (command.NewPhoto == null)
            {
                result.Errors.Add("New photo file is required");
                result.Success = false;
                return result;
            }

            // Get existing photos
            var existingPhotos = await _photoRepository.GetByUserIdWithTrackingAsync(
                command.UserId,
                cancellationToken);

            // Create a list with just the new photo (to reuse existing logic)
            var photoList = new List<IFormFile> { command.NewPhoto };

            await ProcessPhotoUpdatesAsync(
                command.UserId,
                photoList,
                existingPhotos,
                result,
                cancellationToken);

            await UpdatePrimaryPhotoInfoAsync(command.UserId, result, cancellationToken);

            result.Success = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to replace photo for user {UserId}", command.UserId);
            result.Errors.Add($"Failed to replace photo: {ex.Message}");
            result.Success = false;
        }

        return result;
    }

    public async Task<PrimaryPhotoInfo?> GetPrimaryPhotoInfoAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var primaryPhoto = await _photoRepository.GetPrimaryPhotoAsync(userId, cancellationToken);

            if (primaryPhoto == null)
            {
                // Try to get the first photo as fallback
                var allPhotos = await _photoRepository.GetByUserIdAsync(userId, cancellationToken);
                primaryPhoto = allPhotos.OrderBy(p => p.DisplayOrder).FirstOrDefault();

                if (primaryPhoto == null)
                {
                    _logger.LogDebug("No photos found for user {UserId}", userId);
                    return null;
                }

                _logger.LogDebug("Using first photo as primary for user {UserId}", userId);
            }

            return new PrimaryPhotoInfo
            {
                PhotoId = primaryPhoto.Id,
                Url = primaryPhoto.Url,
                DisplayOrder = primaryPhoto.DisplayOrder,
                IsPrimary = primaryPhoto.IsPrimary,
                UploadedAt = primaryPhoto.UploadedAt,
                UserId = primaryPhoto.UserId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get primary photo info for user {UserId}", userId);
            throw;
        }
    }

    public async Task<string?> GetPrimaryPhotoUrlAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var primaryPhotoInfo = await GetPrimaryPhotoInfoAsync(userId, cancellationToken);
            return primaryPhotoInfo?.Url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get primary photo URL for user {UserId}", userId);
            throw;
        }
    }

    public async Task<PhotoValidationResult> ValidatePhotosAsync(
        List<IFormFile> photos,
        CancellationToken cancellationToken = default)
    {
        var result = new PhotoValidationResult();

        if (photos == null || photos.Count == 0)
        {
            result.IsValid = true;
            return result;
        }

        // Check for duplicate filenames
        var duplicateFileNames = photos
            .GroupBy(x => x.FileName)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateFileNames.Any())
        {
            result.Errors.Add($"Duplicate file names found: {string.Join(", ", duplicateFileNames)}");
        }

        // Validate each file
        foreach (var photo in photos)
        {
            var fileValidation = ValidateFile(photo);
            if (!fileValidation.IsValid)
            {
                result.Errors.AddRange(fileValidation.Errors.Select(e => $"{photo.FileName}: {e}"));
            }
            else
            {
                result.ValidFiles.Add(photo);
            }
        }

        result.IsValid = !result.Errors.Any();
        return result;
    }

    // Private helper methods
    private FileValidationResult ValidateFile(IFormFile file)
    {
        var result = new FileValidationResult();

        if (file == null || file.Length == 0)
        {
            result.Errors.Add("File is empty");
            return result;
        }

        // Check file size (10MB limit)
        const long maxFileSize = 10 * 1024 * 1024;
        if (file.Length > maxFileSize)
        {
            result.Errors.Add($"File size exceeds maximum allowed size of {maxFileSize / 1024 / 1024}MB");
        }

        // Validate file extension
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
        {
            result.Errors.Add($"File extension '{extension}' is not allowed. Allowed extensions: {string.Join(", ", allowedExtensions)}");
        }

        // Validate MIME type
        var allowedMimeTypes = new[]
        {
            "image/jpeg", "image/png", "image/gif", "image/bmp", "image/webp"
        };

        if (!string.IsNullOrEmpty(file.ContentType) &&
            !allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
        {
            result.Errors.Add($"File type '{file.ContentType}' is not allowed");
        }

        result.IsValid = !result.Errors.Any();
        return result;
    }

    private async Task DeleteAllPhotosAsync(
        List<UserPhoto> existingPhotos,
        PhotoUpdateResult result,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Deleting {Count} photos for user {UserId}",
            existingPhotos.Count, result.UserId);

        foreach (var photo in existingPhotos)
        {
            try
            {
                // Delete file from storage
                var deleteResult = await _fileStorageService.DeleteFileAsync(photo.Url, cancellationToken);
                if (deleteResult.IsSuccess)
                {
                    result.DeletedPhotoUrls.Add(photo.Url);
                }
                else
                {
                    result.Errors.Add($"Failed to delete file from storage: {photo.Url}");
                }

                // Delete from database
                await _photoRepository.DeleteAsync(photo.Id, cancellationToken);
                result.DeletedPhotos.Add(photo);

                _logger.LogDebug("Deleted photo {PhotoId} from user {UserId}",
                    photo.Id, photo.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete photo {PhotoId}", photo.Id);
                result.Errors.Add($"Failed to delete photo {photo.Url}: {ex.Message}");
            }
        }
    }

    private async Task ProcessPhotoUpdatesAsync(
        Guid userId,
        List<IFormFile> newPhotos,
        List<UserPhoto> existingPhotos,
        PhotoUpdateResult result,
        CancellationToken cancellationToken)
    {
        // Create a dictionary of existing photos by filename
        var existingPhotosByFileName = existingPhotos
            .ToDictionary(p => GetFileNameFromUrl(p.Url));

        var processedPhotoIds = new HashSet<Guid>();

        // Process each new photo in order
        for (int i = 0; i < newPhotos.Count; i++)
        {
            var formFile = newPhotos[i];
            var fileName = formFile.FileName;
            var isPrimary = i == 0; // First file is primary
            var displayOrder = i;

            _logger.LogDebug("Processing photo {FileName} for user {UserId} (Primary: {IsPrimary}, Order: {Order})",
                fileName, userId, isPrimary, displayOrder);

            try
            {
                // Check if this is an existing photo
                if (existingPhotosByFileName.TryGetValue(fileName, out var existingPhoto))
                {
                    // Update existing photo
                    await UpdateExistingPhotoAsync(
                        existingPhoto,
                        displayOrder,
                        isPrimary,
                        result,
                        cancellationToken);

                    processedPhotoIds.Add(existingPhoto.Id);
                    existingPhotosByFileName.Remove(fileName); // Remove from dict so remaining will be deleted

                    _logger.LogDebug("Updated existing photo {PhotoId} for user {UserId}",
                        existingPhoto.Id, userId);
                }
                else
                {
                    // Create new photo
                    await CreateNewPhotoAsync(
                        userId,
                        formFile,
                        displayOrder,
                        isPrimary,
                        result,
                        cancellationToken);

                    _logger.LogDebug("Created new photo for user {UserId} from file {FileName}",
                        userId, fileName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process photo {FileName} for user {UserId}",
                    fileName, userId);
                result.Errors.Add($"Failed to process '{fileName}': {ex.Message}");
            }
        }

        // Delete photos that are no longer in the list
        await DeleteRemovedPhotosAsync(existingPhotosByFileName.Values, result, cancellationToken);

        // Update any remaining existing photos that need reordering
        await UpdateRemainingPhotosOrderAsync(existingPhotos, processedPhotoIds, result, cancellationToken);
    }

    private async Task UpdateExistingPhotoAsync(
        UserPhoto photo,
        int newDisplayOrder,
        bool newIsPrimary,
        PhotoUpdateResult result,
        CancellationToken cancellationToken)
    {
        var changesMade = false;

        // Update display order if changed
        if (photo.DisplayOrder != newDisplayOrder)
        {
            var orderResult = photo.UpdateDisplayOrder(newDisplayOrder);
            if (orderResult.IsSuccess)
            {
                changesMade = true;
                _logger.LogTrace("Updated display order for photo {PhotoId} from {OldOrder} to {NewOrder}",
                    photo.Id, photo.DisplayOrder, newDisplayOrder);
            }
            else
            {
                result.Errors.Add($"Failed to update display order for photo {photo.Url}: {orderResult.Error}");
            }
        }

        // Update primary status if changed
        if (newIsPrimary && !photo.IsPrimary)
        {
            // First, ensure no other photo is marked as primary
            await _photoRepository.MarkAllAsSecondaryAsync(photo.UserId, cancellationToken);

            photo.SetAsPrimary();
            changesMade = true;

            _logger.LogDebug("Set photo {PhotoId} as primary for user {UserId}",
                photo.Id, photo.UserId);
        }
        else if (!newIsPrimary && photo.IsPrimary)
        {
            photo.SetAsSecondary();
            changesMade = true;

            _logger.LogDebug("Set photo {PhotoId} as secondary for user {UserId}",
                photo.Id, photo.UserId);
        }

        // Save changes if any were made
        if (changesMade)
        {
            await _photoRepository.UpdateAsync(photo, cancellationToken);
            result.UpdatedPhotos.Add(photo);
        }
        else
        {
            _logger.LogTrace("No changes needed for photo {PhotoId}", photo.Id);
        }
    }

    private async Task CreateNewPhotoAsync(
        Guid userId,
        IFormFile formFile,
        int displayOrder,
        bool isPrimary,
        PhotoUpdateResult result,
        CancellationToken cancellationToken)
    {
        // Ensure no other photo is primary if this one should be
        if (isPrimary)
        {
            await _photoRepository.MarkAllAsSecondaryAsync(userId, cancellationToken);
            _logger.LogDebug("Marked all existing photos as secondary for user {UserId}", userId);
        }

        // Upload the file
        var uploadResult = await _fileStorageService.UploadFileAsync(formFile, cancellationToken);
        if (uploadResult.IsFailure)
        {
            result.Errors.Add($"Failed to upload '{formFile.FileName}': {uploadResult.Error}");
            return;
        }

        _logger.LogDebug("Successfully uploaded file {FileName} to {Url}",
            formFile.FileName, uploadResult.Value.Url);

        // Create the photo entity
        var photoResult = UserPhoto.Create(
            userId,
            uploadResult.Value.Url,
            displayOrder,
            isPrimary);

        if (photoResult.IsFailure)
        {
            // Rollback file upload
            await _fileStorageService.DeleteFileAsync(uploadResult.Value.Url, cancellationToken);
            result.Errors.Add($"Failed to create photo for '{formFile.FileName}': {photoResult.Error}");
            _logger.LogWarning("Rolled back file upload for {FileName} due to entity creation failure",
                formFile.FileName);
            return;
        }

        // Save to database
        try
        {
            await _photoRepository.AddAsync(photoResult.Value, cancellationToken);

            result.UploadedFileUrls.Add(uploadResult.Value.Url);
            result.CreatedPhotos.Add(photoResult.Value);

            _logger.LogInformation("Created new photo {PhotoId} for user {UserId} from file {FileName}",
                photoResult.Value.Id, userId, formFile.FileName);
        }
        catch (Exception ex)
        {
            // Rollback file upload
            await _fileStorageService.DeleteFileAsync(uploadResult.Value.Url, cancellationToken);
            result.Errors.Add($"Failed to save photo to database: {ex.Message}");
            _logger.LogError(ex, "Failed to save photo to database, rolled back file upload for {FileName}",
                formFile.FileName);
            throw;
        }
    }

    private async Task DeleteRemovedPhotosAsync(
        IEnumerable<UserPhoto> photosToDelete,
        PhotoUpdateResult result,
        CancellationToken cancellationToken)
    {
        foreach (var photo in photosToDelete)
        {
            try
            {
                // Delete file from storage
                var deleteResult = await _fileStorageService.DeleteFileAsync(photo.Url, cancellationToken);
                if (deleteResult.IsSuccess)
                {
                    result.DeletedPhotoUrls.Add(photo.Url);
                }
                else
                {
                    result.Errors.Add($"Failed to delete file from storage: {photo.Url}");
                }

                // Delete from database
                await _photoRepository.DeleteAsync(photo.Id, cancellationToken);
                result.DeletedPhotos.Add(photo);

                _logger.LogInformation("Deleted photo {PhotoId} for user {UserId}",
                    photo.Id, photo.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete photo {PhotoId}", photo.Id);
                result.Errors.Add($"Failed to delete photo {photo.Url}: {ex.Message}");
            }
        }
    }

    private async Task UpdateRemainingPhotosOrderAsync(
        List<UserPhoto> allExistingPhotos,
        HashSet<Guid> processedPhotoIds,
        PhotoUpdateResult result,
        CancellationToken cancellationToken)
    {
        // Find photos that weren't processed but might need their primary status updated
        var unprocessedPhotos = allExistingPhotos
            .Where(p => !processedPhotoIds.Contains(p.Id))
            .ToList();

        if (!unprocessedPhotos.Any())
        {
            return;
        }

        _logger.LogDebug("Checking {Count} unprocessed photos for user {UserId}",
            unprocessedPhotos.Count, result.UserId);

        foreach (var photo in unprocessedPhotos)
        {
            // If this photo was primary but wasn't in the new list, it should become secondary
            if (photo.IsPrimary)
            {
                photo.SetAsSecondary();
                await _photoRepository.UpdateAsync(photo, cancellationToken);
                result.UpdatedPhotos.Add(photo);

                _logger.LogDebug("Set unprocessed photo {PhotoId} as secondary for user {UserId}",
                    photo.Id, photo.UserId);
            }
        }
    }

    private async Task UpdatePrimaryPhotoInfoAsync(
        Guid userId,
        PhotoUpdateResult result,
        CancellationToken cancellationToken)
    {
        try
        {
            var primaryPhoto = await _photoRepository.GetPrimaryPhotoAsync(userId, cancellationToken);
            if (primaryPhoto != null)
            {
                result.NewPrimaryPhoto = primaryPhoto;
                result.NewPrimaryPhotoUrl = primaryPhoto.Url;

                _logger.LogDebug("Primary photo for user {UserId} is {PhotoId}",
                    userId, primaryPhoto.Id);
            }
            else
            {
                _logger.LogDebug("No primary photo found for user {UserId}", userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get primary photo info for user {UserId}", userId);
            // Don't add to errors as this doesn't affect the main operation
        }
    }

    private string GetFileNameFromUrl(string url)
    {
        try
        {
            var uri = new Uri(url, UriKind.RelativeOrAbsolute);
            return Path.GetFileName(uri.LocalPath);
        }
        catch
        {
            // If URL parsing fails, try to extract filename directly
            var lastSlash = url.LastIndexOf('/');
            if (lastSlash >= 0 && lastSlash < url.Length - 1)
            {
                return url.Substring(lastSlash + 1);
            }
            return url;
        }
    }

    // Additional utility method
    public async Task<PhotoSummary> GetPhotoSummaryAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var photos = await _photoRepository.GetByUserIdAsync(userId, cancellationToken);
            var primaryPhoto = await _photoRepository.GetPrimaryPhotoAsync(userId, cancellationToken);
            var hasPhotos = await _photoRepository.HasPhotosAsync(userId, cancellationToken);

            return new PhotoSummary
            {
                UserId = userId,
                TotalCount = photos.Count,
                HasPhotos = hasPhotos,
                HasPrimaryPhoto = primaryPhoto != null,
                PrimaryPhotoUrl = primaryPhoto?.Url,
                PrimaryPhotoId = primaryPhoto?.Id,
                Photos = photos.Select(p => new PhotoItem
                {
                    Id = p.Id,
                    Url = p.Url,
                    DisplayOrder = p.DisplayOrder,
                    IsPrimary = p.IsPrimary,
                    UploadedAt = p.UploadedAt
                }).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get photo summary for user {UserId}", userId);
            throw;
        }
    }
}


