using System;
using System.Collections.Generic;
using System.Text;
using eMeetup.Common.Domain;

namespace eMeetup.Modules.Users.Domain.Tags;

public class TagGroup : Entity
{
    private readonly List<UserTag> _tags = new();

    // === PROPERTIES ===
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public string? Icon { get; private set; }
    public string? Color { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsSystem { get; private set; } // Системная группа (из enum) или пользовательская
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? DeactivatedAt { get; private set; }

    // === NAVIGATION ===
    public virtual IReadOnlyCollection<UserTag> Tags => _tags.AsReadOnly();

    // === PRIVATE CONSTRUCTOR ===
    private TagGroup() { } // For EF Core

    private TagGroup(
        string name,
        string? description = null,
        string? icon = null,
        string? color = null,
        int displayOrder = 0,
        bool isSystem = false)
    {
        Id = Guid.NewGuid();
        Name = name.Trim();
        Description = description?.Trim();
        Icon = icon;
        Color = color ?? GenerateRandomColor();
        DisplayOrder = displayOrder;
        IsActive = true;
        IsSystem = isSystem;
        CreatedAt = DateTime.UtcNow;
    }

    // === FACTORY METHODS ===

    /// <summary>
    /// Создает системную группу (из предопределенных категорий)
    /// </summary>
    public static Result<TagGroup> CreateSystem(
        string name,
        string? description = null,
        string? icon = null,
        string? color = null,
        int displayOrder = 0)
    {
        return Create(name, description, icon, color, displayOrder, isSystem: true);
    }

    /// <summary>
    /// Создает пользовательскую группу
    /// </summary>
    public static Result<TagGroup> CreateCustom(
        string name,
        string? description = null,
        string? icon = null,
        string? color = null,
        int displayOrder = 0)
    {
        return Create(name, description, icon, color, displayOrder, isSystem: false);
    }

    private static Result<TagGroup> Create(
        string name,
        string? description = null,
        string? icon = null,
        string? color = null,
        int displayOrder = 0,
        bool isSystem = false)
    {
        // Валидация
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<TagGroup>(TagGroupErrors.InvalidName);

        var trimmedName = name.Trim();

        if (trimmedName.Length < 2)
            return Result.Failure<TagGroup>(TagGroupErrors.NameTooShort(2));

        if (trimmedName.Length > 50)
            return Result.Failure<TagGroup>(TagGroupErrors.NameTooLong(50));

        if (description?.Length > 500)
            return Result.Failure<TagGroup>(TagGroupErrors.DescriptionTooLong(500));

        if (icon?.Length > 50)
            return Result.Failure<TagGroup>(TagGroupErrors.IconTooLong(50));

        if (color != null && !IsValidColor(color))
            return Result.Failure<TagGroup>(TagGroupErrors.InvalidColor);

        if (displayOrder < 0)
            return Result.Failure<TagGroup>(TagGroupErrors.InvalidDisplayOrder);

        var group = new TagGroup(
            trimmedName,
            description,
            icon,
            color,
            displayOrder,
            isSystem);

        return Result<TagGroup>.Success(group);
    }

    // === BUSINESS METHODS ===

    public Result Update(
        string name,
        string? description = null,
        string? icon = null,
        string? color = null,
        int? displayOrder = null)
    {
        if (!IsActive)
            return Result.Failure(TagGroupErrors.GroupInactive);

        // Валидация
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(TagGroupErrors.InvalidName);

        var trimmedName = name.Trim();

        if (trimmedName.Length < 2)
            return Result.Failure(TagGroupErrors.NameTooShort(2));

        if (trimmedName.Length > 50)
            return Result.Failure(TagGroupErrors.NameTooLong(50));

        if (description?.Length > 500)
            return Result.Failure(TagGroupErrors.DescriptionTooLong(500));

        if (icon?.Length > 50)
            return Result.Failure(TagGroupErrors.IconTooLong(50));

        if (color != null && !IsValidColor(color))
            return Result.Failure(TagGroupErrors.InvalidColor);

        if (displayOrder.HasValue && displayOrder < 0)
            return Result.Failure(TagGroupErrors.InvalidDisplayOrder);

        // Обновляем
        Name = trimmedName;
        Description = description?.Trim();
        Icon = icon;
        Color = color ?? Color;

        if (displayOrder.HasValue)
            DisplayOrder = displayOrder.Value;

        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result AddTag(UserTag tag)
    {
        if (tag == null)
            return Result.Failure(TagGroupErrors.InvalidTag);

        if (!IsActive)
            return Result.Failure(TagGroupErrors.GroupInactive);

        if (!tag.IsActive)
            return Result.Failure(TagGroupErrors.TagInactive);

        if (_tags.Any(t => t.Id == tag.Id))
            return Result.Failure(TagGroupErrors.TagAlreadyInGroup);

        _tags.Add(tag);
        tag.AssignToGroup(Id);
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result RemoveTag(UserTag tag)
    {
        if (tag == null)
            return Result.Failure(TagGroupErrors.InvalidTag);

        if (!_tags.Any(t => t.Id == tag.Id))
            return Result.Failure(TagGroupErrors.TagNotFoundInGroup);

        _tags.Remove(tag);
        tag.RemoveFromGroup();
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result RemoveAllTags()
    {
        if (!_tags.Any())
            return Result.Success();

        foreach (var tag in _tags.ToList())
        {
            tag.RemoveFromGroup();
        }

        _tags.Clear();
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public void Activate()
    {
        if (!IsActive)
        {
            IsActive = true;
            DeactivatedAt = null;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void Deactivate()
    {
        if (IsActive)
        {
            IsActive = false;
            DeactivatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public bool HasTags()
    {
        return _tags.Any(t => t.IsActive);
    }

    public int GetActiveTagsCount()
    {
        return _tags.Count(t => t.IsActive);
    }

    public IReadOnlyList<UserTag> GetActiveTags()
    {
        return _tags
            .Where(t => t.IsActive)
            .OrderBy(t => t.DisplayOrder)
            .ThenBy(t => t.Name)
            .ToList()
            .AsReadOnly();
    }

    // === PRIVATE HELPERS ===

    private static bool IsValidColor(string color)
    {
        // Проверка HEX цвета (#RRGGBB или #RGB)
        if (string.IsNullOrWhiteSpace(color))
            return false;

        if (!color.StartsWith("#"))
            return false;

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

    // === OVERRIDES ===
    public override string ToString() => Name;
}

