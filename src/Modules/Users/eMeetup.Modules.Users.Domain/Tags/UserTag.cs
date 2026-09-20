using eMeetup.Common.Domain;

namespace eMeetup.Modules.Users.Domain.Tags;

public class UserTag : Entity
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Slug { get; private set; }
    public string? Description { get; private set; }
    public string? Icon { get; private set; }
    public string? Color { get; private set; }
    public int UsageCount { get; private set; }
    public bool IsActive { get; private set; }
    public int DisplayOrder { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Связь с группой
    public Guid? TagGroupId { get; private set; }
    public virtual TagGroup? TagGroup { get; private set; }

    private UserTag() { } // For EF

    private UserTag(
        string name,
        string? description = null,
        string? icon = null,
        string? color = null,
        int displayOrder = 0,
        Guid? tagGroupId = null)
    {
        Id = Guid.NewGuid();
        Name = name.Trim();
        Slug = GenerateSlug(name);
        Description = description?.Trim();
        Icon = icon;
        Color = color ?? GenerateRandomColor();
        UsageCount = 0;
        IsActive = true;
        DisplayOrder = displayOrder;
        TagGroupId = tagGroupId;
        CreatedAt = DateTime.UtcNow;
    }

    public static Result<UserTag> Create(
        string name,
        string? description = null,
        string? icon = null,
        string? color = null,
        int displayOrder = 0,
        Guid? tagGroupId = null)
    {
        // === ВАЛИДАЦИЯ ===

        // Name
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<UserTag>(TagErrors.InvalidTagName);

        var trimmedName = name.Trim();

        if (trimmedName.Length < 2)
            return Result.Failure<UserTag>(TagErrors.TagNameTooShort(2));

        if (trimmedName.Length > 50)
            return Result.Failure<UserTag>(TagErrors.TagNameTooLong(50));

        if (!IsValidTagName(trimmedName))
            return Result.Failure<UserTag>(TagErrors.InvalidCharacters);

        // Description
        if (description?.Length > 200)
            return Result.Failure<UserTag>(TagErrors.DescriptionTooLong);

        // Icon
        if (icon?.Length > 50)
            return Result.Failure<UserTag>(TagErrors.IconTooLong);

        // Color
        if (color != null && !IsValidColor(color))
            return Result.Failure<UserTag>(TagErrors.InvalidColor);

        // DisplayOrder
        if (displayOrder < 0)
            return Result.Failure<UserTag>(TagErrors.InvalidDisplayOrder);

        // Создаем тег
        var tag = new UserTag(
            trimmedName,
            description,
            icon,
            color,
            displayOrder,
            tagGroupId);

        return Result<UserTag>.Success(tag);
    }

    public Result Update(
        string name,
        string? description = null,
        string? icon = null,
        string? color = null,
        int? displayOrder = null,
        Guid? tagGroupId = null)
    {
        if (!IsActive)
            return Result.Failure(TagErrors.TagNotActive);

        // Name
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(TagErrors.InvalidTagName);

        var trimmedName = name.Trim();

        if (trimmedName.Length < 2)
            return Result.Failure(TagErrors.TagNameTooShort(2));

        if (trimmedName.Length > 50)
            return Result.Failure(TagErrors.TagNameTooLong(50));

        if (!IsValidTagName(trimmedName))
            return Result.Failure(TagErrors.InvalidCharacters);

        // Description
        if (description?.Length > 200)
            return Result.Failure(TagErrors.DescriptionTooLong);

        // Icon
        if (icon?.Length > 50)
            return Result.Failure(TagErrors.IconTooLong);

        // Color
        if (color != null && !IsValidColor(color))
            return Result.Failure(TagErrors.InvalidColor);

        // DisplayOrder
        if (displayOrder.HasValue && displayOrder < 0)
            return Result.Failure(TagErrors.InvalidDisplayOrder);

        // Обновляем
        Name = trimmedName;
        Slug = GenerateSlug(trimmedName);
        Description = description?.Trim();
        Icon = icon;
        Color = color ?? Color;

        if (displayOrder.HasValue)
            DisplayOrder = displayOrder.Value;

        if (tagGroupId.HasValue)
            TagGroupId = tagGroupId;

        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result AssignToGroup(Guid groupId)
    {
        if (groupId == Guid.Empty)
            return Result.Failure(TagErrors.InvalidGroupId);

        if (!IsActive)
            return Result.Failure(TagErrors.TagNotActive);

        if (TagGroupId == groupId)
            return Result.Success();

        TagGroupId = groupId;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result RemoveFromGroup()
    {
        if (!TagGroupId.HasValue)
            return Result.Success();

        TagGroupId = null;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public void IncrementUsage()
    {
        UsageCount++;
    }

    public void DecrementUsage()
    {
        if (UsageCount > 0)
            UsageCount--;
    }

    public void SetUsageCount(int count)
    {
        UsageCount = Math.Max(0, count);
    }

    public void Activate()
    {
        if (!IsActive)
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void Deactivate()
    {
        if (IsActive)
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    // === PRIVATE HELPERS ===

    private static bool IsValidTagName(string name)
    {
        return name.All(c => char.IsLetterOrDigit(c) || c == ' ' || c == '-');
    }

    private static bool IsValidColor(string color)
    {
        if (string.IsNullOrWhiteSpace(color))
            return false;

        if (!color.StartsWith("#"))
            return false;

        var hex = color.TrimStart('#');
        return hex.Length == 3 || hex.Length == 6;
    }

    private static string GenerateSlug(string name)
    {
        return name.Trim()
            .ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("--", "-")
            .Trim('-');
    }

    private static string GenerateRandomColor()
    {
        var colors = new[]
        {
            "#FF6B6B", "#4ECDC4", "#45B7D1", "#96CEB4",
            "#FFEAA7", "#DDA0DD", "#FF8A5C", "#A29BFE",
            "#FD79A8", "#00CEC9", "#FDCB6E", "#E17055",
            "#6C5CE7", "#00B894", "#0984E3", "#F39C12"
        };
        return colors[new Random().Next(colors.Length)];
    }

    public override string ToString() => Name;
}
