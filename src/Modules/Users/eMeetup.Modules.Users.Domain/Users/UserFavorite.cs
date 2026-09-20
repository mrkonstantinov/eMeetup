using eMeetup.Common.Domain;

namespace eMeetup.Modules.Users.Domain.Users;

public class UserFavorite : Entity
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid TargetUserId { get; private set; }
    public DateTime AddedAt { get; private set; }
    public DateTime? RemovedAt { get; private set; }
    public bool IsActive { get; private set; }

    // Navigation
    public virtual User User { get; private set; }
    public virtual User TargetUser { get; private set; }

    private UserFavorite() { } // EF Core

    private UserFavorite(Guid userId, Guid targetUserId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        TargetUserId = targetUserId;
        AddedAt = DateTime.UtcNow;
        IsActive = true;
    }

    public static Result<UserFavorite> Create(Guid userId, Guid targetUserId)
    {
        if (userId == Guid.Empty)
            return Result.Failure<UserFavorite>(FavoriteErrors.InvalidUserId);

        if (targetUserId == Guid.Empty)
            return Result.Failure<UserFavorite>(FavoriteErrors.InvalidTargetUserId);

        if (userId == targetUserId)
            return Result.Failure<UserFavorite>(FavoriteErrors.CannotAddYourself);

        var favorite = new UserFavorite(userId, targetUserId);
        return Result<UserFavorite>.Success(favorite);
    }

    public Result Remove()
    {
        if (!IsActive)
            return Result.Failure(FavoriteErrors.CannotRemoveInactiveFavorite);

        IsActive = false;
        RemovedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result Reactivate()
    {
        if (IsActive)
            return Result.Failure(FavoriteErrors.FavoriteAlreadyActive);

        if (!RemovedAt.HasValue)
            return Result.Failure(FavoriteErrors.CannotReactivateRemovedFavorite);

        IsActive = true;
        RemovedAt = null;
        return Result.Success();
    }

    public bool IsRemoved() => !IsActive && RemovedAt.HasValue;
    public bool IsActiveFavorite() => IsActive;
}



public static class FavoriteErrors
{
    // === ОШИБКИ ВАЛИДАЦИИ ===

    public static Error InvalidUserId =>
        Error.Validation("Users.Favorite.InvalidUserId", "Invalid user ID format");

    public static Error InvalidTargetUserId =>
        Error.Validation("Users.Favorite.InvalidTargetUserId", "Invalid target user ID format");

    public static Error CannotAddYourself =>
        Error.Validation("Users.Favorite.CannotAddYourself", "Cannot add yourself to favorites");

    public static Error FavoriteIdRequired =>
        Error.Validation("Users.Favorite.IdRequired", "Favorite ID is required");

    public static Error InvalidFavoriteId =>
        Error.Validation("Users.Favorite.InvalidFavoriteId", "Invalid favorite ID format");

    // === ОШИБКИ НЕ НАЙДЕНО ===

    public static Error NotFound(Guid favoriteId) =>
        Error.NotFound("Users.Favorite.NotFound", $"Favorite with ID '{favoriteId}' was not found");

    public static Error NotFound(Guid userId, Guid targetUserId) =>
        Error.NotFound("Users.Favorite.NotFound",
            $"Favorite relationship between user '{userId}' and target '{targetUserId}' was not found");

    public static Error TargetNotFound(Guid targetUserId) =>
        Error.NotFound("Users.Favorite.TargetNotFound", $"Target user with ID '{targetUserId}' was not found");

    public static Error TargetNotFound(string targetUsername) =>
        Error.NotFound("Users.Favorite.TargetNotFound", $"Target user with username '{targetUsername}' was not found");

    public static Error FavoritesNotFoundForUser(Guid userId) =>
        Error.NotFound("Users.Favorite.FavoritesNotFound", $"No favorites found for user '{userId}'");

    public static Error FavoritesNotFoundForTarget(Guid targetUserId) =>
        Error.NotFound("Users.Favorite.FavoritesNotFoundForTarget",
            $"No users have added user '{targetUserId}' to favorites");

