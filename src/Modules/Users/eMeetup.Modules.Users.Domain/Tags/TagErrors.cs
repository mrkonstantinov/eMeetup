using eMeetup.Common.Domain;

namespace eMeetup.Modules.Users.Domain.Tags;

public static class TagErrors
{
    public static Error NotFound =>
    Error.NotFound("Tag.NotFound", "Tag not found");

    public static Error SlugNotFound(string slug) =>
        Error.NotFound("Tag.SlugNotFound", $"Tag with slug '{slug}' was not found");

    public static Error SlugParseFailed =>
        Error.Conflict("Tag.SlugParseFailed", "Failed to parse slugs");
    public static Error InvalidName() =>
        Error.Validation("Tag.InvalidName", "Tag name is required");

    public static Error NameTooShort() =>
        Error.Validation("Tag.NameTooShort", "Tag name must be at least 2 characters");

    public static Error NameTooLong() =>
        Error.Validation("Tag.NameTooLong", "Tag name cannot exceed 50 characters");

    public static Error DescriptionTooLong() =>
        Error.Validation("Tag.DescriptionTooLong", "Description cannot exceed 200 characters");

    public static Error InvalidCharacters() =>
        Error.Validation("Tag.InvalidCharacters", "Tag name can only contain letters, numbers, spaces, and hyphens");

    // New group-related errors
    public static Error InvalidGroupId() =>
        Error.Validation("Tag.InvalidGroupId", "Valid group ID is required");

    public static Error GroupNotFound() =>
        Error.NotFound("Tag.GroupNotFound", "The specified tag group does not exist");

    public static Error GroupInactive() =>
        Error.Conflict("Tag.GroupInactive", "Cannot assign tag to an inactive group");
}
