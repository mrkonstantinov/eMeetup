using eMeetup.Common.Application.Messaging;
using eMeetup.Common.Domain;
using MediatR;

namespace eMeetup.Modules.Users.Application.Users.Photos.AddPhoto;

public sealed record AddUserPhotoCommand(
    Stream FileStream,
    string FileName,
    string ContentType,
    bool IsPrimary = false) : ICommand<Guid>;
