using System;
using System.Collections.Generic;
using System.Text;
using eMeetup.Common.Application.Messaging;
using eMeetup.Modules.Events.Application.Events.CreateEvent;

namespace eMeetup.Modules.Events.Application.EventSessions.CreateSession;

public sealed record CreateSessionCommand(
    Guid OrganizerId,
    Guid EventId,
    string Title,
    string Description,
    DateTime StartsAtUtc,
    DateTime? EndsAtUtc,
    string Locality,
    string Address,
    double? Latitude,
    double? Longitude
    ) : ICommand<Guid>;
