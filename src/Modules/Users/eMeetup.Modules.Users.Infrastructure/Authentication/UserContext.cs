using System.Security.Claims;
using eMeetup.Common.Application.Exceptions;
using eMeetup.Common.Infrastructure.Authentication;
using eMeetup.Modules.Users.Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Http;

namespace eMeetup.Modules.Users.Infrastructure.Authentication;

internal sealed class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    public Guid UserId =>
            httpContextAccessor.HttpContext?.User.GetUserId()
            ?? throw new EmeetupException("User identifier is unavailable");

    public string KeycloakId =>
        httpContextAccessor.HttpContext?.User
            .FindFirst("sub")?.Value
        ?? throw new EmeetupException("Keycloak identifier is unavailable");

    public string Email =>
        httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.Email)?.Value
        ?? throw new EmeetupException("Email is unavailable");

    public string Username =>
        httpContextAccessor.HttpContext?.User
            .FindFirst("preferred_username")?.Value
        ?? throw new EmeetupException("Username is unavailable");
}

