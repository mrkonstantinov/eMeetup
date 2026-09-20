using eMeetup.Common.Domain;

namespace eMeetup.Modules.Users.Domain.Users;

public enum UserStatus
{
    Active = 1,
    Inactive = 2,
    Suspended = 3,
    Deleted = 4
}



public static class StatusErrors
{
    // === ОШИБКИ КОНФЛИКТА ===

    public static Error AlreadyActive =>
        Error.Conflict("Users.Status.AlreadyActive", "User is already active");

    public static Error AlreadySuspended =>
        Error.Conflict("Users.Status.AlreadySuspended", "User is already suspended");

    public static Error AlreadyDeleted =>
        Error.Conflict("Users.Status.AlreadyDeleted", "User is already deleted");

    public static Error AlreadyInactive =>
        Error.Conflict("Users.Status.AlreadyInactive", "User is already inactive");

    public static Error AlreadyActivated =>
        Error.Conflict("Users.Status.AlreadyActivated", "User is already activated");

    // === ОШИБКИ НЕВОЗМОЖНЫХ ПЕРЕХОДОВ ===

    public static Error CannotActivateDeleted =>
        Error.Problem("Users.Status.CannotActivateDeleted", "Cannot activate deleted user");

    public static Error CannotActivateSuspended =>
        Error.Problem("Users.Status.CannotActivateSuspended", "Cannot activate suspended user");

    public static Error CannotActivateInactive =>
        Error.Problem("Users.Status.CannotActivateInactive", "Cannot activate inactive user");

    public static Error CannotActivateWithoutProfile =>
        Error.Validation("Users.Status.CannotActivateWithoutProfile",
            "Cannot activate user without completing profile");

    public static Error CannotSuspendDeleted =>
        Error.Problem("Users.Status.CannotSuspendDeleted", "Cannot suspend deleted user");

    public static Error CannotSuspendInactive =>
        Error.Problem("Users.Status.CannotSuspendInactive", "Cannot suspend inactive user");

    public static Error CannotSuspendActiveWithoutReason =>
        Error.Validation("Users.Status.CannotSuspendWithoutReason", "Reason is required for suspending active user");

    public static Error CannotSuspendSuspended =>
        Error.Conflict("Users.Status.CannotSuspendSuspended", "User is already suspended");

    public static Error CannotDeleteSuspended =>
        Error.Problem("Users.Status.CannotDeleteSuspended", "Cannot delete suspended user without unsuspending first");

    public static Error CannotDeleteActiveWithoutReason =>
        Error.Validation("Users.Status.CannotDeleteWithoutReason", "Reason is required for deleting active user");

    public static Error CannotDeleteInactive =>
        Error.Problem("Users.Status.CannotDeleteInactive", "Cannot delete inactive user");

    public static Error CannotDeleteDeleted =>
        Error.Conflict("Users.Status.CannotDeleteDeleted", "User is already deleted");

    public static Error CannotMarkDeletedAsInactive =>
        Error.Problem("Users.Status.CannotMarkDeletedInactive", "Cannot mark deleted user as inactive");

    public static Error CannotMarkSuspendedAsInactive =>
        Error.Problem("Users.Status.CannotMarkSuspendedInactive", "Cannot mark suspended user as inactive");

    public static Error CannotMarkActiveAsInactive =>
        Error.Problem("Users.Status.CannotMarkActiveInactive", "Cannot mark active user as inactive");

    public static Error CannotUnsuspendActive =>
        Error.Conflict("Users.Status.CannotUnsuspendActive", "Cannot unsuspend user that is not suspended");

    public static Error CannotUnsuspendDeleted =>
        Error.Problem("Users.Status.CannotUnsuspendDeleted", "Cannot unsuspend deleted user");

    public static Error CannotUnsuspendInactive =>
        Error.Problem("Users.Status.CannotUnsuspendInactive", "Cannot unsuspend inactive user");

    public static Error CannotUnsuspendUnsuspended =>
        Error.Conflict("Users.Status.CannotUnsuspendUnsuspended", "User is not suspended");

    public static Error CannotActivateAlreadyActivated =>
        Error.Conflict("Users.Status.CannotActivateAlreadyActivated", "User is already activated");

    public static Error CannotReactivateDeleted =>
        Error.Problem("Users.Status.CannotReactivateDeleted", "Cannot reactivate deleted user");

    public static Error CannotReactivateSuspended =>
        Error.Problem("Users.Status.CannotReactivateSuspended", "Cannot reactivate suspended user");

