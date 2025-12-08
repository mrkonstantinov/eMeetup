using eMeetup.Modules.Users.Domain.Users;

namespace eMeetup.Modules.Users.Application.Users.GetUser;

public sealed class UserResponse
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public DateTime DateOfBirth { get; init; }
    public Gender Gender { get; init; }
    public string? Bio { get; init; }
    public string? ProfilePictureUrl { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public string? City { get; init; }
    public string? Country { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }

    // Коллекции
    public List<UserPhotoResponse> Photos { get; set; } = new();
    public string? Interests { get; set; }
}

public sealed record UserPhotoResponse
{
    public Guid Id { get; init; }
    public string Url { get; init; } = string.Empty;
    public int DisplayOrder { get; init; }
    public bool IsPrimary { get; init; }
}
