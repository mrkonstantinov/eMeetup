using System;
using System.Collections.Generic;
using System.Text;
using eMeetup.Common.Domain;

namespace eMeetup.Modules.Users.Domain.Tags;

public static class TagGroupErrors
{
    public static Error InvalidName() =>
        Error.Validation("TagGroup.InvalidName", "Group name is required");

    public static Error NameTooShort() =>
        Error.Validation("TagGroup.NameTooShort", "Group name must be at least 2 characters");

    public static Error NameTooLong() =>
        Error.Validation("TagGroup.NameTooLong", "Group name cannot exceed 50 characters");

    public static Error DescriptionTooLong() =>
        Error.Validation("TagGroup.DescriptionTooLong", "Group description cannot exceed 200 characters");

    public static Error IconTooLong() =>
        Error.Validation("TagGroup.IconTooLong", "Group icon cannot exceed 50 characters");

    public static Error NotFound() =>
        Error.NotFound("TagGroup.NotFound", "Tag group not found");

    public static Error DuplicateName() =>
        Error.Conflict("TagGroup.DuplicateName", "A tag group with this name already exists");

    public static Error GroupInactive() =>
        Error.Conflict("TagGroup.Inactive", "Cannot modify an inactive group");

    public static Error InvalidTag() =>
        Error.Validation("TagGroup.InvalidTag", "Invalid tag provided");
}
