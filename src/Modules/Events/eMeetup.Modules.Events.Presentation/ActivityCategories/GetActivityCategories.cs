using eMeetup.Common.Presentation.Endpoints;
using eMeetup.Common.Presentation.Results;
using eMeetup.Modules.Events.Application.ActivityCategories.GetActivityCategory;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace eMeetup.Modules.Events.Presentation.ActivityCategories;

internal sealed class GetActivityCategories : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("events/activity-categories", async (
            bool? includeInactive,
            bool? onlyRoots,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(
                new GetActivityCategoryQuery(
                    IncludeInactive: includeInactive ?? false,
                    OnlyRoots: onlyRoots ?? false),
                ct);

            return result.Match(
                categories => Results.Ok(categories),
                error => ApiResults.Problem(error));
        })
        .AllowAnonymous()
        .WithTags(Tags.ActivityCategories)
        .WithName("GetActivityCategories");
    }
}
