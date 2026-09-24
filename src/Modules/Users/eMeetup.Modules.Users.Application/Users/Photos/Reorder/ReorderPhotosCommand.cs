using eMeetup.Common.Application.Messaging;

namespace eMeetup.Modules.Users.Application.Users.Photos.Reorder;

public sealed record ReorderPhotosCommand(Dictionary<Guid, int> OrderMap) : ICommand;
