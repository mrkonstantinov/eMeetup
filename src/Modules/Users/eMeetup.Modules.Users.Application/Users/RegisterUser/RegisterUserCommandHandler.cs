using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using eMeetup.Common.Application.Messaging;
using eMeetup.Common.Domain;
using eMeetup.Modules.Users.Application.Abstractions.Data;
using eMeetup.Modules.Users.Application.Abstractions.Identity;
using eMeetup.Modules.Users.Domain.Helpers;
using eMeetup.Modules.Users.Domain.Interfaces.Repositories;
using eMeetup.Modules.Users.Domain.Interfaces.Services;
using eMeetup.Modules.Users.Domain.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using Serilog.Core;

namespace eMeetup.Modules.Users.Application.Users.RegisterUser;

internal sealed class RegisterUserCommandHandler(
    IIdentityProviderService identityProviderService,
    IUserRepository userRepository,
    ISlugService slugService,
    IFileStorageService fileStorageService,
    IGeocodingService geocodingService,
    IUnitOfWork unitOfWork,
    ILogger<RegisterUserCommandHandler> logger)
    : ICommandHandler<RegisterUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        using var loggerScope = logger.BeginScope("UserRegistration {Email} {Username}",
            request.Email, request.Username);

        try
        {
            logger.LogInformation("Starting user registration process for {Email}", request.Email);

            // Check for existing user
            var existingUserResult = await CheckExistingUserAsync(request.Email, request.Username, cancellationToken);
            if (existingUserResult.IsFailure)
            {
                return Result.Failure<Guid>(existingUserResult.Error);
            }

            // No need to parse gender - it's already an enum!
            var gender = request.Gender;

            // Create location if coordinates provided
            Location? location = null;
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
                    return Result.Failure<Guid>(locationResult.Error);
                }
                location = locationResult.Value;
            }

            // Process interests if provided
            string existingInterests = string.Empty;
            IEnumerable<Tag> slugs = [];
            if (request.Interests != null)
            {
                slugs = await slugService.GetExistingSlugsAsync(request.Interests, cancellationToken);
                if (slugs != null)
                {
                    existingInterests = SlugHelper.CombineSlugs(slugs, includeSpaces: true);
                }
            }

            // Register with identity provider first
            logger.LogInformation("Registering user with identity provider: {Email}", request.Email);
            var identityResult = await identityProviderService.RegisterUserAsync(
                new UserModel(
                    request.Email,
                    request.Password,
                    request.Username,
                    request.DateOfBirth,
                    gender,
                    request.Bio,
                    location,
                    existingInterests),
                cancellationToken);

            if (identityResult.IsFailure)
            {
                logger.LogError("Identity provider registration failed for {Email}: {Error}",
                    request.Email, identityResult.Error);
                return Result.Failure<Guid>(identityResult.Error);
            }

            // Create user domain entity
            logger.LogInformation("Creating user domain entity for {Email}", request.Email);
            var userResult = User.Create(
                request.Email,
                request.Username,
                request.DateOfBirth,
                gender,
                request.Bio,
                identityResult.Value,
                location,
                slugs);

            if (userResult.IsFailure)
            {
                logger.LogError("User domain entity creation failed for {Email}: {Error}",
                    request.Email, userResult.Error);

                await TryCleanupIdentityUserAsync(identityResult.Value, request.Email);
                return Result.Failure<Guid>(userResult.Error);
            }

            var user = userResult.Value;

            // Handle photo uploads if provided (NO VALIDATION NEEDED - already validated)
            if (request.Photos?.Count > 0)
            {
                var photoResult = await HandlePhotoUploadsAsync(user, request.Photos, cancellationToken);
                if (photoResult.IsFailure)
                {
                    logger.LogError("Photo upload failed for {Email}: {Error}",
                        request.Email, photoResult.Error);

                    await TryCleanupIdentityUserAsync(identityResult.Value, request.Email);
                    return Result.Failure<Guid>(photoResult.Error);
                }
            }

            // Save user to database
            logger.LogInformation("Saving user to database: {Email}", request.Email);
            userRepository.Insert(user);

            var affectedRows = await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Database save completed. Affected rows: {AffectedRows}", affectedRows);

            if (affectedRows == 0)
            {
                logger.LogError("No rows were affected during database save for {Email}", request.Email);
                await TryCleanupIdentityUserAsync(identityResult.Value, request.Email);
                return Result.Failure<Guid>(UserErrors.DatabaseSaveFailed);
            }

            logger.LogInformation("User registered successfully: {Email} with ID: {UserId}. Interests: {InterestCount}",
                request.Email, user.Id, user.Interests.Count);

            return user.Id;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred during user registration for {Email}",
                request.Email);
            return Result.Failure<Guid>(UserErrors.RegistrationFailed);
        }
    }

    private async Task<Result> CheckExistingUserAsync(string email, string username, CancellationToken cancellationToken)
    {
        var existingByEmail = await userRepository.GetByEmailAsync(email, cancellationToken);
        if (existingByEmail != null)
        {
            logger.LogWarning("Registration attempt with existing email: {Email}", email);
            return Result.Failure(UserErrors.EmailAlreadyExists(email));
        }

        var existingByUsername = await userRepository.GetByUsernameAsync(username, cancellationToken);
        if (existingByUsername != null)
        {
            logger.LogWarning("Registration attempt with existing username: {Username}", username);
            return Result.Failure(UserErrors.UsernameAlreadyExists(username));
        }

        return Result.Success();
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

    private async Task<Result> HandlePhotoUploadsAsync(User user, List<IFormFile> photos, CancellationToken cancellationToken)
    {
        var uploadedPhotoUrls = new List<string>();

        try
        {
            logger.LogInformation("Starting photo upload for {PhotoCount} photos", photos.Count);

            for (int i = 0; i < photos.Count; i++)
            {
                var photo = photos[i];
                var isPrimary = i == 0;

                logger.LogInformation("Uploading photo {Index}/{Total}: {FileName}",
                    i + 1, photos.Count, photo.FileName);

                // NO VALIDATION HERE - Photos are already validated by the validator
                await using var stream = photo.OpenReadStream();
                var uploadResult = await fileStorageService.UploadPhotoAsync(
                    stream, photo.FileName, photo.ContentType, cancellationToken);

                if (uploadResult.IsFailure)
                {
                    logger.LogError("Photo upload failed for {FileName}: {Error}",
                        photo.FileName, uploadResult.Error);

                    await CleanupUploadedPhotosAsync(uploadedPhotoUrls, cancellationToken);
                    return Result.Failure(UserErrors.PhotoUploadFailed);
                }

                uploadedPhotoUrls.Add(uploadResult.Value);

                var addResult = user.AddPhoto(uploadResult.Value, isPrimary);
                if (addResult.IsFailure)
                {
                    logger.LogError("Failed to add photo to user: {Error}", addResult.Error);
                    await CleanupUploadedPhotosAsync(uploadedPhotoUrls, cancellationToken);
                    return Result.Failure(addResult.Error);
                }

                logger.LogInformation("Photo uploaded successfully: {FileName} -> {Url}",
                    photo.FileName, uploadResult.Value);
            }

            logger.LogInformation("Successfully uploaded and added {Count} photos to user", photos.Count);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during photo uploads");
            await CleanupUploadedPhotosAsync(uploadedPhotoUrls, cancellationToken);
            return Result.Failure(UserErrors.PhotoUploadFailed);
        }
    }

    private async Task CleanupUploadedPhotosAsync(List<string> uploadedPhotoUrls, CancellationToken cancellationToken)
    {
        if (!uploadedPhotoUrls.Any()) return;

        logger.LogInformation("Cleaning up {Count} uploaded photos due to failure", uploadedPhotoUrls.Count);

        foreach (var photoUrl in uploadedPhotoUrls)
        {
            try
            {
                await fileStorageService.DeletePhotoAsync(photoUrl, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to cleanup photo: {Url}", photoUrl);
            }
        }
    }

    private async Task TryCleanupIdentityUserAsync(string identityId, string email)
    {
        try
        {
            logger.LogInformation("Attempting to cleanup identity user: {Email} (IdentityId: {IdentityId})",
                email, identityId);

            // Implementation depends on your identity service
            logger.LogWarning("Identity user cleanup not implemented for: {Email}", email);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during identity user cleanup for: {Email}", email);
        }
    }
}
