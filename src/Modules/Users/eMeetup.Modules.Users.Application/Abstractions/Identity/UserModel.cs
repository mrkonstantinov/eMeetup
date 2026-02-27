using eMeetup.Modules.Users.Domain.Users;
using Microsoft.AspNetCore.Http;

namespace eMeetup.Modules.Users.Application.Abstractions.Identity;

public sealed record UserModel(
    string Email, 
    string Password, 
    string Username, 
    DateTime DateOfBirth, 
    Gender Gender);


public sealed record UpdateUserModel(
    string IdentityId,
    string? Bio,
    double? Latitude,
    double? Longitude,
    string? City,
    string? Street,
    string? Interests,
    string? ProfilePictureUrl,
    List<IFormFile>? Photos = null
    );


public sealed record UserProfileModel(
    Guid IdentityId,
    string Email,
    string UserName,
    DateTime DateOfBirth,
    Gender Gender,
    string? Bio,
    double? Latitude,
    double? Longitude,
    string? City,
    string? Street,
    string? Interests,
    string? ProfilePictureUrl
    );
