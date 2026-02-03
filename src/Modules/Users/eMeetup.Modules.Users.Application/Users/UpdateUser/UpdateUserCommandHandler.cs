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

internal sealed class UpdateUserCommandHandler(
    IUserRepository userRepository,
    IUserPhotoService userPhotoService,
    IIdentityProviderService identityProviderService,
    IGeocodingService geocodingService,
    IUnitOfWork unitOfWork,
    ILogger<UpdateUserCommandHandler> logger)
    : ICommandHandler<UpdateUserCommand>
{
    public async Task<Result> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        using var loggerScope = logger.BeginScope("UserUpdate {IdentityId}", request.IdentityId);

        try
        {
            logger.LogInformation("Starting user update for {IdentityId}", request.IdentityId);

            // 1. Get user
            var user = await userRepository.GetByIdentityIdAsync(request.IdentityId, cancellationToken);
            if (user == null) return Result.Failure(UserErrors.NotFoundByIdentity(request.IdentityId));

            var updates = new UserUpdateSet(user);

            // 2. Handle photos (using service)
            await HandlePhotosAsync(user, request.Photos, updates, cancellationToken);

            // 3. Handle bio
            if (request.Bio != null && request.Bio != user.Bio)
            {
                user.UpdateBio(request.Bio);
                updates.Bio = request.Bio;
            }

            // 4. Handle location
            await HandleLocationAsync(user, request, updates, cancellationToken);

            // 5. Update timestamp

            // 6. Save changes
            await unitOfWork.SaveChangesAsync(cancellationToken);

            // 7. Update Keycloak if needed
            if (updates.HasUpdates)
            {
                var keycloakResult = await UpdateKeycloakWithRetryAsync(
                    request.IdentityId, updates, cancellationToken);

                if (keycloakResult.IsFailure)
                {
                    // Cleanup uploaded photos
                    

                    // Attempt to rollback Keycloak changes
                    await RollbackKeycloakChangesAsync(request.IdentityId, updates, cancellationToken);

                    logger.LogError("Keycloak update failed: {Error}", keycloakResult.Error);
                    return Result.Failure(keycloakResult.Error);
                }

                logger.LogInformation("Keycloak update successful");
            }

            logger.LogInformation("User update completed for {IdentityId}", request.IdentityId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "User update failed for {IdentityId}", request.IdentityId);
            return Result.Failure(UserErrors.UpdateFailed);
        }
    }

    private async Task HandlePhotosAsync(
        User user,
        List<IFormFile>? photos,
        UserUpdateSet updates,
        CancellationToken cancellationToken)
    {
        if (photos?.Count == 0) return;

        var result = await userPhotoService.ProcessPhotosAndUpdateProfileAsync(
            user.Id, photos, cancellationToken);

        if (result.IsSuccess && result.Value != null)
        {
            updates.ProfilePictureUrl = result.Value;
        }
    }

    private async Task HandleLocationAsync(
        User user,
        UpdateUserCommand request,
        UserUpdateSet updates,
        CancellationToken cancellationToken)
    {
        if (!request.Latitude.HasValue || !request.Longitude.HasValue) return;

        var locationResult = await CreateLocationAsync(
            request.Latitude.Value,
            request.Longitude.Value,
            request.City,
            request.Country,
            cancellationToken);

        if (locationResult.IsSuccess)
        {
            user.UpdateLocation(locationResult.Value);
            updates.Latitude = request.Latitude;
            updates.Longitude = request.Longitude;
            updates.City = request.City;
            updates.Country = request.Country;
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

            var geocodingResult = await geocodingService.ReverseGeocodeAsync(latitude, longitude, cancellationToken);
            return geocodingResult.IsSuccess
                ? geocodingResult
                : Location.Create(latitude, longitude, "Unknown", "Unknown");
        }
        catch (Exception)
        {
            return Location.Create(latitude, longitude, "Unknown", "Unknown");
        }
    }

    private async Task<Result> UpdateKeycloakWithRetryAsync(
        Guid identityId,
        UserUpdateSet updates,
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
                    profilePictureUrl: updates.ProfilePictureUrl,
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

    private class UserUpdateSet
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
}
