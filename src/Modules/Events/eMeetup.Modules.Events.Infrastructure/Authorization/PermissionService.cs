using System;
using System.Collections.Generic;
using System.Text;
using eMeetup.Common.Application.Authorization;
using eMeetup.Common.Domain;
using MediatR;

namespace eMeetup.Modules.Events.Infrastructure.Authorization;

//internal sealed class PermissionService(ISender sender) : IPermissionService
//{
//    public async Task<Result<PermissionsResponse>> GetUserPermissionsAsync(string identityId)
//    {
//        return await sender.Send(new GetUserPermissionsQuery(identityId));
//    }
//}
