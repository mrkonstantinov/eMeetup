using eMeetup.Common.Application.Messaging;
using eMeetup.Modules.Users.Domain.Users;
using Microsoft.AspNetCore.Http;

namespace eMeetup.Modules.Users.Application.Users.UpdateUser;

public sealed record UpdateUserCommand(
    Guid IdentityId,
    string? Bio,
    double? Latitude,
    double? Longitude,
    string? City,
    string? Country,
    string? Interests) : ICommand;
