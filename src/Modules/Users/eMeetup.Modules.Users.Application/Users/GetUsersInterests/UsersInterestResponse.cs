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

internal sealed class GetEventsQueryHandler(IDbConnectionFactory dbConnectionFactory)
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
                 category_id AS {nameof(UsersInterestResponse.Name)},
                 title AS {nameof(UsersInterestResponse.Name)},
                 description AS {nameof(UsersInterestResponse.Slug)}
             FROM users.tags
             """;

        List<UsersInterestResponse> events = (await connection.QueryAsync<UsersInterestResponse>(sql, request)).AsList();

        return events;
    }
}
