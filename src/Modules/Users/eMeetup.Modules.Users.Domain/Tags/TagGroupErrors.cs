using System;
using System.Collections.Generic;
using System.Text;
using eMeetup.Common.Domain;

namespace eMeetup.Modules.Users.Domain.Tags;

public static class TagGroupErrors
{
    // === ОШИБКИ ВАЛИДАЦИИ ===

    public static Error InvalidName =>
        Error.Validation("Users.TagGroup.InvalidName", "Group name cannot be empty");

    public static Error NameTooShort(int minLength) =>
        Error.Validation("Users.TagGroup.NameTooShort", $"Group name must be at least {minLength} characters");

    public static Error NameTooLong(int maxLength) =>
        Error.Validation("Users.TagGroup.NameTooLong", $"Group name cannot exceed {maxLength} characters");

    public static Error DescriptionTooLong(int maxLength) =>
        Error.Validation("Users.TagGroup.DescriptionTooLong", $"Description cannot exceed {maxLength} characters");

    public static Error IconTooLong(int maxLength) =>
        Error.Validation("Users.TagGroup.IconTooLong", $"Icon cannot exceed {maxLength} characters");

    public static Error InvalidColor =>
        Error.Validation("Users.TagGroup.InvalidColor", "Invalid color format. Use HEX color (e.g., #FF6B6B)");

    public static Error InvalidDisplayOrder =>
        Error.Validation("Users.TagGroup.InvalidDisplayOrder", "Display order must be greater than or equal to 0");

    // === ОШИБКИ НЕ НАЙДЕНО ===

    public static Error NotFound(Guid groupId) =>
        Error.NotFound("Users.TagGroup.NotFound", $"Tag group with ID '{groupId}' was not found");

    public static Error NotFound(string name) =>
        Error.NotFound("Users.TagGroup.NotFoundByName", $"Tag group with name '{name}' was not found");

    // === ОШИБКИ КОНФЛИКТА ===

    public static Error NameAlreadyExists(string name) =>
        Error.Conflict("Users.TagGroup.NameAlreadyExists", $"Tag group with name '{name}' already exists");

    public static Error TagAlreadyInGroup =>
        Error.Conflict("Users.TagGroup.TagAlreadyInGroup", "Tag is already in this group");

    // === ОШИБКИ СТАТУСА ===

    public static Error GroupInactive =>
        Error.Problem("Users.TagGroup.Inactive", "Cannot perform operation on inactive group");

    public static Error TagInactive =>
        Error.Problem("Users.TagGroup.TagInactive", "Cannot add inactive tag to group");

    public static Error TagNotFoundInGroup =>
        Error.NotFound("Users.TagGroup.TagNotFound", "Tag not found in this group");

    public static Error CannotDeleteSystemGroup =>
        Error.Problem("Users.TagGroup.CannotDeleteSystem", "Cannot delete system group");

    public static Error CannotModifySystemGroup =>
        Error.Problem("Users.TagGroup.CannotModifySystem", "Cannot modify system group");

    // === ОШИБКИ ОПЕРАЦИЙ ===

    public static Error InvalidTag =>
        Error.Validation("Users.TagGroup.InvalidTag", "Invalid tag provided");

    public static Error CreateFailed(string reason) =>
        Error.Failure("Users.TagGroup.CreateFailed", $"Failed to create tag group: {reason}");

    public static Error UpdateFailed(string reason) =>
        Error.Failure("Users.TagGroup.UpdateFailed", $"Failed to update tag group: {reason}");

    public static Error DeleteFailed(string reason) =>
        Error.Failure("Users.TagGroup.DeleteFailed", $"Failed to delete tag group: {reason}");

    public static Error InvalidOperation(string reason) =>
        Error.Problem("Users.TagGroup.InvalidOperation", reason);
}
