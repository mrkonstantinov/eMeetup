using eMeetup.Common.Application.Messaging;

namespace eMeetup.Modules.Events.Application.ActivityCategories.GetActivityCategory;

public sealed record GetActivityCategoryQuery(
    bool IncludeInactive = false,
    bool OnlyRoots = false)
    : IQuery<IReadOnlyCollection<ActivityCategoryResponse>>;
