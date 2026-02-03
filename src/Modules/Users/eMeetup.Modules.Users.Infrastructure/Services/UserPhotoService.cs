using System;
using System.Collections.Generic;
using System.Text;
using eMeetup.Common.Domain;
using eMeetup.Modules.Users.Domain.Interfaces.Repositories;
using eMeetup.Modules.Users.Domain.Interfaces.Services;
using eMeetup.Modules.Users.Domain.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace eMeetup.Modules.Users.Infrastructure.Services;

public class UserPhotoService : IUserPhotoService
{
    private readonly IPhotoRepository _photoRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserPhotoService> _logger;

    public UserPhotoService(
        IPhotoRepository photoRepository,
        IFileStorageService fileStorageService,
        IUserRepository userRepository,
        ILogger<UserPhotoService> logger)
    {
        _photoRepository = photoRepository;
        _fileStorageService = fileStorageService;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<Result<List<string>>> UploadUserPhotosAsync(
        Guid userId,
        List<IFormFile> photos,
        CancellationToken cancellationToken = default)
    {
        if (photos?.Count == 0) return Result.Success(new List<string>());

        try
        {
            // 1. Validate
            var validationResult = await ValidatePhotosAsync(photos, cancellationToken);
            if (validationResult.IsFailure) return Result.Failure<List<string>>(validationResult.Error);

            // 2. Upload to storage
            var uploadedUrls = new List<string>();
            foreach (var photo in photos)
            {
                var uploadResult = await UploadPhotoToStorageAsync(photo, userId.ToString(), cancellationToken);
                if (uploadResult.IsFailure)
                {
                    await RollbackUploadedPhotosAsync(uploadedUrls, cancellationToken);
                    return Result.Failure<List<string>>(uploadResult.Error);
                }
                uploadedUrls.Add(uploadResult.Value);
            }

            // 3. Save to database
            var saveResult = await _photoRepository.AddPhotosAsync(userId, uploadedUrls, cancellationToken);
            if (saveResult.IsFailure)
            {
                await RollbackUploadedPhotosAsync(uploadedUrls, cancellationToken);
                return Result.Failure<List<string>>(saveResult.Error);
            }

            return Result.Success(uploadedUrls);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading photos for user {UserId}", userId);
            return Result.Failure<List<string>>(UserErrors.PhotoUploadFailed);
        }
    }

    public async Task<Result<string?>> ProcessPhotosAndUpdateProfileAsync(
        Guid userId,
        List<IFormFile>? photos,
        CancellationToken cancellationToken = default)
    {
        if (photos?.Count == 0) return Result.Success<string?>(null);

        try
        {
            // Upload photos
            var uploadResult = await UploadUserPhotosAsync(userId, photos, cancellationToken);
            if (uploadResult.IsFailure) return Result.Failure<string?>(uploadResult.Error);

            var uploadedUrls = uploadResult.Value;
            if (uploadedUrls.Count == 0) return Result.Success<string?>(null);

            // Set first photo as profile picture
            var profilePictureUrl = uploadedUrls[0];

            // Update user's profile picture
            var user = await _userRepository.GetAsync(userId, cancellationToken);
            if (user == null) return Result.Failure<string?>(UserErrors.NotFound);

            user.UpdateProfilePictureUrl(profilePictureUrl);

            return Result.Success<string?>(profilePictureUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing photos for user {UserId}", userId);
            return Result.Failure<string?>(UserErrors.PhotoUpdateFailed);
        }
    }

    private async Task<Result> ValidatePhotosAsync(List<IFormFile> photos, CancellationToken cancellationToken)
    {
        if (photos.Count > 10) return Result.Failure(UserErrors.TooManyPhotos(10));

        long totalSize = 0;
        foreach (var photo in photos)
        {
            if (photo.Length > 10 * 1024 * 1024)
                return Result.Failure(UserErrors.PhotoTooLarge(10 * 1024 * 1024));

            totalSize += photo.Length;
            if (totalSize > 50 * 1024 * 1024)
                return Result.Failure(UserErrors.TotalPhotoSizeExceeded);
        }

        return Result.Success();
    }

    private async Task<Result<string>> UploadPhotoToStorageAsync(
        IFormFile photo,
        string userId,
        CancellationToken cancellationToken,
        int maxRetries = 3)
    {
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await using var stream = photo.OpenReadStream();
                var fileName = GenerateUniqueFileName(photo.FileName, userId);

                var result = await _fileStorageService.UploadPhotoAsync(
                    stream, fileName, photo.ContentType, cancellationToken);

                if (result.IsSuccess) return result;

                if (attempt == maxRetries) return Result.Failure<string>(result.Error);

                await Task.Delay(100 * attempt, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Upload attempt {Attempt} failed for {FileName}",
                    attempt, photo.FileName);
                if (attempt == maxRetries)
                    return Result.Failure<string>(UserErrors.PhotoUploadFailed);
            }
        }

        return Result.Failure<string>(UserErrors.PhotoUploadFailed);
    }

    private async Task RollbackUploadedPhotosAsync(List<string> urls, CancellationToken cancellationToken)
    {
        foreach (var url in urls)
        {
            try
            {
                await _fileStorageService.DeletePhotoAsync(url, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to rollback photo: {Url}", url);
            }
        }
    }

    private string GenerateUniqueFileName(string originalFileName, string userId)
    {
        var extension = Path.GetExtension(originalFileName);
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
        var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);

        return $"{userId}_{fileNameWithoutExtension}_{timestamp}_{uniqueId}{extension}";
    }

    public async Task<Result> DeleteUserPhotosAsync(
        Guid userId,
        List<string> photoUrls,
        CancellationToken cancellationToken = default)
    {
        try
        {
            foreach (var url in photoUrls)
            {
                await _fileStorageService.DeletePhotoAsync(url, cancellationToken);
            }

            // Optionally, remove from database via repository
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting photos for user {UserId}", userId);
            return Result.Failure(UserErrors.PhotoDeletionFailed);
        }
    }
}
