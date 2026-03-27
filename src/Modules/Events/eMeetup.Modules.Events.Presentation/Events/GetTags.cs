using System.Security.Claims;
using eMeetup.Common.Domain;
using eMeetup.Common.Presentation.Endpoints;
using eMeetup.Common.Presentation.Results;
using eMeetup.Modules.Events.Application.Events.GetTags;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace eMeetup.Modules.Events.Presentation.Events;

internal sealed class GetTags : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("events/tags", async (ClaimsPrincipal claims, ISender sender) =>
        {
            Result<IReadOnlyCollection<TagGroupResponse>> result = await sender.Send(new GetTagsQuery());

            return result.Match(Results.Ok, ApiResults.Problem);
        })
    .RequireAuthorization(Permissions.GetEvents)
    .WithTags(Tags.Events);
    }

}
