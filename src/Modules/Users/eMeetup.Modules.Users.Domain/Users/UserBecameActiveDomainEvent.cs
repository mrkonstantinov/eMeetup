using eMeetup.Common.Domain;

namespace eMeetup.Modules.Users.Domain.Users;

public class UserBecameActiveDomainEvent(Guid userId, DateTime lastActive, Location location) : DomainEvent
{
    public Guid UserId { get; init; } = userId;
    public DateTime LastActive { get; init; } = lastActive;
    public Location Location { get; init; } = location;
}
