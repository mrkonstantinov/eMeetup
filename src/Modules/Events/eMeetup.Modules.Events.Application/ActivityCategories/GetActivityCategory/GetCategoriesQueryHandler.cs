using System.Data.Common;
using Dapper;
using eMeetup.Common.Application.Data;
using eMeetup.Common.Application.Messaging;
using eMeetup.Common.Domain;
using eMeetup.Modules.Events.Domain.ActivityCategories;

namespace eMeetup.Modules.Events.Application.ActivityCategories.GetActivityCategory;

internal sealed class GetActivityCategoriesQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetActivityCategoryQuery, IReadOnlyCollection<ActivityCategoryResponse>>
{
    public async Task<Result<IReadOnlyCollection<ActivityCategoryResponse>>> Handle(
        GetActivityCategoryQuery request,
        CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        // ============================================================
        // SQL для плоского списка
        // ============================================================
        var sql = """
            SELECT
                c.id            AS Id,
                c.name          AS Name,
                c.slug          AS Slug,
                c.icon          AS Icon,
                c.color         AS Color,
                c.parent_id     AS ParentId,
                p.name          AS ParentName,
                c.display_order AS DisplayOrder
            FROM events.activity_categories c
            LEFT JOIN events.activity_categories p ON p.id = c.parent_id
            WHERE 1 = 1
            """;

        var parameters = new DynamicParameters();

        if (!request.IncludeInactive)
        {
            sql += " AND c.status = @ActiveStatus";
            parameters.Add("ActiveStatus", (int)ActivityCategoryStatus.Active);
        }

        if (request.OnlyRoots)
        {
            sql += " AND c.parent_id IS NULL";
        }

        sql += " ORDER BY c.display_order, c.name";

        List<ActivityCategoryResponse> categories = (await connection
            .QueryAsync<ActivityCategoryResponse>(sql, parameters))
            .AsList();

        return categories;
    }
}
