using System.ComponentModel.DataAnnotations;
using eMeetup.Common.Domain;
using eMeetup.Modules.Users.Domain.Errors;
using eMeetup.Modules.Users.Domain.UserInterests;

namespace eMeetup.Modules.Users.Domain.Tags;

public class Tag
{
    public Tag() { }

    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; private set; }
    public string Slug { get; private set; }
    public string Description { get; private set; }
    public int UsageCount { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Navigation property back to UserInterests
    public virtual ICollection<UserInterest>? UserInterests { get; set; } = new List<UserInterest>();

    // New group relationship
    public Guid? TagGroupId { get; private set; }
    public virtual TagGroup? TagGroup { get; private set; }

    // Private constructor
    private Tag(string name, string description)
    {
        Id = Guid.NewGuid();
        Name = name.Trim();
        Slug = GenerateSlug(name);
        Description = description?.Trim() ?? string.Empty;
        UsageCount = 0;
        IsActive = true;
    }

    // Factory method
    public static Result<Tag> Create(string name, string? description = null, Guid? tagGroupId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Tag>(TagErrors.InvalidName());

        var trimmedName = name.Trim();

        if (trimmedName.Length < 2)
            return Result.Failure<Tag>(TagErrors.NameTooShort());

        if (trimmedName.Length > 50)
            return Result.Failure<Tag>(TagErrors.NameTooLong());

        if (description?.Length > 200)
            return Result.Failure<Tag>(TagErrors.DescriptionTooLong());

        if (!IsValidTagName(trimmedName))
            return Result.Failure<Tag>(TagErrors.InvalidCharacters());

        var tag = new Tag(trimmedName, description ?? string.Empty);

        if (tagGroupId.HasValue)
        {
            tag.TagGroupId = tagGroupId;
        }

        return Result.Success(tag);
    }

    // For EF Core seeding only
    public static Result<Tag> CreateForSeeding(Guid id, string name, string? description = null, Guid? tagGroupId = null)
    {
        var result = Create(name, description, tagGroupId);
        if (result.IsFailure)
            return result;

        var tag = result.Value;
        // Use reflection to set the Id since it has a private setter
        typeof(Tag).GetProperty(nameof(Id))?.SetValue(tag, id);

        return Result.Success(tag);
    }

    // Business methods
    public Result Update(string name, string? description = null, Guid? tagGroupId = null)
    {
        // Validate the updated values
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(TagErrors.InvalidName());

        var trimmedName = name.Trim();

        if (trimmedName.Length < 2)
            return Result.Failure(TagErrors.NameTooShort());

        if (trimmedName.Length > 50)
            return Result.Failure(TagErrors.NameTooLong());

        if (description?.Length > 200)
            return Result.Failure(TagErrors.DescriptionTooLong());

        if (!IsValidTagName(trimmedName))
            return Result.Failure(TagErrors.InvalidCharacters());

        // Update properties
        Name = trimmedName;
        Slug = GenerateSlug(trimmedName);
        Description = description?.Trim() ?? string.Empty;

        // Update group if provided
        if (tagGroupId.HasValue)
        {
            TagGroupId = tagGroupId;
        }

        return Result.Success();
    }

    // New method to assign to a group
    public Result AssignToGroup(Guid groupId)
    {
        if (groupId == Guid.Empty)
            return Result.Failure(TagErrors.InvalidGroupId());

        TagGroupId = groupId;

        return Result.Success();
    }

    // New method to remove from current group
    public Result RemoveFromGroup()
    {
        TagGroupId = null;

        return Result.Success();
    }

    // New method to change group
    public Result ChangeGroup(Guid newGroupId)
    {
        if (newGroupId == Guid.Empty)
            return Result.Failure(TagErrors.InvalidGroupId());

        if (TagGroupId == newGroupId)
            return Result.Success(); // Already in this group

        TagGroupId = newGroupId;

        return Result.Success();
    }

    public void IncrementUsage()
    {
        UsageCount++;
    }

    public void DecrementUsage()
    {
        if (UsageCount > 0)
        {
            UsageCount--;
        }
    }

    public void SetUsageCount(int count)
    {
        UsageCount = Math.Max(0, count);
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    // Helper methods
    private static string GenerateSlug(string name)
    {
        return name.Trim()
            .ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("--", "-")
            .Trim('-');
    }

    private static bool IsValidTagName(string name)
    {
        // Allow letters, numbers, spaces, hyphens
        return name.All(c => char.IsLetterOrDigit(c) || c == ' ' || c == '-');
    }

    public override string ToString() => Name;
}
