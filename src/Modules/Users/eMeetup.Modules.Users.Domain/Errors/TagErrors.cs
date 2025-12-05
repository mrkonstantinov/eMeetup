using System;
using System.Collections.Generic;
using System.Text;
using eMeetup.Common.Domain;

namespace eMeetup.Modules.Users.Domain.Errors
{
    public static class TagErrors
    {
        public static Error InvalidName =>
            Error.Validation("Tag.InvalidName",
            "Tag name cannot be empty");

        public static Error NameTooShort =>
            Error.Validation("Tag.NameTooShort",
            "Tag name must be at least 2 characters long");

        public static Error NameTooLong =>
            Error.Validation("Tag.NameTooLong",
            "Tag name cannot exceed 50 characters");

        public static Error DescriptionTooLong =>
            Error.Validation("Tag.DescriptionTooLong",
            "Tag description cannot exceed 200 characters");

        public static Error InvalidCharacters =>
            Error.Validation("Tag.InvalidCharacters",
            "Tag name contains invalid characters. Only letters, numbers, spaces, and hyphens are allowed");

        public static Error NotFound =>
            Error.NotFound("Tag.NotFound",
            "Tag not found");

        public static Error AlreadyExists =>
            Error.Validation("Tag.AlreadyExists",
            "Tag with this name already exists");

        public static Error Inactive =>
            Error.Validation("Tag.Inactive", "Tag is inactive");

        public static Error SlugNotFound(string slug) =>
            Error.NotFound("Tag.SlugNotFound", $"Tag with slug '{slug}' was not found");

        public static Error SlugParseFailed => 
            Error.Conflict("Tag.SlugParseFailed", "Failed to parse slugs");
    }
}
