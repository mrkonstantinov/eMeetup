using eMeetup.Common.Domain;

namespace eMeetup.Modules.Users.Domain.Users;

public sealed class UserRegisteredDomainEvent(Guid userId, Guid identityId) : DomainEvent
{
    public Guid UserId { get; init; } = userId;
    public Guid IdentityId { get; init; } = identityId;
}
