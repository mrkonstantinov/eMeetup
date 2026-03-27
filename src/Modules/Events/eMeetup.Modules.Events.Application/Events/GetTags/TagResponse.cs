namespace eMeetup.Modules.Events.Application.Events.GetTags;

public sealed record TagResponse(
    Guid Id,
    string Name,
    string Slug,
    int UsageCount);

public sealed record TagGroupResponse(
    string TagGroupName,
    IReadOnlyCollection<TagResponse> Tags);


public sealed record TagWithGroupDto
(
    string TagGroupName,
    Guid Id,
    string Name,
    string Slug,
    int UsageCount
);
