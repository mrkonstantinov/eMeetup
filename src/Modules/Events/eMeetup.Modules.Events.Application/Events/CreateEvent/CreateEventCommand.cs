using System;
using System.Collections.Generic;
using System.Text;
using eMeetup.Common.Application.Messaging;
using eMeetup.Modules.Events.Domain.Events;

namespace eMeetup.Modules.Events.Application.Events.CreateEvent;

public sealed record CreateEventCommand(
    Guid CreatorId,
    string Title,
    string? Description,
    string? Url,
    //double? Latitude,
    //double? Longitude,
    //string? City,
    //string? Street,
    string? Tags
    ) : ICommand<Guid>;
