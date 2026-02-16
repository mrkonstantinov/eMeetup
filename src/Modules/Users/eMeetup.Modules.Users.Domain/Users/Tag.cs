using System.ComponentModel.DataAnnotations;
using eMeetup.Common.Domain;
using eMeetup.Modules.Users.Domain.Errors;
using eMeetup.Modules.Users.Domain.UserInterests;

namespace eMeetup.Modules.Users.Domain.Users;

public class Tag
{
    public Tag() { }

    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; private set; }
    public string Slug { get; private set; }
    public string Description { get; private set; }
    public int UsageCount { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Navigation property back to UserInterests
    public virtual ICollection<UserInterest>? UserInterests { get; set; } = new List<UserInterest>();


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
    public static Result<Tag> Create(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Tag>(TagErrors.InvalidName);

        var trimmedName = name.Trim();
        
        if (trimmedName.Length < 2)
            return Result.Failure<Tag>(TagErrors.NameTooShort);

        if (trimmedName.Length > 50)
            return Result.Failure<Tag>(TagErrors.NameTooLong);

        if (description?.Length > 200)
            return Result.Failure<Tag>(TagErrors.DescriptionTooLong);

        if (!IsValidTagName(trimmedName))
            return Result.Failure<Tag>(TagErrors.InvalidCharacters);

        return new Tag(trimmedName, description ?? string.Empty);
    }

    // Business methods
    public Result Update(string name, string? description = null)
    {
        var validationResult = Create(name, description);
        if (validationResult.IsFailure)
            return Result.Failure(validationResult.Error);

        Name = validationResult.Value.Name;
        Slug = validationResult.Value.Slug;
        Description = validationResult.Value.Description;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public void IncrementUsage()
    {
        UsageCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void DecrementUsage()
    {
        UsageCount = Math.Max(0, UsageCount - 1);
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetUsageCount(int count)
    {
        UsageCount = Math.Max(0, count);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
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
