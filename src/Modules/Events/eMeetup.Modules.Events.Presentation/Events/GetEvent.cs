using eMeetup.Common.Domain;
using eMeetup.Common.Presentation.Endpoints;
using eMeetup.Common.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace eMeetup.Modules.Events.Presentation.Events;

internal sealed class GetEvent : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {

        app.MapGet("events/{id}", async (Guid id, ISender sender) =>
        {
            Result<Application.Events.GetEvent.EventResponse> result = await sender.Send(new Application.Events.GetEvent.GetEventQuery(id));

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization(Permissions.GetEvents)
        .WithTags(Tags.Events);
    }
}
