using eMeetup.Common.Domain;
using eMeetup.Common.Presentation.Endpoints;
using eMeetup.Common.Presentation.Results;
using eMeetup.Modules.Users.Application.Users.Photos.Reorder;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace eMeetup.Modules.Users.Presentation.Users.Photos;

internal sealed class ReorderPhotos : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("users/photos/reorder", async (
            Dictionary<Guid, int> orderMap,
            ISender sender,
            CancellationToken ct) =>
        {
            Result result = await sender.Send(new ReorderPhotosCommand(orderMap), ct);

            return result.Match(
                () => Results.Ok(new { Message = "Photos reordered successfully" }),
                error => ApiResults.Problem(error));
        })
        .RequireAuthorization()
        .WithTags(Tags.Users);
    }
}
