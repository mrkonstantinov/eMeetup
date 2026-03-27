using eMeetup.Common.Application.Messaging;

namespace eMeetup.Modules.Events.Application.Events.GetTags;

public sealed record GetTagsQuery() : IQuery<IReadOnlyCollection<TagGroupResponse>>;
