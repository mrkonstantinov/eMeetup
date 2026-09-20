using eMeetup.Common.Domain;

namespace eMeetup.Modules.Users.Domain.Users;

public sealed class UserPhotoAddedEvent(Guid userId, string url, bool isPrimary) : DomainEvent
{
    public Guid UserId { get; init; } = userId;
    public string Url { get; init; } = url;
    public bool IsPrimary { get; init; } = isPrimary;
}

public sealed class UserPrimaryPhotoChangedEvent(Guid userId, Guid photoId) : DomainEvent
{
    public Guid UserId { get; init; } = userId;
    public Guid PhotoId { get; init; } = photoId;
}

public sealed class UserPhotoRemovedEvent(Guid userId, Guid photoId) : DomainEvent
{
    public Guid UserId { get; init; } = userId;
    public Guid PhotoId { get; init; } = photoId;
}

