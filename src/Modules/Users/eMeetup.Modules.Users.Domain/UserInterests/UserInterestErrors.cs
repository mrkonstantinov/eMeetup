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


    public static Error InvalidTagSlug =>
    Error.Validation("User.InvalidTagSlug", "Tag slug is invalid");

    public static Error InterestAddFailed =>
        Error.Validation("User.InterestAddFailed", "Failed to add interest to user");

    public static Error InterestRemoveFailed =>
        Error.Validation("User.InterestRemoveFailed", "Failed to remove interest from user");

    public static Error InterestSyncFailed =>
        Error.Validation("User.InterestSyncFailed", "Failed to sync user interests");

    public static Error InterestRetrievalFailed =>
        Error.Validation("User.InterestRetrievalFailed", "Failed to retrieve user interests");

    public static Error InterestAlreadyAdded =>
        Error.Validation("User.InterestAlreadyAdded", "This interest has already been added to the user");

    public static Error TooManyInterests(int maxInterests) =>
    Error.Validation("User.TooManyInterests", $"Cannot add more than {maxInterests} interests");

    public static Error InterestNotFound =>
        Error.NotFound("User.InterestNotFound", "Interest was not found for this user");
}
