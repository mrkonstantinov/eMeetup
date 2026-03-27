using eMeetup.Common.Application.Exceptions;
using eMeetup.Common.Infrastructure.Authentication;
using eMeetup.Modules.Events.Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Http;

namespace eMeetup.Modules.Events.Infrastructure.Authentication;

internal sealed class OrganizerContext(IHttpContextAccessor httpContextAccessor) : IOrganizerContext
{
    public Guid OrganizerId => httpContextAccessor.HttpContext?.User.GetUserId() ??
                              throw new EmeetupException("User identifier is unavailable");

    public string OrganizerName => httpContextAccessor.HttpContext?.User.GetUserName() ??
                              throw new EmeetupException("User identifier is unavailable");
}
