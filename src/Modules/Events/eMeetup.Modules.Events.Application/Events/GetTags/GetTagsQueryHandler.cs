using System.Data.Common;
using Dapper;
using eMeetup.Common.Application.Data;
using eMeetup.Common.Application.Messaging;
using eMeetup.Common.Domain;

namespace eMeetup.Modules.Events.Application.Events.GetTags;

internal sealed class GetTagsQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetTagsQuery, IReadOnlyCollection<TagGroupResponse>>
{
    public async Task<Result<IReadOnlyCollection<TagGroupResponse>>> Handle(
        GetTagsQuery request,
        CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        const string sql =
            $"""
             SELECT 
                 tg.name AS TagGroupName,
                 t.id AS Id,
                 t.name AS Name,
                 t.slug AS Slug,
                 t.usage_count AS UsageCount
             FROM events.tags t
             LEFT JOIN events.tag_groups tg ON t.tag_group_id = tg.id
             WHERE t.is_active = true
             """;

        var results = await connection.QueryAsync<TagWithGroupDto>(sql, request);

        var groupedResults = results
            .Select(x => new
            {
                TagGroupName = string.IsNullOrEmpty(x.TagGroupName) ? "Uncategorized" : x.TagGroupName,
                x.Id,
                x.Name,
                x.Slug,
                x.UsageCount
            })
            .GroupBy(x => x.TagGroupName)
            .Select(g => new
            {
                TagGroupName = g.Key,
                TotalUsage = g.Sum(x => x.UsageCount),
                Tags = g.Select(x => new TagResponse(
                    Id: x.Id,
                    Name: x.Name,
                    Slug: x.Slug,
                    UsageCount: x.UsageCount
                ))
                .OrderByDescending(t => t.UsageCount)
                .ThenBy(t => t.Name, StringComparer.OrdinalIgnoreCase)
                .ToList()
            })
            .OrderByDescending(g => g.TotalUsage)  // Сначала по сумме DESC
            .ThenBy(g => g.TagGroupName, StringComparer.OrdinalIgnoreCase)  // Затем по алфавиту
            .Select(g => new TagGroupResponse(
                TagGroupName: g.TagGroupName,
                Tags: g.Tags
            ))
            .ToList()
            .AsReadOnly();

        return groupedResults;
    }
}
