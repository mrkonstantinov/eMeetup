using System;
using System.Collections.Generic;
using System.Text;
using eMeetup.Common.Domain;

namespace eMeetup.Modules.Events.Domain.ActivityCategories;

public sealed class ActivityCategory : Entity
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? Icon { get; private set; }
    public string? Color { get; private set; }
    public int? ParentId { get; private set; }
    public ActivityCategory? Parent { get; private set; }
    public int DisplayOrder { get; private set; }
    public ActivityCategoryStatus Status { get; private set; }
    public bool IsSystem { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    private readonly List<ActivityCategory> _children = new();
    public IReadOnlyCollection<ActivityCategory> Children => _children.AsReadOnly();

    private ActivityCategory() { } // EF Core

    private ActivityCategory(
        string name,
        string? description = null,
        string? icon = null,
        string? color = null,
        int? parentId = null,
        int displayOrder = 0,
        bool isSystem = false)
    {
        Id = 0;
        Name = name.Trim();
        Slug = GenerateSlug(name);
        Description = description?.Trim();
        Icon = icon;
        Color = color ?? GenerateRandomColor();
        ParentId = parentId;
        DisplayOrder = displayOrder;
        Status = ActivityCategoryStatus.Active;
        IsSystem = isSystem;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public static Result<ActivityCategory> Create(
        string name,
        string? description = null,
        string? icon = null,
        string? color = null,
        int? parentId = null,
        int displayOrder = 0,
        bool isSystem = false)
    {
        // Валидация
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<ActivityCategory>(ActivityCategoryErrors.InvalidName);

        var trimmedName = name.Trim();

        if (trimmedName.Length < 2)
            return Result.Failure<ActivityCategory>(ActivityCategoryErrors.NameTooShort(2));

        if (trimmedName.Length > 100)
            return Result.Failure<ActivityCategory>(ActivityCategoryErrors.NameTooLong(100));

        if (description?.Length > 500)
            return Result.Failure<ActivityCategory>(ActivityCategoryErrors.DescriptionTooLong(500));

        if (icon?.Length > 50)
            return Result.Failure<ActivityCategory>(ActivityCategoryErrors.IconTooLong(50));

        if (color != null && !IsValidColor(color))
            return Result.Failure<ActivityCategory>(ActivityCategoryErrors.InvalidColor);

        if (displayOrder < 0)
            return Result.Failure<ActivityCategory>(ActivityCategoryErrors.InvalidDisplayOrder);

        var category = new ActivityCategory(
            trimmedName,
            description,
            icon,
            color,
            parentId,
            displayOrder,
            isSystem);

        return Result.Success(category);
    }

    public Result Update(
        string name,
        string? description = null,
        string? icon = null,
        string? color = null,
        int? parentId = null,
        int? displayOrder = null)
    {
        if (Status == ActivityCategoryStatus.Inactive)
            return Result.Failure(ActivityCategoryErrors.CategoryInactive);

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(ActivityCategoryErrors.InvalidName);

        var trimmedName = name.Trim();

        if (trimmedName.Length < 2)
            return Result.Failure(ActivityCategoryErrors.NameTooShort(2));

        if (trimmedName.Length > 100)
            return Result.Failure(ActivityCategoryErrors.NameTooLong(100));

        if (description?.Length > 500)
            return Result.Failure(ActivityCategoryErrors.DescriptionTooLong(500));

        if (icon?.Length > 50)
            return Result.Failure(ActivityCategoryErrors.IconTooLong(50));

        if (color != null && !IsValidColor(color))
            return Result.Failure(ActivityCategoryErrors.InvalidColor);

        if (parentId.HasValue && parentId.Value == Id)
            return Result.Failure(ActivityCategoryErrors.CannotBeOwnParent);

        Name = trimmedName;
        Slug = GenerateSlug(trimmedName);
        Description = description?.Trim();
        Icon = icon;
        Color = color ?? Color;
        ParentId = parentId ?? ParentId;

        if (displayOrder.HasValue)
        {
            if (displayOrder < 0)
                return Result.Failure(ActivityCategoryErrors.InvalidDisplayOrder);
            DisplayOrder = displayOrder.Value;
        }

        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public Result Activate()
    {
        if (Status == ActivityCategoryStatus.Active)
            return Result.Failure(ActivityCategoryErrors.AlreadyActive);

        Status = ActivityCategoryStatus.Active;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public Result Deactivate()
    {
        if (Status == ActivityCategoryStatus.Inactive)
            return Result.Failure(ActivityCategoryErrors.AlreadyInactive);

        if (IsSystem)
            return Result.Failure(ActivityCategoryErrors.CannotDeactivateSystem);

        Status = ActivityCategoryStatus.Inactive;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public Result AddChild(ActivityCategory child)
    {
        if (child == null)
            return Result.Failure(ActivityCategoryErrors.InvalidChild);

        if (_children.Any(c => c.Id == child.Id))
            return Result.Failure(ActivityCategoryErrors.ChildAlreadyExists);

        _children.Add(child);
        return Result.Success();
    }

    public bool HasChildren() => _children.Any();

    public bool IsRoot() => ParentId is null;

    // === PRIVATE HELPERS ===

    private static string GenerateSlug(string name)
    {
        return name.Trim()
            .ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("ё", "e")
            .Replace("й", "i")
            .Replace("ц", "ts")
            .Replace("у", "u")
            .Replace("к", "k")
            .Replace("е", "e")
            .Replace("н", "n")
            .Replace("г", "g")
            .Replace("ш", "sh")
            .Replace("щ", "sch")
            .Replace("з", "z")
            .Replace("х", "h")
            .Replace("ъ", "")
            .Replace("ф", "f")
            .Replace("ы", "y")
            .Replace("в", "v")
            .Replace("а", "a")
            .Replace("п", "p")
            .Replace("р", "r")
            .Replace("о", "o")
            .Replace("л", "l")
            .Replace("д", "d")
            .Replace("ж", "zh")
            .Replace("э", "e")
            .Replace("я", "ya")
            .Replace("ч", "ch")
            .Replace("с", "s")
            .Replace("м", "m")
            .Replace("и", "i")
            .Replace("т", "t")
            .Replace("ь", "")
            .Replace("б", "b")
            .Replace("ю", "yu")
            .Replace("--", "-")
            .Trim('-');
    }

    private static bool IsValidColor(string color)
    {
        if (string.IsNullOrWhiteSpace(color)) return false;
        if (!color.StartsWith("#")) return false;
        var hex = color.TrimStart('#');
        return hex.Length == 3 || hex.Length == 6;
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

    /// <summary>
    /// Создает категорию для seed data (EF Core migrations).
    /// Позволяет явно задать Id и CreatedAt.
    /// </summary>
    public static ActivityCategory CreateForSeeding(
        int id,
        string name,
        string? description,
        string? icon,
        string? color,
        int? parentId,
        int displayOrder,
        DateTimeOffset createdAt)
    {
        var category = new ActivityCategory(
            name,
            description,
            icon,
            color,
            parentId,
            displayOrder,
            isSystem: true);

        // Устанавливаем Id и CreatedAt через рефлексию или backing field
        SetId(category, id);
        SetCreatedAt(category, createdAt);

        return category;
    }

    // === PRIVATE HELPERS ДЛЯ УСТАНОВКИ ID ===

    private static void SetId(ActivityCategory category, int id)
    {
        typeof(ActivityCategory)
            .GetProperty(nameof(Id))?
            .SetValue(category, id);
    }

    private static void SetCreatedAt(ActivityCategory category, DateTimeOffset createdAt)
    {
        typeof(ActivityCategory)
            .GetProperty(nameof(CreatedAt))?
            .SetValue(category, createdAt);
    }

    public override string ToString() => Name;
}
