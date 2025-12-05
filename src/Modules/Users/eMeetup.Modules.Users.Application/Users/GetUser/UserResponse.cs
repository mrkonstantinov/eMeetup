using eMeetup.Modules.Users.Domain.Users;

namespace eMeetup.Modules.Users.Application.Users.GetUser;

public sealed record UserResponse(
    Guid Id, 
    string Email, 
    string UserName,
    DateTime DateOfBirth,
    Gender Gender,
    string? Bio,
    string? ProfilePictureUrl,
    double? Latitude, 
    double? Longitude, 
    string? City, 
    string? Country, 
    DateTime CreatedAt, 
    DateTime? UpdatedAt,
    List<UserPhotoResponse> Photos
    );

public sealed record UserPhotoResponse
{
    public Guid Id { get; init; }
    public string Url { get; init; } = string.Empty;
    public int DisplayOrder { get; init; }
    public bool IsPrimary { get; init; }
}
