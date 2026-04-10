using System.Data.Common;
using Dapper;
using eMeetup.Common.Application.Data;
using eMeetup.Common.Application.Messaging;
using eMeetup.Common.Domain;
using eMeetup.Modules.Events.Domain.Events;

namespace eMeetup.Modules.Events.Application.Events.GetEvent;

internal sealed class GetEventQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetEventQuery, EventResponse>
{
    public async Task<Result<EventResponse>> Handle(GetEventQuery request, CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        const string sql =
            $"""
             SELECT
                e.id AS {nameof(EventResponse.Id)},
                e.organizer_id AS {nameof(EventResponse.OrganizerId)},
                e.organizer_name AS {nameof(EventResponse.OrganizerName)},
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
             WHERE e.id = @EventId
             GROUP BY e.id, e.organizer_id, e.organizer_name, e.title, 
                      e.description, e.url, e.is_archived
             """;

        EventResponse? category = await connection.QuerySingleOrDefaultAsync<EventResponse>(sql, request);

        if (category is null)
        {
            return Result.Failure<EventResponse>(EventErrors.NotFound(request.EventId));
        }

        return category;
    }
}
