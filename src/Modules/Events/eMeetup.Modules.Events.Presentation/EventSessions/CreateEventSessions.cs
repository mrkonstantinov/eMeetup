using eMeetup.Common.Domain;
using eMeetup.Common.Presentation.Endpoints;
using eMeetup.Common.Presentation.Results;
using eMeetup.Modules.Events.Application.Abstractions.Authentication;
using eMeetup.Modules.Events.Application.EventSessions.CreateSession;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace eMeetup.Modules.Events.Presentation.EventSessions;

internal sealed class CreateEventSessions : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("event-sessions", async (Request request, IOrganizerContext organizerContext, ISender sender) =>
        {
            Result<Guid> result = await sender.Send(new CreateSessionCommand(
                organizerContext.OrganizerId,
                request.EventId,
                request.Title,
                request.Description,
                request.StartsAtUtc,
                request.EndsAtUtc,
                request.Locality,
                request.Address,
                request.Latitude,
                request.Longitude
                ));

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization(Permissions.ModifyEvents)
        .WithTags(Tags.Events);
    }

    internal sealed class Request
    {
        public Guid EventId { get; init; }
        public string Title { get; init; }
        public string Description { get; init; }
        public DateTime StartsAtUtc { get; init; }
        public DateTime? EndsAtUtc { get; init; }
        public string Locality { get; init; }
        public string Address { get; init; }
        public double? Latitude { get; init; }
        public double? Longitude { get; init; }
    }
}
