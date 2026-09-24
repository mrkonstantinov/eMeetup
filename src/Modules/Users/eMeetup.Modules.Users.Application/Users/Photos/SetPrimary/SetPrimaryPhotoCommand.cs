using eMeetup.Common.Application.Messaging;

namespace eMeetup.Modules.Users.Application.Users.Photos.SetPrimary;

public sealed record SetPrimaryPhotoCommand(Guid PhotoId) : ICommand;
