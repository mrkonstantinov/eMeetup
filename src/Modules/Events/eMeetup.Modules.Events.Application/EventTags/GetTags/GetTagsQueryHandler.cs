using System.Data.Common;
using Dapper;
using eMeetup.Common.Application.Data;
using eMeetup.Common.Application.Messaging;
using eMeetup.Common.Domain;
using eMeetup.Modules.Events.Domain.TagGroups;

namespace eMeetup.Modules.Events.Application.EventTags.GetTags;

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
                 tg.id AS TagGroupId,
                 tg.name AS TagGroupName,
                 tg.picture_file_name AS PictureFileName,
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
                x.TagGroupId,
                TagGroupName = string.IsNullOrEmpty(x.TagGroupName) ? "Uncategorized" : x.TagGroupName,
                x.PictureFileName,
                x.Id,
                x.Name,
                x.Slug,
                x.UsageCount
            })
            .GroupBy(x => new { x.TagGroupId, x.TagGroupName, x.PictureFileName })
            .Select(g => new
            {
                g.Key.TagGroupId,
                g.Key.TagGroupName,
                g.Key.PictureFileName,
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
                TagGroupId: g.TagGroupId,
                TagGroupName: g.TagGroupName,
                PictureFileName: g.PictureFileName,
                Tags: g.Tags
            ))
            .ToList()
            .AsReadOnly();

        return groupedResults;
    }
}
