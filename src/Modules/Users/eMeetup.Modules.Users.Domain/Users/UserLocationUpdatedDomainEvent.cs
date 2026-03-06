using eMeetup.Common.Domain;

namespace eMeetup.Modules.Users.Domain.Users;

public class UserLocationUpdatedDomainEvent(Guid userId, string location, DateTime updatedAt) : DomainEvent
{
    public Guid UserId { get; init; } = userId;    
    public string Location { get; init; } = location;
    public DateTime UpdatedAt { get; init; } = updatedAt;
}
