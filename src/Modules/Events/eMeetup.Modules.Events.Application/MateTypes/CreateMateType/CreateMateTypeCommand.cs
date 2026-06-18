using eMeetup.Common.Application.Messaging;
using eMeetup.Modules.Events.Domain.Events;

namespace eMeetup.Modules.Events.Application.MateTypes.CreateMateType;

public sealed record CreateMateTypeCommand(
    Guid CreatorId,
    Guid SessionId,
    string Title,
    string? Description,
    int AllocatedSlots,
    decimal? Budget,
    int? MinAge,
    int? MaxAge,
    Gender? Gender,
    Gender? PreferredGender,
    string? PreferredAgeRange,
    int Priority = 0
    ) : ICommand<Guid>;
