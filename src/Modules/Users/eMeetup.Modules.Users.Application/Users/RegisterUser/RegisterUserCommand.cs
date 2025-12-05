using eMeetup.Common.Application.Messaging;
using eMeetup.Modules.Users.Domain.Users;
using Microsoft.AspNetCore.Http;

namespace eMeetup.Modules.Users.Application.Users.RegisterUser;

public sealed record RegisterUserCommand(
    string Email, 
    string Password, 
    string Username, 
    DateTime DateOfBirth, 
    Gender Gender, 
    string? Bio,
    double? Latitude,
    double? Longitude,
    string? City,
    string? Country,
    string? Interests,
    List<IFormFile>? Photos = null)
    : ICommand<Guid>;

public sealed record PhotoUploadRequest(
    Stream FileStream,
    string FileName,
    string ContentType,
    bool IsPrimary = false,
    int DisplayOrder = 0);
