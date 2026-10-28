using System.Data.Common;
using Dapper;
using eMeetup.Common.Application.Data;
using eMeetup.Common.Application.Messaging;
using eMeetup.Common.Domain;
using eMeetup.Modules.Events.Domain.ActivityCategories;

namespace eMeetup.Modules.Events.Application.ActivityCategories.GetActivityCategory;

internal sealed class GetActivityCategoryTreeQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetActivityCategoryTreeQuery, IReadOnlyCollection<ActivityCategoryTreeResponse>>
{
    public async Task<Result<IReadOnlyCollection<ActivityCategoryTreeResponse>>> Handle(
        GetActivityCategoryTreeQuery request,
        CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        // ============================================================
        // SQL для дерева (загружаем все категории, строим в памяти)
        // ============================================================
        var sql = """
            SELECT
                c.id            AS Id,
                c.name          AS Name,
                c.slug          AS Slug,
                c.icon          AS Icon,
                c.color         AS Color,
                c.parent_id     AS ParentId,
                c.display_order AS DisplayOrder
            FROM events.activity_categories c
            WHERE 1 = 1
            """;

        var parameters = new DynamicParameters();

        if (!request.IncludeInactive)
        {
            sql += " AND c.status = @ActiveStatus";
            parameters.Add("ActiveStatus", (int)ActivityCategoryStatus.Active);
        }

        sql += " ORDER BY c.display_order, c.name";

        // ============================================================
        // Плоский список → дерево
        // ============================================================
        var flatCategories = (await connection
            .QueryAsync<ActivityCategoryFlatRow>(sql, parameters))
            .ToList();

        var tree = BuildTree(flatCategories, null);

        return tree;
    }

    // ================================================================
    // === TREE BUILDING ===
    // ================================================================

    private static List<ActivityCategoryTreeResponse> BuildTree(
        List<ActivityCategoryFlatRow> categories,
        int? parentId)
    {
        return categories
            .Where(c => c.ParentId == parentId)
            .Select(c => new ActivityCategoryTreeResponse(
                Id: c.Id,
                Name: c.Name,
                Slug: c.Slug,
                Icon: c.Icon,
                Color: c.Color,
                ParentId: c.ParentId,
                DisplayOrder: c.DisplayOrder,
                Children: BuildTree(categories, c.Id)))
            .ToList();
    }

    // ================================================================
    // === ВНУТРЕННИЙ DTO ДЛЯ DAPPER ===
    // ================================================================

    private sealed class ActivityCategoryFlatRow
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Slug { get; init; } = string.Empty;
        public string? Icon { get; init; }
        public string? Color { get; init; }
        public int? ParentId { get; init; }
        public int DisplayOrder { get; init; }
    }
}
