using eMeetup.Common.Application.Messaging;
using static Dapper.SqlMapper;

namespace eMeetup.Modules.Users.Application.Users.GetUser;

public sealed record GetUserQuery(Guid UserId, Guid IdentityId) : IQuery<UserResponse>;
