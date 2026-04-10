
using eMeetup.Common.Domain;
using eMeetup.Common.Presentation.Endpoints;
using eMeetup.Common.Presentation.Results;
using eMeetup.Modules.Events.Application.EventSessions.GetSessions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace eMeetup.Modules.Events.Presentation.EventSessions;

internal sealed class GetSessions : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {

        app.MapGet("event-sessions", async (ISender sender) =>
        {
            Result<IReadOnlyCollection<SessionResponse>> result = await sender.Send(new GetSessionsQuery());

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization(Permissions.GetEvents)
        .WithTags(Tags.Events);
    }
}
