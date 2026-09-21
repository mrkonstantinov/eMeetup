using eMeetup.Common.Application.EventBus;

namespace eMeetup.Modules.Users.IntegrationEvents;

public sealed class UserRegisteredIntegrationEvent : IntegrationEvent
{
    public UserRegisteredIntegrationEvent(
        Guid id,
        DateTime occurredOnUtc,
        Guid userId,
        string email,
        string userName
        ): base(id, occurredOnUtc)
    {
        UserId = userId;
        Email = email;
        UserName = userName;
    }

    public Guid UserId { get; init; }
    public string Email { get; init; }
    public string UserName { get; private set; }

    //public string? Locality { get; init; }
    //public string? Street { get; init; }
}
