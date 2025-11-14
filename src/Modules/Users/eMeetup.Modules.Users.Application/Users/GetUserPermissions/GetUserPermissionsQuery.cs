using eMeetup.Common.Application.Authorization;
using eMeetup.Common.Application.Messaging;

namespace eMeetup.Modules.Users.Application.Users.GetUserPermissions;
public sealed record GetUserPermissionsQuery(string IdentityId) : IQuery<PermissionsResponse>;
