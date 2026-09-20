using eMeetup.Common.Domain;

namespace eMeetup.Modules.Users.Domain.Users;

public class UserSubscription : Entity
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid TargetUserId { get; private set; }
    public SubscriptionType Type { get; private set; }
    public DateTime SubscribedAt { get; private set; }
    public DateTime? UnsubscribedAt { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? LastNotificationSentAt { get; private set; }

    // Navigation
    public virtual User User { get; private set; }
    public virtual User TargetUser { get; private set; }

    private UserSubscription() { } // EF Core

    private UserSubscription(Guid userId, Guid targetUserId, SubscriptionType type)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        TargetUserId = targetUserId;
        Type = type;
        SubscribedAt = DateTime.UtcNow;
        IsActive = true;
    }

    public static Result<UserSubscription> Create(Guid userId, Guid targetUserId, SubscriptionType type)
    {
        if (userId == Guid.Empty)
            return Result.Failure<UserSubscription>(SubscriptionErrors.InvalidUserId);

        if (targetUserId == Guid.Empty)
            return Result.Failure<UserSubscription>(SubscriptionErrors.InvalidTargetUserId);

        if (userId == targetUserId)
            return Result.Failure<UserSubscription>(SubscriptionErrors.CannotSubscribeYourself);

        if (!Enum.IsDefined(typeof(SubscriptionType), type))
            return Result.Failure<UserSubscription>(SubscriptionErrors.InvalidSubscriptionType);

        var subscription = new UserSubscription(userId, targetUserId, type);
        return Result.Success(subscription);
    }

    public Result Unsubscribe()
    {
        if (!IsActive)
            return Result.Failure(SubscriptionErrors.SubscriptionIsInactive);

        IsActive = false;
        UnsubscribedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result Reactivate()
    {
        if (IsActive)
            return Result.Failure(SubscriptionErrors.SubscriptionAlreadyActive);

        if (!UnsubscribedAt.HasValue)
            return Result.Failure(SubscriptionErrors.CannotReactivateCancelledSubscription);

        IsActive = true;
        UnsubscribedAt = null;
        return Result.Success();
    }

    public void UpdateLastNotificationSent()
    {
        LastNotificationSentAt = DateTime.UtcNow;
    }

    public bool ShouldSendNotification(DateTime lastActivity)
    {
        if (!IsActive)
            return false;

        if (!LastNotificationSentAt.HasValue)
            return true;

        // Не отправлять уведомления чаще чем раз в час
        return (DateTime.UtcNow - LastNotificationSentAt.Value).TotalHours >= 1;
    }

    public bool IsCancelled() => !IsActive && UnsubscribedAt.HasValue;
    public bool IsActiveSubscription() => IsActive;
    public bool IsExpired() => false; // Для будущей реализации с expiration
}

public static class SubscriptionErrors
{
    // === ОШИБКИ ВАЛИДАЦИИ ===

    public static Error InvalidUserId =>
        Error.Validation("Users.Subscription.InvalidUserId", "Invalid user ID format");

    public static Error InvalidTargetUserId =>
        Error.Validation("Users.Subscription.InvalidTargetUserId", "Invalid target user ID format");

    public static Error InvalidSubscriptionType =>
        Error.Validation("Users.Subscription.InvalidType", "Invalid subscription type");

    public static Error CannotSubscribeYourself =>
        Error.Validation("Users.Subscription.CannotSubscribeYourself", "Cannot subscribe to yourself");

    public static Error SubscriptionIdRequired =>
        Error.Validation("Users.Subscription.IdRequired", "Subscription ID is required");

    public static Error InvalidSubscriptionId =>
        Error.Validation("Users.Subscription.InvalidSubscriptionId", "Invalid subscription ID format");

    // === ОШИБКИ НЕ НАЙДЕНО ===

    public static Error NotFound(Guid subscriptionId) =>
        Error.NotFound("Users.Subscription.NotFound", $"Subscription with ID '{subscriptionId}' was not found");

    public static Error NotFound(Guid userId, Guid targetUserId, SubscriptionType type) =>
        Error.NotFound("Users.Subscription.NotFound",
            $"Subscription from user '{userId}' to '{targetUserId}' with type '{type}' was not found");

    public static Error TargetNotFound(Guid targetUserId) =>
        Error.NotFound("Users.Subscription.TargetNotFound", $"Target user with ID '{targetUserId}' was not found");

    public static Error TargetNotFound(string targetUsername) =>
        Error.NotFound("Users.Subscription.TargetNotFound", $"Target user with username '{targetUsername}' was not found");

    public static Error SubscriptionsNotFoundForUser(Guid userId) =>
        Error.NotFound("Users.Subscription.NotFoundForUser", $"No subscriptions found for user '{userId}'");

    public static Error SubscriptionsNotFoundForTarget(Guid targetUserId) =>
        Error.NotFound("Users.Subscription.NotFoundForTarget",
            $"No subscriptions found for target user '{targetUserId}'");

