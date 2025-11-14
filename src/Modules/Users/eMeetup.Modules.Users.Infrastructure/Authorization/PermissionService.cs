using eMeetup.Common.Application.Authorization;
using eMeetup.Common.Domain;
using eMeetup.Modules.Users.Application.Users.GetUserPermissions;
using MediatR;

namespace eMeetup.Modules.Users.Infrastructure.Authorization;
internal sealed class PermissionService(ISender sender) : IPermissionService
{
    public async Task<Result<PermissionsResponse>> GetUserPermissionsAsync(string identityId)
    {
        return await sender.Send(new GetUserPermissionsQuery(identityId));
    }
}
