using eMeetup.Common.Application.Messaging;

namespace eMeetup.Modules.Events.Application.MateTypes.GetMateTypes;

public sealed record GetMateTypesQuery(Guid EventSessionId) : IQuery<IReadOnlyCollection<MateTypeResponse>>;

