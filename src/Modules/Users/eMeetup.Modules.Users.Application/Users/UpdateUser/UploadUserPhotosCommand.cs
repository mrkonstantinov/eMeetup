using System;
using System.Collections.Generic;
using System.Text;
using eMeetup.Common.Application.Messaging;
using eMeetup.Modules.Users.Domain.Users;
using Microsoft.AspNetCore.Http;

namespace eMeetup.Modules.Users.Application.Users.UpdateUser;

public sealed record UploadUserPhotosCommand(
    Guid IdentityId,
    List<IFormFile>? Photos) : ICommand;