    public static Error UserNotFound(Guid userId) =>
        Error.NotFound("Users.Favorite.UserNotFound", $"User with ID '{userId}' was not found");

    // === ОШИБКИ КОНФЛИКТА ===

    public static Error AlreadyExists(Guid targetUserId) =>
        Error.Conflict("Users.Favorite.AlreadyExists", $"User with ID '{targetUserId}' is already in favorites");

    public static Error AlreadyExists(string targetUsername) =>
        Error.Conflict("Users.Favorite.AlreadyExists", $"User '{targetUsername}' is already in favorites");

    public static Error AlreadyExists(Guid userId, Guid targetUserId) =>
        Error.Conflict("Users.Favorite.AlreadyExists",
            $"User '{userId}' already has user '{targetUserId}' in favorites");

    public static Error FavoriteAlreadyRemoved =>
        Error.Conflict("Users.Favorite.AlreadyRemoved", "This favorite has already been removed");

    public static Error FavoriteAlreadyActive =>
        Error.Conflict("Users.Favorite.AlreadyActive", "This favorite is already active");

    public static Error DuplicateFavorite =>
        Error.Conflict("Users.Favorite.Duplicate", "Duplicate favorite detected");

    // === ОШИБКИ СТАТУСА ===

    public static Error CannotAddInactiveUser =>
        Error.Problem("Users.Favorite.CannotAddInactive", "Cannot add favorites while account is inactive");

    public static Error CannotAddDeletedUser =>
        Error.Problem("Users.Favorite.CannotAddDeleted", "Cannot add favorites while account is deleted");

    public static Error CannotAddSuspendedUser =>
        Error.Problem("Users.Favorite.CannotAddSuspended", "Cannot add favorites while account is suspended");

    public static Error TargetInactive =>
        Error.Problem("Users.Favorite.TargetInactive", "Cannot add inactive user to favorites");

    public static Error TargetDeleted =>
        Error.Problem("Users.Favorite.TargetDeleted", "Cannot add deleted user to favorites");

    public static Error TargetSuspended =>
        Error.Problem("Users.Favorite.TargetSuspended", "Cannot add suspended user to favorites");

    public static Error TargetNotFoundInSystem =>
        Error.Problem("Users.Favorite.TargetNotFoundInSystem", "Target user not found in system");

    public static Error CannotRemoveInactiveFavorite =>
        Error.Problem("Users.Favorite.CannotRemoveInactive", "Cannot remove an inactive favorite");

    public static Error CannotReactivateRemovedFavorite =>
        Error.Problem("Users.Favorite.CannotReactivateRemoved", "Cannot reactivate a removed favorite");

    public static Error CannotReactivateActiveFavorite =>
        Error.Problem("Users.Favorite.CannotReactivateActive", "Cannot reactivate an already active favorite");

    public static Error FavoriteIsInactive =>
        Error.Problem("Users.Favorite.IsInactive", "This favorite is inactive");

    // === ОШИБКИ ОГРАНИЧЕНИЙ ===

    public static Error TooManyFavorites(int maxCount) =>
        Error.Validation("Users.Favorite.TooMany", $"Maximum {maxCount} favorites allowed per user");

    public static Error MaxFavoritesReached =>
        Error.Validation("Users.Favorite.MaxFavoritesReached", "Maximum number of favorites reached");

    public static Error DailyLimitExceeded(int limit) =>
        Error.Problem("Users.Favorite.DailyLimitExceeded", $"Daily limit of {limit} favorites exceeded");

    public static Error RateLimitExceeded =>
        Error.Problem("Users.Favorite.RateLimitExceeded", "Too many favorite operations. Please try again later");

    public static Error FavoriteLimitReached =>
        Error.Validation("Users.Favorite.LimitReached", "Favorite limit reached");

    // === ОШИБКИ ПРАВ ДОСТУПА ===

    public static Error CannotViewOtherFavorites =>
        Error.Forbidden("Users.Favorite.CannotViewOther", "You don't have permission to view this user's favorites");

