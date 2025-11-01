using eMeetup.Common.Application.EventBus;
using eMeetup.Modules.Users.Domain.Users;

namespace eMeetup.Modules.Users.IntegrationEvents;

public sealed class UserRegisteredIntegrationEvent : IntegrationEvent
{
    public UserRegisteredIntegrationEvent(
        Guid id,
        DateTime occurredOnUtc,
        Guid userId,
        string email,
        string userName,
        int? gender,
        DateTime dateOfBirth,
        string bio,
        string? interests)
        : base(id, occurredOnUtc)
    {
        UserId = userId;
        Email = email;
        UserName = userName;
        Gender = gender;
        DateOfBirth = dateOfBirth;
        Bio = bio;
        Interests = interests;
    }

    public Guid UserId { get; init; }

    public string Email { get; init; }

    public string UserName { get; private set; }

    public int? Gender { get; init; }

    public DateTime DateOfBirth { get; init; }

    public string Bio { get; init; }

    public string? Interests { get; init; }

    public Location Location { get; init; }
}