    public static Error SubscriptionNotFoundForType(Guid userId, SubscriptionType type) =>
        Error.NotFound("Users.Subscription.NotFoundForType",
            $"No subscription of type '{type}' found for user '{userId}'");

    public static Error UserNotFound(Guid userId) =>
        Error.NotFound("Users.Subscription.UserNotFound", $"User with ID '{userId}' was not found");

    // === ОШИБКИ КОНФЛИКТА ===

    public static Error AlreadyExists(Guid targetUserId, SubscriptionType type) =>
        Error.Conflict("Users.Subscription.AlreadyExists",
            $"Already subscribed to user '{targetUserId}' with type '{type}'");

    public static Error AlreadyExists(string targetUsername, SubscriptionType type) =>
        Error.Conflict("Users.Subscription.AlreadyExists",
            $"Already subscribed to user '{targetUsername}' with type '{type}'");

    public static Error AlreadyExists(Guid userId, Guid targetUserId, SubscriptionType type) =>
        Error.Conflict("Users.Subscription.AlreadyExists",
            $"User '{userId}' already subscribed to '{targetUserId}' with type '{type}'");

    public static Error SubscriptionAlreadyCancelled =>
        Error.Conflict("Users.Subscription.AlreadyCancelled", "This subscription has already been cancelled");

    public static Error SubscriptionAlreadyActive =>
        Error.Conflict("Users.Subscription.AlreadyActive", "This subscription is already active");

    public static Error DuplicateSubscription =>
        Error.Conflict("Users.Subscription.Duplicate", "Duplicate subscription detected");

    public static Error CannotSubscribeToSelf =>
        Error.Conflict("Users.Subscription.CannotSubscribeToSelf", "You cannot subscribe to yourself");

    // === ОШИБКИ СТАТУСА ===

    public static Error CannotSubscribeInactiveUser =>
        Error.Problem("Users.Subscription.CannotSubscribeInactive", "Cannot subscribe while account is inactive");

    public static Error CannotSubscribeDeletedUser =>
        Error.Problem("Users.Subscription.CannotSubscribeDeleted", "Cannot subscribe while account is deleted");

    public static Error CannotSubscribeSuspendedUser =>
        Error.Problem("Users.Subscription.CannotSubscribeSuspended", "Cannot subscribe while account is suspended");

    public static Error TargetInactive =>
        Error.Problem("Users.Subscription.TargetInactive", "Cannot subscribe to inactive user");

    public static Error TargetDeleted =>
        Error.Problem("Users.Subscription.TargetDeleted", "Cannot subscribe to deleted user");

    public static Error TargetSuspended =>
        Error.Problem("Users.Subscription.TargetSuspended", "Cannot subscribe to suspended user");

    public static Error TargetNotFoundInSystem =>
        Error.Problem("Users.Subscription.TargetNotFoundInSystem", "Target user not found in system");

    public static Error CannotUnsubscribeInactiveSubscription =>
        Error.Problem("Users.Subscription.CannotUnsubscribeInactive", "Cannot unsubscribe from an inactive subscription");

    public static Error CannotReactivateCancelledSubscription =>
        Error.Problem("Users.Subscription.CannotReactivateCancelled", "Cannot reactivate a cancelled subscription");

    public static Error CannotReactivateActiveSubscription =>
        Error.Problem("Users.Subscription.CannotReactivateActive", "Cannot reactivate an already active subscription");

    public static Error SubscriptionIsInactive =>
        Error.Problem("Users.Subscription.IsInactive", "This subscription is inactive");

    public static Error SubscriptionIsCancelled =>
        Error.Problem("Users.Subscription.IsCancelled", "This subscription has been cancelled");

    // === ОШИБКИ ОГРАНИЧЕНИЙ ===

    public static Error TooManySubscriptions(int maxCount) =>
        Error.Validation("Users.Subscription.TooMany", $"Maximum {maxCount} subscriptions allowed per user");

    public static Error MaxSubscriptionsReached =>
        Error.Validation("Users.Subscription.MaxReached", "Maximum number of subscriptions reached");

    public static Error DailyLimitExceeded(int limit) =>
        Error.Problem("Users.Subscription.DailyLimitExceeded", $"Daily limit of {limit} subscriptions exceeded");

    public static Error RateLimitExceeded =>
        Error.Problem("Users.Subscription.RateLimitExceeded", "Too many subscription operations. Please try again later");

    public static Error SubscriptionLimitReached =>
        Error.Validation("Users.Subscription.LimitReached", "Subscription limit reached");

    public static Error TooManySubscriptionsToUser(int maxCount) =>
        Error.Validation("Users.Subscription.TooManyToUser", $"Maximum {maxCount} subscriptions per user allowed");

    // === ОШИБКИ ПРАВ ДОСТУПА ===

    public static Error CannotViewOtherSubscriptions =>
        Error.Forbidden("Users.Subscription.CannotViewOther", "You don't have permission to view this user's subscriptions");

