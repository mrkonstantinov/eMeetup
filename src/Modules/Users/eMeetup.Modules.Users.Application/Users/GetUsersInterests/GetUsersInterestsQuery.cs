using eMeetup.Common.Application.Messaging;

namespace eMeetup.Modules.Users.Application.Users.GetUsersInterests;

public sealed record GetUsersInterestsQuery() : IQuery<IReadOnlyCollection<UsersInterestResponse>>;
