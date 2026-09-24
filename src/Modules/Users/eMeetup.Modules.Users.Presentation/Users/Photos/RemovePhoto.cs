using eMeetup.Common.Domain;
using eMeetup.Common.Presentation.Endpoints;
using eMeetup.Common.Presentation.Results;
using eMeetup.Modules.Users.Application.Users.Photos.Remove;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace eMeetup.Modules.Users.Presentation.Users.Photos;

internal sealed class RemovePhoto : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("users/photos/{photoId:guid}", async (
            Guid photoId,
            ISender sender,
            CancellationToken ct) =>
        {
            Result result = await sender.Send(new RemoveUserPhotoCommand(photoId), ct);

            return result.Match(
                () => Results.NoContent(),
                error => ApiResults.Problem(error));
        })
        .RequireAuthorization()
        .WithTags(Tags.Users);
    }
}
