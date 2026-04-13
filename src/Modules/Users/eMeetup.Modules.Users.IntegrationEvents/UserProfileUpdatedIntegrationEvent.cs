using eMeetup.Common.Application.EventBus;

namespace eMeetup.Modules.Users.IntegrationEvents;

public sealed class UserProfileUpdatedIntegrationEvent : IntegrationEvent
{
    public UserProfileUpdatedIntegrationEvent(
        Guid id,
        DateTime occurredOnUtc,
        Guid userId,
        string userName)
        : base(id, occurredOnUtc)
    {
        UserId = userId;
        UserName = userName;
    }

    public Guid UserId { get; init; }

    public string UserName { get; init; }
}
