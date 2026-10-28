using eMeetup.Common.Domain;

namespace eMeetup.Modules.Events.Domain.ActivityCategories;

public static class ActivityCategoryErrors
{
    // === ОШИБКИ ВАЛИДАЦИИ ===

    public static Error InvalidName =>
        Error.Validation("ActivityCategories.InvalidName", "Category name cannot be empty");

    public static Error NameTooShort(int minLength) =>
        Error.Validation("ActivityCategories.NameTooShort", $"Category name must be at least {minLength} characters");

    public static Error NameTooLong(int maxLength) =>
        Error.Validation("ActivityCategories.NameTooLong", $"Category name cannot exceed {maxLength} characters");

    public static Error DescriptionTooLong(int maxLength) =>
        Error.Validation("ActivityCategories.DescriptionTooLong", $"Description cannot exceed {maxLength} characters");

    public static Error IconTooLong(int maxLength) =>
        Error.Validation("ActivityCategories.IconTooLong", $"Icon cannot exceed {maxLength} characters");

    public static Error InvalidColor =>
        Error.Validation("ActivityCategories.InvalidColor", "Invalid color format. Use HEX (e.g., #FF6B6B)");

    public static Error InvalidDisplayOrder =>
        Error.Validation("ActivityCategories.InvalidDisplayOrder", "Display order must be greater than or equal to 0");

    // === ОШИБКИ НЕ НАЙДЕНО ===

    public static Error NotFound(int categoryId) =>
        Error.NotFound("ActivityCategories.NotFound", $"Category with ID '{categoryId}' was not found");

    public static Error NotFoundBySlug(string slug) =>
        Error.NotFound("ActivityCategories.NotFoundBySlug", $"Category with slug '{slug}' was not found");

    public static Error ParentNotFound(int parentId) =>
        Error.NotFound("ActivityCategories.ParentNotFound", $"Parent category with ID '{parentId}' was not found");

    // === ОШИБКИ КОНФЛИКТА ===

    public static Error NameAlreadyExists(string name) =>
        Error.Conflict("ActivityCategories.NameAlreadyExists", $"Category with name '{name}' already exists");

    public static Error SlugAlreadyExists(string slug) =>
        Error.Conflict("ActivityCategories.SlugAlreadyExists", $"Category with slug '{slug}' already exists");

    public static Error AlreadyActive =>
        Error.Conflict("ActivityCategories.AlreadyActive", "Category is already active");

    public static Error AlreadyInactive =>
        Error.Conflict("ActivityCategories.AlreadyInactive", "Category is already inactive");

    public static Error ChildAlreadyExists =>
        Error.Conflict("ActivityCategories.ChildAlreadyExists", "Child category already exists");

    public static Error CannotBeOwnParent =>
        Error.Validation("ActivityCategories.CannotBeOwnParent", "Category cannot be its own parent");

    // === ОШИБКИ СТАТУСА ===

    public static Error CategoryInactive =>
        Error.Problem("ActivityCategories.Inactive", "Cannot perform operation on inactive category");

    public static Error CannotDeactivateSystem =>
        Error.Problem("ActivityCategories.CannotDeactivateSystem", "Cannot deactivate system category");

    public static Error CannotDeleteWithActivities =>
        Error.Problem("ActivityCategories.CannotDeleteWithActivities", "Cannot delete category with existing activities");

    public static Error CannotDeleteWithChildren =>
        Error.Problem("ActivityCategories.CannotDeleteWithChildren", "Cannot delete category with children");

    // === ОШИБКИ ОПЕРАЦИЙ ===

    public static Error InvalidChild =>
        Error.Validation("ActivityCategories.InvalidChild", "Invalid child category");

    public static Error CreateFailed(string reason) =>
        Error.Failure("ActivityCategories.CreateFailed", $"Failed to create category: {reason}");

    public static Error UpdateFailed(string reason) =>
        Error.Failure("ActivityCategories.UpdateFailed", $"Failed to update category: {reason}");

    public static Error DeleteFailed(string reason) =>
        Error.Failure("ActivityCategories.DeleteFailed", $"Failed to delete category: {reason}");

    public static Error InvalidOperation(string reason) =>
        Error.Problem("ActivityCategories.InvalidOperation", reason);
}
