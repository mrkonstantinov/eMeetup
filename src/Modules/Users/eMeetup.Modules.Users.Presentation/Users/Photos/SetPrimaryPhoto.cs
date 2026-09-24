using eMeetup.Common.Domain;
using eMeetup.Common.Presentation.Endpoints;
using eMeetup.Common.Presentation.Results;
using eMeetup.Modules.Users.Application.Users.Photos.SetPrimary;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace eMeetup.Modules.Users.Presentation.Users.Photos;

internal sealed class SetPrimaryPhoto : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("users/photos/{photoId:guid}/primary", async (
            Guid photoId,
            ISender sender,
            CancellationToken ct) =>
        {
            Result result = await sender.Send(new SetPrimaryPhotoCommand(photoId), ct);

            return result.Match(
                () => Results.Ok(new { Message = "Primary photo updated successfully" }),
                error => ApiResults.Problem(error));
        })
        .RequireAuthorization()
        .WithTags(Tags.Users);
    }
}
