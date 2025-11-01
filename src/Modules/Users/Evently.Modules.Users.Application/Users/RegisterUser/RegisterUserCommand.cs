using eMeetup.Common.Application.Messaging;
using eMeetup.Modules.Users.Domain.Users;

namespace eMeetup.Modules.Users.Application.Users.RegisterUser;

public sealed record RegisterUserCommand(string Email, string Password, string Username, DateTime DateOfBirth, Gender Gender, string Bio, string ProfilePictureUrl, Location Location)
    : ICommand<Guid>;
