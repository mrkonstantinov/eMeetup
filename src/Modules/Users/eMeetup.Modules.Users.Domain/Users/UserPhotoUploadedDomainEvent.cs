using eMeetup.Common.Domain;

namespace eMeetup.Modules.Users.Domain.Users;

public class UserPhotoUploadedDomainEvent(Guid userId, Guid photoId, string imageUrl, bool isPrimary, DateTime uploadedAt) : DomainEvent
{
    public Guid UserId { get; init; } = userId;
    public Guid PhotoId { get; init; } = photoId;
    public string ImageUrl { get; init; } = imageUrl;
    public bool IsPrimary { get; init; } = isPrimary;
    public DateTime UploadedAt { get; init; } = uploadedAt;
}
