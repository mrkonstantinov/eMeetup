using eMeetup.Common.Application.Messaging;
using eMeetup.Modules.Events.Application.Events.GetEvents;

namespace eMeetup.Modules.Events.Application.Events.GetEvent;

public sealed record GetEventQuery(Guid EventId) : IQuery<EventResponse>;