    public static Error CannotModifyOtherFavorites =>
        Error.Forbidden("Users.Favorite.CannotModifyOther", "You don't have permission to modify this user's favorites");

    public static Error CannotViewDeletedUserFavorites =>
        Error.Forbidden("Users.Favorite.CannotViewDeleted", "Cannot view favorites of deleted user");

    public static Error CannotViewSuspendedUserFavorites =>
        Error.Forbidden("Users.Favorite.CannotViewSuspended", "Cannot view favorites of suspended user");

    public static Error CannotViewPrivateFavorites =>
        Error.Forbidden("Users.Favorite.CannotViewPrivate", "This user's favorites are private");

    public static Error CannotAddFavoriteToBlockedUser =>
        Error.Forbidden("Users.Favorite.CannotAddBlocked", "Cannot add blocked user to favorites");

    // === ОШИБКИ ОПЕРАЦИЙ ===

    public static Error AddFailed(string reason) =>
        Error.Failure("Users.Favorite.AddFailed", $"Failed to add favorite: {reason}");

    public static Error RemoveFailed(string reason) =>
        Error.Failure("Users.Favorite.RemoveFailed", $"Failed to remove favorite: {reason}");

    public static Error ReactivateFailed(string reason) =>
        Error.Failure("Users.Favorite.ReactivateFailed", $"Failed to reactivate favorite: {reason}");

    public static Error GetFavoritesFailed(string reason) =>
        Error.Failure("Users.Favorite.GetFavoritesFailed", $"Failed to retrieve favorites: {reason}");

    public static Error InvalidOperation(string reason) =>
        Error.Problem("Users.Favorite.InvalidOperation", reason);

    public static Error DataIntegrityViolation(string details) =>
        Error.Problem("Users.Favorite.DataIntegrityViolation", $"Data integrity violation: {details}");

    public static Error SaveFailed(string reason) =>
        Error.Failure("Users.Favorite.SaveFailed", $"Failed to save favorite: {reason}");

    public static Error UpdateFailed(string reason) =>
        Error.Failure("Users.Favorite.UpdateFailed", $"Failed to update favorite: {reason}");

    public static Error DeleteFailed(string reason) =>
        Error.Failure("Users.Favorite.DeleteFailed", $"Failed to delete favorite: {reason}");

    // === ОШИБКИ СИНХРОНИЗАЦИИ ===

    public static Error TargetUserNotExists =>
        Error.Problem("Users.Favorite.TargetUserNotExists", "Target user does not exist");

    public static Error FavoriteOutOfSync =>
        Error.Problem("Users.Favorite.OutOfSync", "Favorite data is out of sync");

    public static Error ConcurrentModification =>
        Error.Conflict("Users.Favorite.ConcurrentModification", "Favorite was modified by another user");

    // === ОШИБКИ БИЗНЕС-ЛОГИКИ ===

    public static Error CannotFavoriteYourself =>
        Error.Validation("Users.Favorite.CannotFavoriteYourself", "You cannot add yourself to favorites");

    public static Error CannotFavoriteDeletedUser =>
        Error.Problem("Users.Favorite.CannotFavoriteDeleted", "Cannot add deleted user to favorites");

    public static Error CannotFavoriteInactiveUser =>
        Error.Problem("Users.Favorite.CannotFavoriteInactive", "Cannot add inactive user to favorites");

    public static Error CannotFavoriteSuspendedUser =>
        Error.Problem("Users.Favorite.CannotFavoriteSuspended", "Cannot add suspended user to favorites");

    public static Error MutualFavoritesRequired =>
        Error.Problem("Users.Favorite.MutualRequired", "Mutual favorites required for this action");

    public static Error FavoriteAlreadyExists =>
        Error.Conflict("Users.Favorite.AlreadyExists", "This user is already in your favorites");

    public static Error FavoriteNotFoundForUser =>
        Error.NotFound("Users.Favorite.NotFoundForUser", "Favorite not found for this user");
}
