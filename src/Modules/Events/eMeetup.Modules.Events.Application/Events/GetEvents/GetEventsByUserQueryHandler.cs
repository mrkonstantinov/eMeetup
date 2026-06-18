using System.Data.Common;
using Dapper;
using eMeetup.Common.Application.Data;
using eMeetup.Common.Application.Messaging;
using eMeetup.Common.Domain;

namespace eMeetup.Modules.Events.Application.Events.GetEvents;

internal sealed class GetEventsByUserQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetEventQuery, IReadOnlyCollection<EventResponse>>
{
    public async Task<Result<IReadOnlyCollection<EventResponse>>> Handle(GetEventQuery request, CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        const string sql =
            $"""
             SELECT
                e.id AS {nameof(EventResponse.Id)},
                e.creator_id AS {nameof(EventResponse.CreatorId)},
                e.title AS {nameof(EventResponse.Title)},
                e.description AS {nameof(EventResponse.Description)},
                e.url AS {nameof(EventResponse.Url)},
                e.is_archived AS {nameof(EventResponse.IsArchived)},
                COALESCE(
                    string_agg('#' || t.name, ', ' ORDER BY t.usage_count DESC, t.name ASC), 
                    ''
                ) AS {nameof(EventResponse.Tags)}
                 
             FROM events.events e
             LEFT JOIN events.event_tags et ON e.id = et.event_id
             LEFT JOIN events.tags t ON et.tag_id = t.id
             WHERE e.organizer_id = @UserId
             GROUP BY e.id, e.organizer_id, e.organizer_name, e.title, 
                      e.description, e.url, e.is_archived
             ORDER BY e.created_at DESC
             """;

        IEnumerable<EventResponse> events = await connection.QueryAsync<EventResponse>(sql, new { request.UserId });

        IReadOnlyCollection<EventResponse> result = events.ToList();

        return Result.Success(result);
    }
}