    public static Error CannotModifyOtherSubscriptions =>
        Error.Forbidden("Users.Subscription.CannotModifyOther", "You don't have permission to modify this user's subscriptions");

    public static Error CannotViewDeletedUserSubscriptions =>
        Error.Forbidden("Users.Subscription.CannotViewDeleted", "Cannot view subscriptions of deleted user");

    public static Error CannotViewSuspendedUserSubscriptions =>
        Error.Forbidden("Users.Subscription.CannotViewSuspended", "Cannot view subscriptions of suspended user");

    public static Error CannotViewPrivateSubscriptions =>
        Error.Forbidden("Users.Subscription.CannotViewPrivate", "This user's subscriptions are private");

    public static Error CannotSubscribeToBlockedUser =>
        Error.Forbidden("Users.Subscription.CannotSubscribeBlocked", "Cannot subscribe to blocked user");

    public static Error CannotSubscribeToUserWhoBlockedYou =>
        Error.Forbidden("Users.Subscription.CannotSubscribeBlockedBy", "Cannot subscribe to user who blocked you");

    // === ОШИБКИ ОПЕРАЦИЙ ===

    public static Error SubscribeFailed(string reason) =>
        Error.Failure("Users.Subscription.SubscribeFailed", $"Failed to subscribe: {reason}");

    public static Error UnsubscribeFailed(string reason) =>
        Error.Failure("Users.Subscription.UnsubscribeFailed", $"Failed to unsubscribe: {reason}");

    public static Error ReactivateFailed(string reason) =>
        Error.Failure("Users.Subscription.ReactivateFailed", $"Failed to reactivate subscription: {reason}");

    public static Error GetSubscriptionsFailed(string reason) =>
        Error.Failure("Users.Subscription.GetFailed", $"Failed to retrieve subscriptions: {reason}");

    public static Error InvalidOperation(string reason) =>
        Error.Problem("Users.Subscription.InvalidOperation", reason);

    public static Error DataIntegrityViolation(string details) =>
        Error.Problem("Users.Subscription.DataIntegrityViolation", $"Data integrity violation: {details}");

    public static Error SaveFailed(string reason) =>
        Error.Failure("Users.Subscription.SaveFailed", $"Failed to save subscription: {reason}");

    public static Error UpdateFailed(string reason) =>
        Error.Failure("Users.Subscription.UpdateFailed", $"Failed to update subscription: {reason}");

    public static Error DeleteFailed(string reason) =>
        Error.Failure("Users.Subscription.DeleteFailed", $"Failed to delete subscription: {reason}");

    // === ОШИБКИ УВЕДОМЛЕНИЙ ===

    public static Error NotificationFailed(string reason) =>
        Error.Failure("Users.Subscription.NotificationFailed", $"Failed to send notification: {reason}");

    public static Error NotificationPreferenceNotFound =>
        Error.NotFound("Users.Subscription.NotificationPreferenceNotFound", "Notification preference not found");

    public static Error CannotSendNotificationToInactiveUser =>
        Error.Problem("Users.Subscription.CannotSendNotification", "Cannot send notification to inactive user");

    public static Error NotificationDisabled =>
        Error.Problem("Users.Subscription.NotificationDisabled", "Notifications are disabled for this subscription");

    public static Error NotificationTypeNotSupported =>
        Error.Validation("Users.Subscription.NotificationTypeNotSupported", "Notification type is not supported");

    // === ОШИБКИ СИНХРОНИЗАЦИИ ===

    public static Error TargetUserNotExists =>
        Error.Problem("Users.Subscription.TargetUserNotExists", "Target user does not exist");

    public static Error SubscriptionOutOfSync =>
        Error.Problem("Users.Subscription.OutOfSync", "Subscription data is out of sync");

    public static Error ConcurrentModification =>
        Error.Conflict("Users.Subscription.ConcurrentModification", "Subscription was modified by another user");

    // === ОШИБКИ БИЗНЕС-ЛОГИКИ ===

    public static Error CannotSubscribeToDeletedUser =>
        Error.Problem("Users.Subscription.CannotSubscribeDeletedUser", "Cannot subscribe to deleted user");

    public static Error CannotSubscribeToInactiveUser =>
        Error.Problem("Users.Subscription.CannotSubscribeInactiveUser", "Cannot subscribe to inactive user");

    public static Error CannotSubscribeToSuspendedUser =>
        Error.Problem("Users.Subscription.CannotSubscribeSuspendedUser", "Cannot subscribe to suspended user");

    public static Error InvalidSubscriptionPeriod =>
        Error.Validation("Users.Subscription.InvalidPeriod", "Invalid subscription period");

    public static Error SubscriptionExpired =>
        Error.Problem("Users.Subscription.Expired", "Subscription has expired");

    public static Error CannotRenewExpiredSubscription =>
        Error.Problem("Users.Subscription.CannotRenewExpired", "Cannot renew expired subscription");

    public static Error SubscriptionAlreadyExpired =>
        Error.Conflict("Users.Subscription.AlreadyExpired", "Subscription has already expired");
}
