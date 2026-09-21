using eMeetup.Common.Application.Messaging;
using eMeetup.Common.Domain;
using eMeetup.Modules.Users.Application.Abstractions.Authentication;
using eMeetup.Modules.Users.Application.Abstractions.Data;
using eMeetup.Modules.Users.Domain.Interfaces.Repositories;
using eMeetup.Modules.Users.Domain.Users;
using Microsoft.Extensions.Logging;

namespace eMeetup.Modules.Users.Application.Users.CompleteProfile;

internal sealed class CompleteProfileCommandHandler(
    IUserRepository userRepository,
    IUserContext userContext,
    IUnitOfWork unitOfWork,
    ILogger<CompleteProfileCommandHandler> logger)
    : ICommandHandler<CompleteProfileCommand>
{
    public async Task<Result> Handle(
        CompleteProfileCommand request,
        CancellationToken cancellationToken)
    {
        // Получаем текущего пользователя из контекста (Keycloak)
        var userId = userContext.UserId;

        using var loggerScope = logger.BeginScope(
            "CompleteProfile {UserId}",
            userId);

        try
        {
            logger.LogInformation("Starting profile completion for user {UserId}", userId);

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
            // 2. Проверяем, не заполнен ли профиль уже
            // ============================================================
            if (user.ProfileCompleted)
            {
                logger.LogWarning("Profile for user {UserId} is already completed", userId);
                return Result.Failure(ProfileErrors.ProfileAlreadyComplete);
            }

            // ============================================================
            // 3. Создаём полный профиль
            // ============================================================
            logger.LogInformation("Building full profile for user {UserId}", userId);

            var profileResult = UserProfile.Create(
                avatarUrl: request.AvatarUrl,
                dateOfBirth: request.DateOfBirth,
                bio: request.Bio,
                phone: request.Phone,
                telegram: request.Telegram,
                instagram: request.Instagram,
                city: request.City,
                country: request.Country,
                latitude: request.Latitude,
                longitude: request.Longitude,
                timeZone: request.TimeZone,
                gender: request.Gender,
                languages: request.Languages,
                interests: request.Interests);

            if (profileResult.IsFailure)
            {
                logger.LogError(
                    "Failed to build profile for user {UserId}: {Error}",
                    userId,
                    profileResult.Error);

                return Result.Failure(profileResult.Error);
            }

            // ============================================================
            // 4. Обновляем профиль пользователя
            // ============================================================
            logger.LogInformation("Updating profile for user {UserId}", userId);

            var updateResult = user.UpdateProfile(profileResult.Value);

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
                    "No rows were affected during profile save for user {UserId}",
                    userId);

                return Result.Failure(UserErrors.DatabaseSaveFailed);
            }

            // ============================================================
            // 6. Успех
            // ============================================================
            logger.LogInformation(
                "Profile for user {UserId} completed successfully. New status: {Status}",
                userId,
                user.Status);

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "An unexpected error occurred during profile completion for user {UserId}",
                userId);

            return Result.Failure(ProfileErrors.ProfileUpdateFailed(ex.Message));
        }
    }
}
