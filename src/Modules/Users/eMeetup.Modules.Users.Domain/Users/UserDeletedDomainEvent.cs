using eMeetup.Common.Domain;

namespace eMeetup.Modules.Users.Domain.Users;

public class UserDeletedDomainEvent(Guid userId, string reason, DateTime deletedAt) : DomainEvent
{
    public Guid UserId { get; init; } = userId;
    public string Reason { get; init; } = reason;
    public DateTime DeletedAt { get; init; } = deletedAt;
}
