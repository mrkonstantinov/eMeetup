using System;
using System.Collections.Generic;
using System.Text;
using eMeetup.Common.Application.Clock;
using eMeetup.Common.Application.Messaging;
using eMeetup.Modules.Events.Application.EventSessions.CreateSession;
using eMeetup.Modules.Events.Domain.Events;
using eMeetup.Modules.Events.Domain.EventSessions;

namespace eMeetup.Modules.Events.Application.MateTypes.CreateMateType;

public sealed record CreateMateTypeCommand(
    Guid OrganizerId,
    Guid EventSessionId,
    string Title,
    string? Description,
    int AllocatedSlots,
    int? MinAge,
    int? MaxAge,
    Gender? Gender,
    Gender? PreferredGender,
    string? PreferredAgeRange,
    int Priority = 0
    ) : ICommand<Guid>;
