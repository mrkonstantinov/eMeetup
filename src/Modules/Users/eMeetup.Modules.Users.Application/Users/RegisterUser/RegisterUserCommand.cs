using eMeetup.Common.Application.Messaging;

namespace eMeetup.Modules.Users.Application.Users.RegisterUser;

public sealed record RegisterUserCommand(
    string Email, 
    string Password, 
    string Username
    ): ICommand<Guid>;
