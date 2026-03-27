using eMeetup.Common.Application.Messaging;

namespace eMeetup.Modules.Events.Application.Events.GetEvents;

public sealed record GetEventQuery(Guid UserId) : IQuery<IReadOnlyCollection<EventResponse>>;

