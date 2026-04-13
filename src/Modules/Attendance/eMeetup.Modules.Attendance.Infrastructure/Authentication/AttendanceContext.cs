using eMeetup.Common.Application.Exceptions;
using eMeetup.Common.Infrastructure.Authentication;
using eMeetup.Modules.Attendance.Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Http;

namespace eMeetup.Modules.Attendance.Infrastructure.Authentication;

internal sealed class AttendanceContext(IHttpContextAccessor httpContextAccessor) : IAttendanceContext
{
    public Guid AttendeeId => httpContextAccessor.HttpContext?.User.GetUserId() ??
                              throw new EmeetupException("User identifier is unavailable");
}
