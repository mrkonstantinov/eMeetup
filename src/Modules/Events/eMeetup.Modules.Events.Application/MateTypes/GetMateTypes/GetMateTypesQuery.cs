using eMeetup.Common.Application.Messaging;

namespace eMeetup.Modules.Events.Application.MateTypes.GetMateTypes;

public sealed record GetMateTypesQuery(Guid SessionId) : IQuery<IReadOnlyCollection<MateTypeResponse>>;

