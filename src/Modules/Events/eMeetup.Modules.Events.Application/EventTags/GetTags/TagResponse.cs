namespace eMeetup.Modules.Events.Application.EventTags.GetTags;

public sealed record TagResponse(
    Guid Id,
    string Name,
    string Slug,
    int UsageCount);

public sealed record TagGroupResponse(
    int TagGroupId,
    string TagGroupName,
    string PictureFileName,
    IReadOnlyCollection<TagResponse> Tags);


public sealed record TagWithGroupDto
(
    int TagGroupId,
    string TagGroupName,
    string PictureFileName,
    Guid Id,
    string Name,
    string Slug,
    int UsageCount
);
