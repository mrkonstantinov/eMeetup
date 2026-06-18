using System.Data.Common;
using Dapper;
using eMeetup.Common.Application.Data;
using eMeetup.Common.Application.Messaging;
using eMeetup.Common.Domain;

namespace eMeetup.Modules.Events.Application.EventSessions.GetSessions;

internal sealed class GetSessionsQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetSessionsQuery, IReadOnlyCollection<SessionResponse>>
{
    public async Task<Result<IReadOnlyCollection<SessionResponse>>> Handle(
        GetSessionsQuery request,
        CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        const string sql =
            $"""
             SELECT
                events.event_sessions.id AS {nameof(SessionResponse.Id)},
                events.events.creator_id AS {nameof(SessionResponse.CreatorId)},
                event_id AS {nameof(SessionResponse.EventId)},
                events.event_sessions.title AS {nameof(SessionResponse.Title)},
                events.event_sessions.description AS {nameof(SessionResponse.Description)},                 
                starts_at_utc AS {nameof(SessionResponse.StartsAtUtc)},
                ends_at_utc AS {nameof(SessionResponse.EndsAtUtc)},
                locality AS {nameof(SessionResponse.Locality)},
                address AS {nameof(SessionResponse.Address)},
                latitude AS {nameof(SessionResponse.Latitude)},
                longitude AS {nameof(SessionResponse.Longitude)}
             FROM events.event_sessions
             JOIN events.events ON events.event_sessions.event_id = events.events.id
             """;

        try
        {
            List<SessionResponse> events = (await connection.QueryAsync<SessionResponse>(sql, request)).AsList();
            return events;
        }
        catch (Exception e)
        {
            ;
        }
        return null;

    }
}
