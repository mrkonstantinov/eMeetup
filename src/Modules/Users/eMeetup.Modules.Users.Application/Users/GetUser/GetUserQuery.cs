using eMeetup.Common.Application.Messaging;

namespace eMeetup.Modules.Users.Application.Users.GetUser;

public sealed record GetUserQuery(Guid UserId) : IQuery<UserResponse>;
