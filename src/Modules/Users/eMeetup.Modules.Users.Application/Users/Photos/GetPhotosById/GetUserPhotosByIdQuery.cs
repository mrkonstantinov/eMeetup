using eMeetup.Common.Application.Messaging;
using eMeetup.Modules.Users.Application.Users.Photos.GetPhotos;

namespace eMeetup.Modules.Users.Application.Users.Photos.GetPhotosById;

public sealed record GetUserPhotosByIdQuery(Guid UserId) : IQuery<UserPhotosResponse>;
