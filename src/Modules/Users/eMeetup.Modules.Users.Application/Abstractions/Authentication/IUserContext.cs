namespace eMeetup.Modules.Users.Application.Abstractions.Authentication;

public interface IUserContext
{
    Guid UserId { get; }
    string KeycloakId { get; }
    string Email { get; }
    string Username { get; }
}
