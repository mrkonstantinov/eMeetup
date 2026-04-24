using eMeetup.Common.Domain;

namespace eMeetup.Modules.Events.Domain.TagGroups;

public class TagGroup
{
    public TagGroup() { }

    public int Id { get; set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public string? PictureFileName { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Navigation property
    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();

    private TagGroup(string name, string? description = null, string? pictureFileName = null, int displayOrder = 0)
    {
        Name = name.Trim();
        Description = description?.Trim();
        PictureFileName = pictureFileName;
        DisplayOrder = displayOrder;
        IsActive = true;
    }

    public static Result<TagGroup> Create(string name, string? description = null, string? pictureFileName = null, int displayOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<TagGroup>(TagGroupErrors.InvalidName());

        var trimmedName = name.Trim();

        if (trimmedName.Length < 2)
            return Result.Failure<TagGroup>(TagGroupErrors.NameTooShort());

        if (trimmedName.Length > 50)
            return Result.Failure<TagGroup>(TagGroupErrors.NameTooLong());

        if (description?.Length > 200)
            return Result.Failure<TagGroup>(TagGroupErrors.DescriptionTooLong());

        if (pictureFileName?.Length > 50)
            return Result.Failure<TagGroup>(TagGroupErrors.IconTooLong());

        return Result.Success(new TagGroup(trimmedName, description, pictureFileName, displayOrder));
    }

    // For EF Core seeding only
    public static Result<TagGroup> CreateForSeeding(int id, string name, string? description = null, string? pictureFileName = null, int displayOrder = 0)
    {
        pictureFileName = $"{id}.webp";
        var result = Create(name, description, pictureFileName, displayOrder);
        if (result.IsFailure)
            return result;

        var tagGroup = result.Value;
        // Use reflection to set the Id since it has a private setter
        typeof(TagGroup).GetProperty(nameof(Id))?.SetValue(tagGroup, id);

        return Result.Success(tagGroup);
    }

    public Result Update(string name, string? description = null, string? icon = null, int? displayOrder = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(TagGroupErrors.InvalidName());

        var trimmedName = name.Trim();

        if (trimmedName.Length < 2)
            return Result.Failure(TagGroupErrors.NameTooShort());

        if (trimmedName.Length > 50)
            return Result.Failure(TagGroupErrors.NameTooLong());

        if (description?.Length > 200)
            return Result.Failure(TagGroupErrors.DescriptionTooLong());

        if (icon?.Length > 50)
            return Result.Failure(TagGroupErrors.IconTooLong());

        Name = trimmedName;
        Description = description;
        PictureFileName = icon;

        if (displayOrder.HasValue)
            DisplayOrder = displayOrder.Value;

        return Result.Success();
    }

    public Result AddTag(Tag tag)
    {
        if (tag == null)
            return Result.Failure(TagGroupErrors.InvalidTag());

        if (!IsActive)
            return Result.Failure(TagGroupErrors.GroupInactive());

        if (!Tags.Contains(tag))
        {
            Tags.Add(tag);
            tag.AssignToGroup(Id);
        }

        return Result.Success();
    }

    public Result RemoveTag(Tag tag)
    {
        if (tag == null)
            return Result.Failure(TagGroupErrors.InvalidTag());

        if (Tags.Contains(tag))
        {
            Tags.Remove(tag);
            tag.RemoveFromGroup();
        }

        return Result.Success();
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }
}
