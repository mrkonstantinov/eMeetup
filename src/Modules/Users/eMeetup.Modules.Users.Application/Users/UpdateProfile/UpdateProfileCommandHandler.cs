using System.Data;
using System.Net;
using eMeetup.Common.Application.Messaging;
using eMeetup.Common.Domain;
using eMeetup.Modules.Users.Application.Abstractions.Authentication;
using eMeetup.Modules.Users.Application.Abstractions.Data;
using eMeetup.Modules.Users.Application.Abstractions.Identity;
using eMeetup.Modules.Users.Application.Users.UpdateUser;
using eMeetup.Modules.Users.Domain.Interfaces.Repositories;
using eMeetup.Modules.Users.Domain.Interfaces.Services;
using eMeetup.Modules.Users.Domain.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace eMeetup.Modules.Users.Application.Users.UpdateProfile;

internal sealed class UpdateProfileCommandHandler(
    IUserRepository userRepository,
    IUserContext userContext,
    IUnitOfWork unitOfWork,
    ILogger<UpdateProfileCommandHandler> logger)
    : ICommandHandler<UpdateProfileCommand>
{
    public async Task<Result> Handle(
        UpdateProfileCommand request,
        CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;

        using var loggerScope = logger.BeginScope(
            "UpdateProfile {UserId}",
            userId);

        try
        {
            logger.LogInformation("Starting profile update for user {UserId}", userId);

            // ============================================================
            // 1. Загружаем пользователя
            // ============================================================
            var user = await userRepository.GetByIdAsync(userId, cancellationToken);

            if (user is null)
            {
                logger.LogWarning("User {UserId} not found", userId);
                return Result.Failure(UserErrors.NotFound(userId.ToString()));
            }

            // ============================================================
            // 2. Проверяем статус
            // ============================================================
            if (user.Status == UserStatus.Deleted)
            {
                logger.LogWarning("Cannot update profile of deleted user {UserId}", userId);
                return Result.Failure(ProfileErrors.CannotUpdateDeletedUser);
            }

            if (user.Status == UserStatus.Suspended)
            {
                logger.LogWarning("Cannot update profile of suspended user {UserId}", userId);
                return Result.Failure(ProfileErrors.CannotUpdateSuspendedUser);
            }

            // ============================================================
            // 3. Обновляем поля профиля (только переданные)
            // ============================================================
            logger.LogInformation("Applying partial update for user {UserId}", userId);

            var profile = user.Profile;

            // --- Личная информация ---
            if (request.Bio is not null ||
                request.DateOfBirth.HasValue ||
                request.Gender is not null)
            {
                profile = profile.UpdateBasicInfo(
                    bio: request.Bio ?? profile.Bio,
                    dateOfBirth: request.DateOfBirth ?? profile.DateOfBirth,
                    gender: request.Gender ?? profile.Gender);
            }

            // --- Контакты ---
            if (request.Phone is not null ||
                request.Telegram is not null ||
                request.Instagram is not null)
            {
                profile = profile.UpdateContacts(
                    phone: request.Phone ?? profile.Phone,
                    telegram: request.Telegram ?? profile.Telegram,
                    instagram: request.Instagram ?? profile.Instagram);
            }

            // --- Местоположение ---
            if (request.City is not null ||
                request.Country is not null ||
                request.Latitude.HasValue ||
                request.Longitude.HasValue ||
                request.TimeZone is not null)
            {
                profile = profile.UpdateLocation(
                    city: request.City ?? profile.City,
                    country: request.Country ?? profile.Country,
                    latitude: request.Latitude ?? profile.Latitude,
                    longitude: request.Longitude ?? profile.Longitude,
                    timeZone: request.TimeZone ?? profile.TimeZone);
            }

            // --- Социальные характеристики ---
            if (request.Languages is not null)
            {
                profile = profile.UpdateSocial(
                    gender: profile.Gender,
                    languages: request.Languages);
            }

            // --- Интересы ---
            if (request.Interests is not null)
            {
                profile = profile.UpdateInterests(request.Interests);
            }

            // --- Приватность ---
            if (request.IsPublic.HasValue)
            {
                profile = profile.UpdatePrivacy(request.IsPublic.Value);
            }

            // ============================================================
            // 4. Применяем обновлённый профиль к пользователю
            // ============================================================
            var updateResult = user.UpdateProfile(profile);

            if (updateResult.IsFailure)
            {
                logger.LogError(
                    "Failed to update profile for user {UserId}: {Error}",
                    userId,
                    updateResult.Error);

                return updateResult;
            }

            // ============================================================
            // 5. Сохраняем в БД
            // ============================================================
            logger.LogInformation("Saving user {UserId} to database", userId);

            var affectedRows = await unitOfWork.SaveChangesAsync(cancellationToken);

            if (affectedRows == 0)
            {
                logger.LogError(
                    "No rows were affected during profile update for user {UserId}",
                    userId);

                return Result.Failure(UserErrors.DatabaseSaveFailed("No rows affected"));
            }

            // ============================================================
            // 6. Успех
            // ============================================================
            logger.LogInformation("Profile for user {UserId} updated successfully", userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "An unexpected error occurred during profile update for user {UserId}",
                userId);

            return Result.Failure(ProfileErrors.ProfileUpdateFailed(ex.Message));
        }
    }
}
