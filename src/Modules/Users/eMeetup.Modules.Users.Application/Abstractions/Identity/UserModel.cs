using eMeetup.Modules.Users.Domain.Users;

namespace eMeetup.Modules.Users.Application.Abstractions.Identity;

public sealed record UserModel(string Email, string Password, string Username, DateTime DateOfBirth, Gender Gender, string Bio, string ProfilePictureUrl, Location Location);





public sealed record UserProfileModel(string IdentityId, string Email, int? Gender, DateTime? DateOfBirth, string? ProfilePhotoUrl, string? Bio, string? Interests, string? Location);
