using eMeetup.Common.Presentation.Endpoints;
using eMeetup.Common.Presentation.Results;
using eMeetup.Modules.Events.Application.ActivityCategories.GetActivityCategory;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace eMeetup.Modules.Events.Presentation.ActivityCategories;

internal sealed class GetActivityCategoryTree : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("events/activity-categories/tree", async (
            bool? includeInactive,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(
                new GetActivityCategoryTreeQuery(
                    IncludeInactive: includeInactive ?? false),
                ct);

            return result.Match(
                tree => Results.Ok(tree),
                error => ApiResults.Problem(error));
        })
        .AllowAnonymous()
        .WithTags(Tags.ActivityCategories)
        .WithName("GetActivityCategoryTree");
    }
}
