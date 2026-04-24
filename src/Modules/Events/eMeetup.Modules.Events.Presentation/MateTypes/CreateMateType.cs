using eMeetup.Common.Domain;
using eMeetup.Common.Presentation.Endpoints;
using eMeetup.Common.Presentation.Results;
using eMeetup.Modules.Events.Application.Abstractions.Authentication;
using eMeetup.Modules.Events.Application.MateTypes.CreateMateType;
using eMeetup.Modules.Events.Domain.Events;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace eMeetup.Modules.Events.Presentation.MateTypes;

internal sealed class CreateMateType : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("mate-types", async (Request request, IOrganizerContext organizerContext, ISender sender) =>
        {
            Result<Guid> result = await sender.Send(new CreateMateTypeCommand(
                organizerContext.OrganizerId,
                request.EventSessionId,
                request.Title,
                request.Description,
                request.AllocatedSlots,
                request.MinAge,
                request.MaxAge,
                request.Gender,
                request.PreferredGender,
                request.PreferredAgeRange,
                request.Priority
                ));

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization(Permissions.ModifyEvents)
        .WithTags(Tags.Events);
    }

    internal sealed class Request
    {
        public Guid EventSessionId { get; init; }
        public string Title { get; init; }
        public string? Description { get; init; }
        public int AllocatedSlots { get; init; }
        public int? MinAge { get; init; }
        public int? MaxAge { get; init; }
        public Gender? Gender { get; init; }
        public Gender? PreferredGender { get; init; }
        public string? PreferredAgeRange { get; init; }
        public int Priority { get; init; }
    }
}
