using eMeetup.Common.Application.Messaging;

namespace eMeetup.Modules.Events.Application.EventTags.GetTags;

public sealed record GetTagsQuery() : IQuery<IReadOnlyCollection<TagGroupResponse>>;
