using System;
using System.Collections.Generic;
using System.Text;
using eMeetup.Common.Application.Messaging;
using eMeetup.Modules.Users.Domain.Users;
using Microsoft.AspNetCore.Http;

namespace eMeetup.Modules.Users.Application.Users.UpdateUser;

public sealed record UpdateUserPhotosCommand(
    Guid UserId,
    List<UpdatePhotoRequest> Photos, // List of photos to add/reorder
    List<Guid>? RemovePhotoIds = null, // Optional: photos to remove
    Guid? SetPrimaryPhotoId = null) // Optional: set primary photo
    : ICommand<UpdateUserPhotosResult>;

public sealed record UpdatePhotoRequest(
    IFormFile File,
    bool IsPrimary = false,
    int? DisplayOrder = null);

public sealed record UpdateUserPhotosResult(
    Guid UserId,
    int AddedCount,
    int RemovedCount,
    Guid? PrimaryPhotoId,
    List<PhotoInfo> Photos);

public sealed record PhotoInfo(
    Guid Id,
    string Url,
    bool IsPrimary,
    int DisplayOrder);

