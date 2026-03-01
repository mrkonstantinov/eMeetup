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
    IUserInterestRepository userInterestRepository,
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
            if (user == null)
                return Result.Failure(UserErrors.NotFoundByIdentity(request.IdentityId));

            var updates = new UserUpdateSet(user);

            // 2. Handle bio
            if (request.Bio != user.Bio)
            {
                user.UpdateBio(request.Bio);
                updates.Bio = request.Bio;
                logger.LogInformation("Updated bio for user {UserId}", user.Id);
            }

            // 3. Handle location
            var locationResult = await HandleLocationAsync(user, request, cancellationToken);
            if (locationResult.IsFailure)
                return locationResult;

            if (locationResult.Value)
            {
                updates.LocationUpdated = true;
                updates.Latitude = user.Location?.Latitude;
                updates.Longitude = user.Location?.Longitude;
                updates.City = user.Location?.City;
                updates.Street = user.Location?.Street;
            }

            // 4. Handle interests
            var interestsResult = await HandleInterestsAsync(user, request, updates, cancellationToken);
            if (interestsResult.IsFailure)
                return interestsResult;

            // 5. Save changes
            await unitOfWork.SaveChangesAsync(cancellationToken);

            // 6. Update Keycloak if needed
            if (updates.HasUpdates)
            {
                var keycloakResult = await UpdateKeycloakWithRetryAsync(
                    request.IdentityId, updates, user.ProfilePictureUrl, cancellationToken);

                if (keycloakResult.IsFailure)
                {
                    // Attempt to rollback changes
                    await RollbackChangesAsync(request.IdentityId, updates, cancellationToken);

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

    private async Task<Result<bool>> HandleLocationAsync(
        User user,
        UpdateUserCommand request,
        CancellationToken cancellationToken)
    {
        // Check if any location parameters were provided
        bool locationParamsProvided = request.Latitude.HasValue ||
                                      request.Longitude.HasValue ||
                                      request.City != null ||
                                      request.Street != null;

        if (!locationParamsProvided)
            return Result.Success(false);

        // Determine what's being updated
        bool coordinatesProvided = request.Latitude.HasValue || request.Longitude.HasValue;
        bool addressProvided = request.City != null || request.Street != null;

        // If only coordinates are provided, try to get city/street via reverse geocoding
        if (coordinatesProvided && !addressProvided && request.Latitude.HasValue && request.Longitude.HasValue)
        {
            var geocodingResult = await geocodingService.ReverseGeocodeAsync(
                request.Latitude.Value,
                request.Longitude.Value,
                cancellationToken);

            if (geocodingResult.IsSuccess && geocodingResult.Value != null)
            {
                // Use geocoded location
                var location = geocodingResult.Value;
                user.UpdateLocation(location);
                return Result.Success(true);
            }

            // If geocoding fails, create location with just coordinates
            var coordinatesOnlyResult = Location.Create(request.Latitude, request.Longitude);
            if (coordinatesOnlyResult.IsFailure)
                return (Result<bool>)Result<bool>.Failure(coordinatesOnlyResult.Error);

            user.UpdateLocation(coordinatesOnlyResult.Value);
            return Result.Success(true);
        }

        // If address is provided (with or without coordinates), use it directly
        if (addressProvided)
        {
            var locationResult = Location.Create(
                request.Latitude,
                request.Longitude,
                request.City,
                request.Street);

            if (locationResult.IsFailure)
                return (Result<bool>)Result<bool>.Failure(locationResult.Error);

            user.UpdateLocation(locationResult.Value);
            return Result.Success(true);
        }

        return Result.Success(false);
    }

    private async Task<Result> HandleInterestsAsync(
        User user,
        UpdateUserCommand request,
        UserUpdateSet updates,
        CancellationToken cancellationToken)
    {
        // Get current interests
        var currentInterests = await userInterestRepository.GetByUserIdAsync(user.Id, cancellationToken);
        var currentInterestsSet = new HashSet<string>(
            currentInterests.Select(ui => ui.Tag.Name.Trim()),
            StringComparer.OrdinalIgnoreCase);

        // Parse requested interests
        var requestedInterestsSet = ParseInterestNames(request.Interests);

        // Check if interests changed (order doesn't matter)
        if (!AreInterestSetsEqual(currentInterestsSet, requestedInterestsSet))
        {
            // Update user interests
            var updatedInterests = await userInterestRepository.UpdateUserInterestsAsync(
                user.Id,
                request.Interests ?? string.Empty,
                cancellationToken);

            // Create comma-separated string of tag names for Keycloak
            updates.Interests = updatedInterests.Any()
                ? string.Join(", ", updatedInterests.Select(ui => ui.Tag.Name))
                : null;

            logger.LogInformation("Updated interests for user {UserId}: {Interests}",
                user.Id, updates.Interests ?? "none");
        }

        return Result.Success();
    }

    private HashSet<string> ParseInterestNames(string? interests)
    {
        if (string.IsNullOrWhiteSpace(interests))
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        return interests.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(i => i.Trim())
            .Where(i => !string.IsNullOrWhiteSpace(i))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private bool AreInterestSetsEqual(HashSet<string>? set1, HashSet<string>? set2)
    {
        // If both are null or empty
        if ((set1 == null || set1.Count == 0) && (set2 == null || set2.Count == 0))
            return true;

        // If one is null/empty and the other has items
        if (set1 == null || set2 == null)
            return false;

        return set1.Count == set2.Count && set1.SetEquals(set2);
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
                    street: updates.Street,
                    interests: updates.Interests,
                    profilePictureUrl: profilePictureUrl,
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

    private async Task RollbackChangesAsync(
        Guid identityId,
        UserUpdateSet attemptedUpdates,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogWarning("Attempting to rollback changes for {IdentityId}", identityId);

            var user = await userRepository.GetByIdentityIdAsync(identityId, cancellationToken);
            if (user == null) return;

            // Rollback bio
            if (attemptedUpdates.Bio != attemptedUpdates.OriginalBio)
            {
                user.UpdateBio(attemptedUpdates.OriginalBio);
                logger.LogInformation("Rolled back bio for user {UserId}", user.Id);
            }

            // Rollback location
            if (attemptedUpdates.LocationUpdated)
            {
                var originalLocationResult = Location.Create(
                    attemptedUpdates.OriginalLatitude,
                    attemptedUpdates.OriginalLongitude,
                    attemptedUpdates.OriginalCity,
                    attemptedUpdates.OriginalStreet);

                if (originalLocationResult.IsSuccess)
                {
                    user.UpdateLocation(originalLocationResult.Value);
                    logger.LogInformation("Rolled back location for user {UserId}", user.Id);
                }
            }

            // Rollback interests
            if (attemptedUpdates.Interests != attemptedUpdates.OriginalInterests)
            {
                await userInterestRepository.UpdateUserInterestsAsync(
                    user.Id,
                    attemptedUpdates.OriginalInterests ?? string.Empty,
                    cancellationToken);

                logger.LogInformation("Rolled back interests for user {UserId}", user.Id);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogWarning("""
                Rollback completed for {IdentityId}. 
                Attempted updates: {Updates}
                """,
                identityId,
                attemptedUpdates.UpdatedFieldsString);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to rollback changes for {IdentityId}", identityId);
        }
    }

    private class UserUpdateSet
    {
        // Current values
        public string? Bio { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? City { get; set; }
        public string? Street { get; set; }
        public string? Interests { get; set; }
        public bool LocationUpdated { get; set; }

        // Original values
        public string? OriginalBio { get; }
        public double? OriginalLatitude { get; }
        public double? OriginalLongitude { get; }
        public string? OriginalCity { get; }
        public string? OriginalStreet { get; }
        public string? OriginalInterests { get; }

        public bool HasUpdates =>
            Bio != OriginalBio ||
            LocationUpdated ||
            Interests != OriginalInterests;

        public string UpdatedFieldsString
        {
            get
            {
                var fields = new List<string>();
                if (Bio != OriginalBio) fields.Add("Bio");
                if (LocationUpdated) fields.Add("Location");
                if (Interests != OriginalInterests) fields.Add("Interests");
                return fields.Count > 0 ? string.Join(", ", fields) : "None";
            }
        }

        public UserUpdateSet(User user)
        {
            // Store original values
            OriginalBio = user.Bio;

            OriginalLatitude = user.Location?.Latitude;
            OriginalLongitude = user.Location?.Longitude;
            OriginalCity = user.Location?.City;
            OriginalStreet = user.Location?.Street;

            if (user.Interests != null && user.Interests.Any())
            {
                OriginalInterests = string.Join(", ", user.Interests.Select(i => i.Tag.Name));
            }

            // Initialize current values
            Bio = user.Bio;
            Interests = OriginalInterests;
        }
    }
}
