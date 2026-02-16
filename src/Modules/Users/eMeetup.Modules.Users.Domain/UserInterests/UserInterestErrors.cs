using eMeetup.Common.Domain;

namespace eMeetup.Modules.Users.Domain.UserInterests;

public static class UserInterestErrors
{
    public static Error EmptyTags() =>
        Error.Validation("UserInterest.EmptyTags", "Tag names cannot be empty");

    public static Error InvalidTags() =>
        Error.Validation("UserInterest.InvalidTags", "No valid tags provided");

    public static Error TagsNotFound() =>
        Error.NotFound("UserInterest.TagsNotFound", "No tags could be created or found");

    public static Error AlreadyExists() =>
        Error.Conflict("UserInterest.AlreadyExists", "User already has this interest");

    public static Error NotFound(Guid userId, Guid? tagId = null) =>
        tagId.HasValue
            ? Error.NotFound("UserInterest.NotFound", $"User interest not found for user {userId} and tag {tagId}")
            : Error.NotFound("UserInterest.NotFound", $"No interests found for user {userId}");

    public static Error AddFailed(string details) =>
        Error.Failure("UserInterest.AddFailed", $"Failed to add user interest: {details}");

    public static Error UpdateFailed(string details) =>
        Error.Failure("UserInterest.UpdateFailed", $"Failed to update user interests: {details}");

    public static Error DeleteFailed(string details) =>
        Error.Failure("UserInterest.DeleteFailed", $"Failed to delete user interests: {details}");

    public static Error EmptyTagIds() =>
        Error.Validation("UserInterest.EmptyTagIds", "Tag IDs cannot be empty");
}
