using eMeetup.Common.Application.Messaging;
using eMeetup.Modules.Users.Application.Users.Photos.GetPhotos;

namespace eMeetup.Modules.Users.Application.Users.Photos.GetPrimary;

public sealed record GetPrimaryPhotoQuery : IQuery<UserPhotoDto?>;