    public static Error CannotReactivateActive =>
        Error.Conflict("Users.Status.CannotReactivateActive", "Cannot reactivate already active user");

    public static Error CannotReactivateInactiveWithoutProfile =>
        Error.Validation("Users.Status.CannotReactivateWithoutProfile",
            "Cannot reactivate inactive user without completing profile");

    // === ОШИБКИ ВАЛИДАЦИИ ===

    public static Error SuspensionReasonRequired =>
        Error.Validation("Users.Status.SuspensionReasonRequired", "Suspension reason is required");

    public static Error InvalidStatusTransition(string from, string to) =>
        Error.Validation("Users.Status.InvalidTransition", $"Cannot transition from '{from}' to '{to}'");

    public static Error InvalidStatus =>
        Error.Validation("Users.Status.InvalidStatus", "Invalid user status");

    public static Error StatusReasonTooLong(int maxLength) =>
        Error.Validation("Users.Status.ReasonTooLong", $"Status reason cannot exceed {maxLength} characters");

    public static Error InvalidStatusValue =>
        Error.Validation("Users.Status.InvalidStatusValue", "Invalid status value");

    public static Error StatusReasonRequired =>
        Error.Validation("Users.Status.ReasonRequired", "Status reason is required for this operation");

    public static Error InvalidStatusReason =>
        Error.Validation("Users.Status.InvalidReason", "Invalid status reason");

    public static Error StatusChangeNotAllowed =>
        Error.Validation("Users.Status.ChangeNotAllowed", "Status change is not allowed");

    // === ОШИБКИ ОПЕРАЦИЙ ===

    public static Error StatusChangeFailed(string reason) =>
        Error.Failure("Users.Status.ChangeFailed", $"Status change failed: {reason}");

    public static Error StatusUpdateFailed(string reason) =>
        Error.Failure("Users.Status.UpdateFailed", $"Status update failed: {reason}");

    public static Error StatusSaveFailed(string reason) =>
        Error.Failure("Users.Status.SaveFailed", $"Failed to save status: {reason}");

    public static Error InvalidOperation(string reason) =>
        Error.Problem("Users.Status.InvalidOperation", reason);

    public static Error DataIntegrityViolation(string details) =>
        Error.Problem("Users.Status.DataIntegrityViolation", $"Data integrity violation: {details}");

    // === ОШИБКИ ПРАВ ДОСТУПА ===

    public static Error CannotChangeOwnStatus =>
        Error.Forbidden("Users.Status.CannotChangeOwn", "Cannot change your own status");

    public static Error CannotChangeOtherUserStatus =>
        Error.Forbidden("Users.Status.CannotChangeOther", "Cannot change other user's status");

    public static Error CannotChangeStatusOfAdmin =>
        Error.Forbidden("Users.Status.CannotChangeAdmin", "Cannot change status of administrator");

    public static Error CannotChangeStatusOfModerator =>
        Error.Forbidden("Users.Status.CannotChangeModerator", "Cannot change status of moderator");

    public static Error CannotChangeStatusOfSystemUser =>
        Error.Forbidden("Users.Status.CannotChangeSystem", "Cannot change status of system user");

    public static Error InsufficientPermissionsToChangeStatus =>
        Error.Forbidden("Users.Status.InsufficientPermissions", "Insufficient permissions to change user status");

    public static Error CannotViewOtherUserStatus =>
        Error.Forbidden("Users.Status.CannotViewOther", "Cannot view other user's status");

    public static Error CannotViewStatusHistory =>
        Error.Forbidden("Users.Status.CannotViewHistory", "Cannot view status history");

    // === ОШИБКИ ОГРАНИЧЕНИЙ ===

    public static Error StatusChangeLimitExceeded =>
        Error.Problem("Users.Status.ChangeLimitExceeded", "Status change limit exceeded. Please try again later");

    public static Error DailyStatusChangeLimitExceeded(int limit) =>
        Error.Problem("Users.Status.DailyLimitExceeded", $"Daily status change limit of {limit} exceeded");

    public static Error RateLimitExceeded =>
        Error.Problem("Users.Status.RateLimitExceeded", "Too many status operations. Please try again later");

    public static Error MaxStatusChangesExceeded =>
        Error.Problem("Users.Status.MaxChangesExceeded", "Maximum number of status changes exceeded");

    // === ОШИБКИ БИЗНЕС-ЛОГИКИ ===

    public static Error CannotSuspendActiveUserWithActiveEvents =>
        Error.Problem("Users.Status.CannotSuspendWithActiveEvents",
            "Cannot suspend user with active events. Please cancel events first");

