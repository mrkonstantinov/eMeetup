using eMeetup.Modules.Users.Domain.Users;

namespace eMeetup.Modules.Users.Presentation;

internal static class Permissions
{
    internal const string GetUser = "users:read";
    internal const string ModifyUser = "users:update";
    internal const string GetTags = "tags:read";
}
