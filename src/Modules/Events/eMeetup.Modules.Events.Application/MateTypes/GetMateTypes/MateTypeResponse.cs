using eMeetup.Modules.Events.Domain.Events;

namespace eMeetup.Modules.Events.Application.MateTypes.GetMateTypes;

public sealed record MateTypeResponse(
    Guid CreatorId,
    Guid SessionId,
    string Title,
    string? Description,
    int AllocatedSlots,
    int? MinAge,
    int? MaxAge,
    Gender? Gender,
    Gender? PreferredGender,
    string? PreferredAgeRange,
    int Priority);

