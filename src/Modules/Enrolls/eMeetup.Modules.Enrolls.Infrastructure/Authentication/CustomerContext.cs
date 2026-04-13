using eMeetup.Common.Application.Exceptions;
using eMeetup.Common.Infrastructure.Authentication;
using eMeetup.Modules.Enrolls.Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Http;

namespace eMeetup.Modules.Enrolls.Infrastructure.Authentication;

internal sealed class CustomerContext(IHttpContextAccessor httpContextAccessor) : ICustomerContext
{
    public Guid CustomerId => httpContextAccessor.HttpContext?.User.GetUserId() ??
                              throw new EmeetupException("User identifier is unavailable");
}
