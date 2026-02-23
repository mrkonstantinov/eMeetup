using System.Data.Common;
using Dapper;
using eMeetup.Common.Application.Data;
using eMeetup.Common.Application.Messaging;
using eMeetup.Common.Domain;

namespace eMeetup.Modules.Users.Application.Users.GetUsersInterests;

internal sealed class GetUsersInterestsQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetUsersInterestsQuery, IReadOnlyCollection<UsersInterestResponse>>
{
    public async Task<Result<IReadOnlyCollection<UsersInterestResponse>>> Handle(
        GetUsersInterestsQuery request,
        CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        const string sql =
            $"""
             SELECT
                 id AS {nameof(UsersInterestResponse.Id)},
                 name AS {nameof(UsersInterestResponse.Name)},
                 slug AS {nameof(UsersInterestResponse.Slug)},
                 usage_count AS {nameof(UsersInterestResponse.UsageCount)}
             FROM users.tags
             """;

        List<UsersInterestResponse> events = (await connection.QueryAsync<UsersInterestResponse>(sql, request)).AsList();

        return events;
    }
}
