using eMeetup.Common.Domain;

namespace eMeetup.Modules.Users.Domain.Tags;

public static class TagErrors
{
    // === ОШИБКИ ВАЛИДАЦИИ ===

    public static Error InvalidTagName =>
        Error.Validation("Users.Tag.InvalidName", "Tag name cannot be empty");

    public static Error TagNameTooShort(int minLength) =>
        Error.Validation("Users.Tag.NameTooShort", $"Tag name must be at least {minLength} characters");

    public static Error TagNameTooLong(int maxLength) =>
        Error.Validation("Users.Tag.NameTooLong", $"Tag name cannot exceed {maxLength} characters");

    public static Error DescriptionTooLong =>
        Error.Validation("Users.Tag.DescriptionTooLong", "Description cannot exceed 200 characters");

    public static Error IconTooLong =>
        Error.Validation("Users.Tag.IconTooLong", "Icon cannot exceed 50 characters");

    public static Error InvalidColor =>
        Error.Validation("Users.Tag.InvalidColor", "Invalid color format. Use HEX color (e.g., #FF6B6B)");

    public static Error InvalidDisplayOrder =>
        Error.Validation("Users.Tag.InvalidDisplayOrder", "Display order must be greater than or equal to 0");

    public static Error InvalidCharacters =>
        Error.Validation("Users.Tag.InvalidCharacters", "Tag name contains invalid characters. Only letters, numbers, spaces and hyphens are allowed");

    public static Error InvalidCategory =>
        Error.Validation("Users.Tag.InvalidCategory", "Invalid tag category");

    public static Error InvalidGroupId =>
        Error.Validation("Users.Tag.InvalidGroupId", "Invalid group ID");

    // === ОШИБКИ НЕ НАЙДЕНО ===

    public static Error NotFound(Guid tagId) =>
        Error.NotFound("Users.Tag.NotFound", $"Tag with ID '{tagId}' was not found");

    public static Error NotFoundByName(string name) =>
        Error.NotFound("Users.Tag.NotFoundByName", $"Tag with name '{name}' was not found");

    public static Error TagNotFoundForUser(Guid tagId, Guid userId) =>
        Error.NotFound("Users.Tag.NotFoundForUser", $"Tag with ID '{tagId}' was not found for user '{userId}'");

    public static Error TagNotFoundForUser(string tagName, Guid userId) =>
        Error.NotFound("Users.Tag.NotFoundForUserByName", $"Tag '{tagName}' was not found for user '{userId}'");

    // === ОШИБКИ КОНФЛИКТА ===

    public static Error AlreadyExists(string tagName) =>
        Error.Conflict("Users.Tag.AlreadyExists", $"Tag '{tagName}' already exists for this user");

    public static Error AlreadyExists(Guid tagId) =>
        Error.Conflict("Users.Tag.AlreadyExists", $"Tag with ID '{tagId}' already exists for this user");

    public static Error TagNameAlreadyExists(string tagName) =>
        Error.Conflict("Users.Tag.NameAlreadyExists", $"Tag name '{tagName}' is already taken");

    public static Error DuplicateTag =>
        Error.Conflict("Users.Tag.Duplicate", "Duplicate tag detected");

    // === ОШИБКИ СТАТУСА ===

    public static Error TagNotActive =>
        Error.Validation("Users.Tag.NotActive", "Tag is not active");

    public static Error TagAlreadyActive =>
        Error.Conflict("Users.Tag.AlreadyActive", "Tag is already active");

    public static Error TagAlreadyInactive =>
        Error.Conflict("Users.Tag.AlreadyInactive", "Tag is already inactive");

    public static Error CannotAddTagToInactiveUser =>
        Error.Problem("Users.Tag.CannotAddInactive", "Cannot add tags to inactive user");

    public static Error CannotAddTagToDeletedUser =>
        Error.Problem("Users.Tag.CannotAddDeleted", "Cannot add tags to deleted user");

    public static Error CannotAddTagToSuspendedUser =>
        Error.Problem("Users.Tag.CannotAddSuspended", "Cannot add tags to suspended user");

    public static Error CannotRemoveTagFromDeletedUser =>
        Error.Problem("Users.Tag.CannotRemoveDeleted", "Cannot remove tags from deleted user");

    // === ОШИБКИ ОГРАНИЧЕНИЙ ===

    public static Error TooManyTags(int maxCount) =>
        Error.Validation("Users.Tag.TooMany", $"Maximum {maxCount} tags allowed per user");

    public static Error TagLimitReached =>
        Error.Validation("Users.Tag.LimitReached", "Maximum number of tags reached");

    public static Error TooManyTagsForGroup(int maxCount) =>
        Error.Validation("Users.Tag.TooManyForGroup", $"Maximum {maxCount} tags allowed per group");

    // === ОШИБКИ ОПЕРАЦИЙ ===

    public static Error CreateFailed(string reason) =>
        Error.Failure("Users.Tag.CreateFailed", $"Tag creation failed: {reason}");

    public static Error UpdateFailed(string reason) =>
        Error.Failure("Users.Tag.UpdateFailed", $"Tag update failed: {reason}");

    public static Error DeleteFailed(string reason) =>
        Error.Failure("Users.Tag.DeleteFailed", $"Tag deletion failed: {reason}");

    public static Error AssignToGroupFailed(string reason) =>
        Error.Failure("Users.Tag.AssignToGroupFailed", $"Failed to assign tag to group: {reason}");

    public static Error RemoveFromGroupFailed(string reason) =>
        Error.Failure("Users.Tag.RemoveFromGroupFailed", $"Failed to remove tag from group: {reason}");

    public static Error InvalidOperation(string reason) =>
        Error.Problem("Users.Tag.InvalidOperation", reason);

    public static Error DataIntegrityViolation(string details) =>
        Error.Problem("Users.Tag.DataIntegrityViolation", $"Data integrity violation: {details}");
}
