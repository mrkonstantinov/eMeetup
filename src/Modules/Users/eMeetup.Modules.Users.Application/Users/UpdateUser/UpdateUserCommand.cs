using eMeetup.Common.Application.Messaging;
using eMeetup.Modules.Users.Domain.Users;
using Microsoft.AspNetCore.Http;

namespace eMeetup.Modules.Users.Application.Users.UpdateUser;

public sealed record UpdateUserCommand(
    Guid IdentityId,
    string? Locality,
    string? Street,
    string? Bio,
    string? Interests) : ICommand;
