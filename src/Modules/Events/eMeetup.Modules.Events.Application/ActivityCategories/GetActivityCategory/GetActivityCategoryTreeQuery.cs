using eMeetup.Common.Application.Messaging;

namespace eMeetup.Modules.Events.Application.ActivityCategories.GetActivityCategory;

public sealed record GetActivityCategoryTreeQuery(
    bool IncludeInactive = false)
    : IQuery<IReadOnlyCollection<ActivityCategoryTreeResponse>>;
