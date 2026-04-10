using eMeetup.Common.Application.Messaging;

namespace eMeetup.Modules.Events.Application.EventSessions.GetSessions;

public sealed record GetSessionsQuery : IQuery<IReadOnlyCollection<SessionResponse>>;

