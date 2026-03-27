using System.Security.Claims;
using eMeetup.Common.Domain;
using eMeetup.Common.Infrastructure.Authentication;
using eMeetup.Common.Presentation.Endpoints;
using eMeetup.Common.Presentation.Results;
using eMeetup.Modules.Events.Application.Abstractions.Authentication;
using eMeetup.Modules.Events.Application.Events.CreateEvent;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace eMeetup.Modules.Events.Presentation.Events;

internal sealed class CreateEvent : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("events", async (Request request, IOrganizerContext organizerContext, ISender sender) =>
        {            
            Result<Guid> result = await sender.Send(new CreateEventCommand(
                organizerContext.OrganizerId,
                organizerContext.OrganizerName,
                request.Title,
                request.Description,
                request.Url,
                request.Tags                                         
                ));

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization(Permissions.ModifyEvents)
        .WithTags(Tags.Events);
    }

    internal sealed class Request
    {
        public string Title { get; init; }
        public string? Description { get; init; }
        public string? Url { get; init; }
        public string? Tags { get; init; }
    }
}
