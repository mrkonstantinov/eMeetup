using System.Data;
using System.Data.Common;
using Dapper;
using eMeetup.Common.Application.Data;
using eMeetup.Common.Application.Messaging;
using eMeetup.Common.Domain;
using eMeetup.Modules.Users.Application.Abstractions.Authentication;
using eMeetup.Modules.Users.Application.Abstractions.Identity;
using eMeetup.Modules.Users.Domain.Interfaces.Repositories;
using eMeetup.Modules.Users.Domain.Users;
using Microsoft.Extensions.Logging;

namespace eMeetup.Modules.Users.Application.Users.GetUser;

internal sealed class GetUserQueryHandler(
    IUserRepository userRepository,
    IUserContext userContext,
    ILogger<GetUserQueryHandler> logger)
    : IQueryHandler<GetUserQuery, UserResponse>
{
    public async Task<Result<UserResponse>> Handle(
        GetUserQuery request,
        CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;

        using var loggerScope = logger.BeginScope(
            "GetUserProfile {UserId}",
            userId);

        try
        {
            logger.LogInformation("Getting profile for user {UserId}", userId);

            var user = await userRepository.GetByIdWithAllAsync(userId, cancellationToken);

            if (user is null)
            {
                logger.LogWarning("User {UserId} not found", userId);
                return Result.Failure<UserResponse>(UserErrors.NotFound(userId.ToString()));
            }

            var response = MapToResponse(user);

            logger.LogInformation("Profile for user {UserId} retrieved successfully", userId);

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "An unexpected error occurred while getting profile for user {UserId}",
                userId);

            return Result.Failure<UserResponse>(
                UserErrors.OperationFailed(ex.Message));
        }
    }

    // ================================================================
    // === MAPPING ===
    // ================================================================

    private static UserResponse MapToResponse(User user)
    {
        var profile = user.Profile;

        return new UserResponse
        {
            Id = user.Id,
            UserName = user.Username,
            Email = user.Email,
            Status = user.Status.ToString(),
            ProfileCompleted = user.ProfileCompleted,
            CreatedAt = user.CreatedAt,
            LastActiveAt = user.LastActiveAt,
            Profile = new UserProfileDetails
            {
                // Личная информация
                AvatarUrl = profile.AvatarUrl,
                DateOfBirth = profile.DateOfBirth,
                Age = profile.DateOfBirth.HasValue ? profile.GetAge() : null,
                Bio = profile.Bio,

                // Контакты
                Phone = profile.Phone,
                Telegram = profile.Telegram,
                Instagram = profile.Instagram,

                // Местоположение
                City = profile.City,
                Country = profile.Country,
                Latitude = profile.Latitude,
                Longitude = profile.Longitude,
                TimeZone = profile.TimeZone,

                // Социальные характеристики
                Gender = profile.Gender,
                Languages = profile.GetLanguagesArray(),

                // Интересы
                Interests = profile.GetInterestsArray(),

                // Активности
                ActivityPreferences = new ActivityPreferencesResponse
                {
                    PreferredActivities = profile.ActivityPreferences.PreferredActivities
                        .Select(a => a.ToString())
                        .ToArray(),
                    ActivityLevels = profile.ActivityPreferences.ActivityLevels
                        .Select(l => l.ToString())
                        .ToArray(),
                    PreferredTimeOfDay = profile.ActivityPreferences.PreferredTimeOfDay?.ToString(),
                    PreferredDays = profile.ActivityPreferences.PreferredDays
                        .Select(d => d.ToString())
                        .ToArray(),
                    MaxDistanceKm = profile.ActivityPreferences.MaxDistanceKm,
                    MinParticipants = profile.ActivityPreferences.MinParticipants,
                    MaxParticipants = profile.ActivityPreferences.MaxParticipants
                },

                // Статус
                AvailabilityStatus = new AvailabilityStatusResponse
                {
                    Type = profile.AvailabilityStatus.Type.ToString(),
                    AvailableFrom = profile.AvailabilityStatus.AvailableFrom,
                    AvailableUntil = profile.AvailabilityStatus.AvailableUntil,
                    StatusMessage = profile.AvailabilityStatus.StatusMessage
                },
                IsPublic = profile.IsPublic,

                // Верификация
                IsEmailVerified = profile.IsEmailVerified,
                IsPhoneVerified = profile.IsPhoneVerified,

                // Фото
                Photos = user.Photos
                    .OrderBy(p => p.DisplayOrder)
                    .Select(p => new UserPhotoResponse
                    {
                        Id = p.Id,
                        Url = p.Url,
                        ThumbnailUrl = p.ThumbnailUrl,
                        IsPrimary = p.IsPrimary,
                        DisplayOrder = p.DisplayOrder,
                        UploadedAt = p.UploadedAt
                    })
                    .ToArray(),
                ProfileImageUrl = user.ProfileImageUrl
            }
        };
    }
}