    public static Error CannotSuspendActiveUserWithActiveBookings =>
        Error.Problem("Users.Status.CannotSuspendWithActiveBookings",
            "Cannot suspend user with active bookings. Please cancel bookings first");

    public static Error CannotDeleteUserWithActiveEvents =>
        Error.Problem("Users.Status.CannotDeleteWithActiveEvents",
            "Cannot delete user with active events. Please cancel events first");

    public static Error CannotDeleteUserWithActiveBookings =>
        Error.Problem("Users.Status.CannotDeleteWithActiveBookings",
            "Cannot delete user with active bookings. Please cancel bookings first");

    public static Error CannotSuspendUserWithPendingReports =>
        Error.Problem("Users.Status.CannotSuspendWithPendingReports",
            "Cannot suspend user with pending reports. Please review reports first");

    public static Error CannotActivateSuspendedUserWithoutAppeal =>
        Error.Validation("Users.Status.CannotActivateWithoutAppeal",
            "Cannot activate suspended user without appeal process");

    public static Error CannotMarkUserInactiveWithoutReason =>
        Error.Validation("Users.Status.CannotMarkInactiveWithoutReason",
            "Reason is required for marking user inactive");

    public static Error CannotMarkUserInactiveWithActiveEvents =>
        Error.Problem("Users.Status.CannotMarkInactiveWithActiveEvents",
            "Cannot mark user inactive with active events");

    public static Error CannotMarkUserInactiveWithActiveBookings =>
        Error.Problem("Users.Status.CannotMarkInactiveWithActiveBookings",
            "Cannot mark user inactive with active bookings");

    public static Error UserAlreadyActivatedBySystem =>
        Error.Conflict("Users.Status.AlreadyActivatedBySystem", "User was already activated by system");

    public static Error UserAutoSuspendedBySystem =>
        Error.Problem("Users.Status.AutoSuspendedBySystem", "User was automatically suspended by system");

    public static Error UserAutoDeletedBySystem =>
        Error.Problem("Users.Status.AutoDeletedBySystem", "User was automatically deleted by system");

    // === ОШИБКИ СИНХРОНИЗАЦИИ С KEYCLOAK ===

    public static Error KeycloakStatusMismatch =>
        Error.Conflict("Users.Status.KeycloakMismatch", "User status in Keycloak does not match local status");

    public static Error KeycloakSyncFailed(string reason) =>
        Error.Failure("Users.Status.KeycloakSyncFailed", $"Failed to sync status with Keycloak: {reason}");

    public static Error KeycloakStatusUpdateFailed(string reason) =>
        Error.Failure("Users.Status.KeycloakUpdateFailed", $"Failed to update status in Keycloak: {reason}");

    public static Error KeycloakConnectionFailed =>
        Error.Failure("Users.Status.KeycloakConnectionFailed", "Failed to connect to Keycloak");

    public static Error KeycloakTimeout =>
        Error.Failure("Users.Status.KeycloakTimeout", "Keycloak operation timeout");

    // === ОШИБКИ ИСТОРИИ СТАТУСОВ ===

    public static Error StatusHistoryNotFound =>
        Error.NotFound("Users.Status.HistoryNotFound", "Status history not found");

    public static Error StatusHistoryNotFoundForUser(Guid userId) =>
        Error.NotFound("Users.Status.HistoryNotFoundForUser", $"Status history not found for user '{userId}'");

    public static Error StatusHistoryCorrupted =>
        Error.Problem("Users.Status.HistoryCorrupted", "Status history data is corrupted");

    public static Error CannotAddDuplicateStatusHistory =>
        Error.Conflict("Users.Status.DuplicateHistory", "Duplicate status history entry detected");

    public static Error StatusHistoryLimitExceeded =>
        Error.Problem("Users.Status.HistoryLimitExceeded", "Status history limit exceeded");

    // === ОШИБКИ УВЕДОМЛЕНИЙ ===

    public static Error StatusChangeNotificationFailed(string reason) =>
        Error.Failure("Users.Status.NotificationFailed", $"Failed to send status change notification: {reason}");

    public static Error SuspensionNotificationFailed(string reason) =>
        Error.Failure("Users.Status.SuspensionNotificationFailed", $"Failed to send suspension notification: {reason}");

    public static Error ActivationNotificationFailed(string reason) =>
        Error.Failure("Users.Status.ActivationNotificationFailed", $"Failed to send activation notification: {reason}");

    public static Error DeletionNotificationFailed(string reason) =>
        Error.Failure("Users.Status.DeletionNotificationFailed", $"Failed to send deletion notification: {reason}");
}
