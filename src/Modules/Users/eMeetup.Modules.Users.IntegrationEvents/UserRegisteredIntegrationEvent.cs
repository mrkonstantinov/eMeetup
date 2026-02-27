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
        Gender gender,
        DateTime dateOfBirth,
        string? bio,
        double? latitude,
        double? longitude,
        string? city,
        string? street)
        : base(id, occurredOnUtc)
    {
        UserId = userId;
        Email = email;
        UserName = userName;
        Gender = gender;
        DateOfBirth = dateOfBirth;
        Bio = bio;
        Latitude = latitude;
        Longitude = longitude;
        City = city;
        Street = street;
    }

    public Guid UserId { get; init; }
    public string Email { get; init; }
    public string UserName { get; private set; }
    public Gender Gender { get; init; }
    public DateTime DateOfBirth { get; init; }
    public string? Bio { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public string? City { get; init; }
    public string? Street { get; init; }
}
