using eMeetup.Common.Application.Messaging;

namespace eMeetup.Modules.Users.Application.Users.Photos.Remove;

public sealed record RemoveUserPhotoCommand(Guid PhotoId) : ICommand;
