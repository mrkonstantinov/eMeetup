using eMeetup.Common.Domain;

namespace eMeetup.Modules.Users.Domain.Users;

public class UserProfileUpdatedDomainEvent(Guid userId, string firstName, string lastName, string bio, Location location, DateTime updatedAt) : DomainEvent
{
    public Guid UserId { get; init; } = userId;
    public string FirstName { get; init; } = firstName;
    public string LastName { get; init; } = lastName;
    public string Bio { get; init; } = bio;
    public Location Location { get; init; } = location;
    public DateTime UpdatedAt { get; init; } = updatedAt;

}
