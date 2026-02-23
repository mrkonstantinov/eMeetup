using System.Data.Common;
using Dapper;
using eMeetup.Common.Application.Data;
using eMeetup.Common.Application.Messaging;
using eMeetup.Common.Domain;

namespace eMeetup.Modules.Users.Application.Users.GetUsersInterests;

public sealed record UsersInterestResponse(
    Guid Id,
    string Name,
    string Slug,
    int UsageCount);
