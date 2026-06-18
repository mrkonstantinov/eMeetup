using eMeetup.Common.Application.EventBus;

namespace eMeetup.Modules.Users.IntegrationEvents;

public sealed class UserRegisteredIntegrationEvent : IntegrationEvent
{
    public UserRegisteredIntegrationEvent(
        Guid id,
        DateTime occurredOnUtc,
        Guid userId,
        string email,
        string userName,
        DateTime dateOfBirth,
        int gender
        //string? bio,
        //string? locality,
        //string? street
        )
        : base(id, occurredOnUtc)
    {
        UserId = userId;
        Email = email;
        UserName = userName;
        Gender = gender;
        DateOfBirth = dateOfBirth;
        //Bio = bio;
        //Locality = locality;
        //Street = street;
    }

    public Guid UserId { get; init; }
    public string Email { get; init; }
    public string UserName { get; private set; }
    public DateTime DateOfBirth { get; init; }
    public int Gender { get; init; }
    public string? Bio { get; init; }
    public string? Locality { get; init; }
    public string? Street { get; init; }
}
